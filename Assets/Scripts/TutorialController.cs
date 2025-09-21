using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Oculus.Interaction;                    // InteractableState
using Oculus.Interaction.Grab;               // GrabInteractable
using Oculus.Interaction.HandGrab;           // HandGrabInteractable


public class TutorialController : MonoBehaviour
{
    public static TutorialController Instance { get; private set; }

    public enum Stage { Saludo, Observacion, Controladores, Rotacion, Agarre, Cierre }

    [Header("Orden de etapas (opcional)")]
    [SerializeField] private Stage startStage = Stage.Saludo;

    [Header("UI Principal")]
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private Toggle[] checklistToggles;   // [0] Obs360, [1] Manos, [2] Rot+Pitch, [3] Agarre, [4] Cierre
    [SerializeField] private GameObject checklistRoot;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioClip saludoClip, obsClip, ctrlClip, rotClip, agarreClip, cierreClip;

    [Header("Aura / Feedback (opcional)")]
    [SerializeField] private Animator auraAnimator;

    [Header("Portal / Salida")]
    [SerializeField] private GameObject portalRoot;
    [SerializeField] private Animator portalAnimator;
    [SerializeField] private string sceneToLoad = "MainModel";
    [SerializeField] private Collider portalTrigger;   // Cualquier collider que envuelva el portal

    [Header("Refs del Rig / Jugador")]
    [SerializeField] private Transform head;               // CenterEyeAnchor
    [SerializeField] private Transform leftController;     // mano izq
    [SerializeField] private Transform rightController;    // mano der
    [SerializeField] private float nearHighlightRadius = 1.2f;

    [Header("Observación 360°")]
    [SerializeField] private float requiredYaw = 300f;
    [SerializeField] private float minAngularSpeed = 5f;
    private float obsAccumYaw;
    private Vector3 lastForwardFlat;

    [Header("Controladores (mover manos)")]
    [SerializeField] private float minHandMovement = 0.2f;
    private Vector3 l0, r0;
    private bool leftMoved, rightMoved;

    [Header("Rotación con Joystick + Mirar arriba")]
    [SerializeField] private float yawGoalDegrees = 90f;
    [SerializeField] private float pitchGoalDegrees = 30f;
    private float rotAccumYaw;
    private bool rotYawDone, rotPitchDone;

    // ==== CAMBIO: soporta cualquier tipo de Interactable (cercano, a distancia, con manos) ====
    [Header("Agarre de objetos (Interaction SDK/ISDK)")]
    [Tooltip("Arrastra aquí los GameObjects de los objetos que se pueden agarrar (balón, botella, etc.).")]
    [SerializeField] private List<GameObject> grabbables = new List<GameObject>();
    private bool anyGrabRegistered;

    private Stage current;
    private bool tutorialFinished;
    private bool _advancing = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (!head && Camera.main) head = Camera.main.transform;

        if (musicSource && musicClip)
        {
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.Play();
        }

        if (checklistRoot) checklistRoot.SetActive(true);
        EnablePortal(false);

        GoToStage(startStage);
    }

    private void Update()
    {
        if (!tutorialFinished)
        {
            switch (current)
            {
                case Stage.Saludo:
                    if (!voiceSource || !voiceSource.isPlaying) StartCoroutine(AdvanceAfter(1.0f));
                    break;

                case Stage.Observacion:
                    if (CheckObservation360()) CompleteStage(0);
                    break;

                case Stage.Controladores:
                    if (CheckControllersMoved()) CompleteStage(1);
                    break;

                case Stage.Rotacion:
                    if (CheckRotationAndLookUp()) CompleteStage(2);
                    break;

                case Stage.Agarre:
                    if (CheckGrabbedOnce()) CompleteStage(3);
                    break;
            }
        }
        else
        {
            // Detección simple por proximidad al portal 
            if (portalTrigger && head && portalTrigger.enabled &&
                portalTrigger.bounds.Contains(head.position))
            {
                TryLoadNextScene();
            }
        }
    }

    private void GoToStage(Stage st)
    {
        current = st;

        switch (st)
        {
            case Stage.Saludo:
                Say("¡Bienvenido al laboratorio! Aquí aprenderás a interactuar en Realidad Virtual y descubrirás cómo desenvolverte en este entorno.");
                PlayVoice(saludoClip);
                AuraTrigger("Salute");
                break;

            case Stage.Observacion:
                obsAccumYaw = 0f;
                lastForwardFlat = FlatForward(head ? head.forward : Vector3.forward);
                Say("Mira a tu alrededor para familiarizarte con el entorno. Cuando completes la observación, aparecerá el panel de tareas.");
                PlayVoice(obsClip);
                AuraTrigger("Talk");
                break;

            case Stage.Controladores:
                InitControllers();
                Say("Ahora aprende a usar los controladores. Mueve ambas manos para ver tus controladores virtuales.");
                PlayVoice(ctrlClip);
                AuraTrigger("Talk");
                break;

            case Stage.Rotacion:
                InitRotation();
                Say("Practiquemos girar con el joystick derecho. Luego mira hacia arriba para ver el increíble techo.");
                PlayVoice(rotClip);
                AuraTrigger("Talk");
                break;

            case Stage.Agarre:
                InitGrab();
                Say("Acércate y agarra un objeto con el gatillo o con tu mano virtual.");
                PlayVoice(agarreClip);
                AuraTrigger("Talk");
                break;

            case Stage.Cierre:
                Say("¡Excelente! Dirígete a la puerta holográfica para continuar al Gemelo Digital COLIVRI.");
                PlayVoice(cierreClip);
                AuraTrigger("ThumbsUp");
                EnablePortal(true);
                SetToggle(4, true);
                tutorialFinished = true;
                break;
        }
    }

    private IEnumerator AdvanceAfter(float seconds)
    {
        if (_advancing) yield break;
        _advancing = true;
        yield return new WaitForSeconds(seconds);
        _advancing = false;
        Advance();
    }

    private void Advance()
    {
        if (current == Stage.Cierre) return;
        GoToStage(current + 1);
    }

    private void CompleteStage(int checklistIndex)
    {
        SetToggle(checklistIndex, true);
        Advance();
    }

    private void Say(string text)
    {
        if (instructionText) instructionText.text = text;
    }

    private void PlayVoice(AudioClip clip)
    {
        if (!voiceSource || !clip) return;
        voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.Play();
    }

    private void AuraTrigger(string trigger)
    {
        if (auraAnimator && !string.IsNullOrEmpty(trigger))
            auraAnimator.SetTrigger(trigger);
    }

    private void SetToggle(int index, bool on)
    {
        if (checklistToggles == null || index < 0 || index >= checklistToggles.Length) return;
        var t = checklistToggles[index];
        if (t) t.isOn = on;
    }

    private void EnablePortal(bool on)
    {
        if (portalRoot) portalRoot.SetActive(on);
        if (portalAnimator) portalAnimator.SetBool("Open", on);
        if (portalTrigger) portalTrigger.enabled = on;
    }

    // ---------- Observación 360 ----------
    private bool CheckObservation360()
    {
        if (!head) return false;

        Vector3 now = FlatForward(head.forward);
        float delta = Vector3.SignedAngle(lastForwardFlat, now, Vector3.up);
        if (Mathf.Abs(delta) > minAngularSpeed * Time.deltaTime)
        {
            obsAccumYaw += Mathf.Abs(delta);
            lastForwardFlat = now;
        }
        return obsAccumYaw >= requiredYaw;
    }

    private Vector3 FlatForward(Vector3 fwd)
    {
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 1e-4f) fwd = Vector3.forward;
        return fwd.normalized;
    }

    // ---------- Controladores ----------
    private void InitControllers()
    {
        leftMoved = rightMoved = false;
        if (leftController) l0 = leftController.position;
        if (rightController) r0 = rightController.position;
    }

    private bool CheckControllersMoved()
    {
        if (leftController && Vector3.Distance(l0, leftController.position) >= minHandMovement) leftMoved = true;
        if (rightController && Vector3.Distance(r0, rightController.position) >= minHandMovement) rightMoved = true;
        return leftMoved && rightMoved;
    }

    // ---------- Rotación + mirar arriba ----------
    private void InitRotation()
    {
        rotAccumYaw = 0f; rotYawDone = rotPitchDone = false;
    }

    private bool CheckRotationAndLookUp()
    {
        Vector2 rs = Vector2.zero;
#if UNITY_ANDROID || UNITY_STANDALONE
        rs = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
#endif
        if (Mathf.Abs(rs.x) > 0.6f)
        {
            rotAccumYaw += 60f * Time.deltaTime; // simulación de yaw acumulado
            if (rotAccumYaw >= yawGoalDegrees) rotYawDone = true;
        }

        if (head)
        {
            float pitch = Vector3.SignedAngle(
                Vector3.ProjectOnPlane(head.forward, Vector3.right),
                head.forward,
                Vector3.right
            );
            if (pitch > pitchGoalDegrees) rotPitchDone = true;
        }
        return rotYawDone && rotPitchDone;
    }

    // ---------- Agarre (cercano / a distancia / con manos) ----------
    private void InitGrab()
    {
        anyGrabRegistered = false;
    }

    private bool CheckGrabbedOnce()
    {
        if (anyGrabRegistered) return true;
        if (grabbables == null || grabbables.Count == 0) return false;

        foreach (var go in grabbables)
        {
            if (!go) continue;
            if (IsSelected(go)) { anyGrabRegistered = true; break; }
        }
        return anyGrabRegistered;
    }

    private static bool IsSelected(GameObject go)
    {
        if (!go) return false;

        var gi = go.GetComponent<GrabInteractable>() ??
                  go.GetComponentInChildren<GrabInteractable>(true);
        if (gi && gi.State == InteractableState.Select) return true;

        var hgi = go.GetComponent<HandGrabInteractable>() ??
                  go.GetComponentInChildren<HandGrabInteractable>(true);
        if (hgi && hgi.State == InteractableState.Select) return true;

        var dgi = go.GetComponent<DistanceGrabInteractable>() ??
                  go.GetComponentInChildren<DistanceGrabInteractable>(true);
        if (dgi && dgi.State == InteractableState.Select) return true;

        var dhgi = go.GetComponent<DistanceHandGrabInteractable>() ??
                   go.GetComponentInChildren<DistanceHandGrabInteractable>(true);
        if (dhgi && dhgi.State == InteractableState.Select) return true;

        return false;
    }

    private void TryLoadNextScene()
    {
        if (!tutorialFinished) return;
        if (string.IsNullOrEmpty(sceneToLoad)) return;
        SceneManager.LoadScene(sceneToLoad);
    }
}
