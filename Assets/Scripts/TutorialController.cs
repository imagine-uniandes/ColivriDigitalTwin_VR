using System.Collections;

using System.Collections.Generic;

using TMPro;

using UnityEngine;

using UnityEngine.SceneManagement;

using UnityEngine.UI;

using Oculus.Interaction;

using Oculus.Interaction.Grab;

using Oculus.Interaction.HandGrab;



public class TutorialController : MonoBehaviour

{

    public static TutorialController Instance { get; private set; }

    public enum Stage { Saludo, Observacion, Controladores, Teleport, Agarre, Cierre }



    [System.Serializable]

    public class StageAnimation

    {

        [Header("Etapa y referencias")]

        public Stage stage;

        public GameObject panel;

        public Animator animator;



        [Header("Triggers opcionales")]

        public string onEnterTrigger = "Enter";

        public string onCompleteTrigger = "Complete";



        [Header("Auto show/hide")]

        public bool autoShowOnEnter = true;

        public bool autoHideOnComplete = true;

        [Tooltip("Retraso para ocultar tras Complete")]

        public float hideDelay = 0.2f;

    }



    [Header("Animaciones por etapa")]

    [SerializeField] private List<StageAnimation> stageAnimations = new List<StageAnimation>();

    [SerializeField, Tooltip("Al iniciar la escena, apaga todos los paneles de etapas")]

    private bool startPanelsInactive = true;



    private StageAnimation GetSA(Stage st) => stageAnimations.Find(x => x.stage == st);



    private void PlayStageEnter(Stage st)

    {

        var sa = GetSA(st);

        if (sa == null) return;

        if (sa.autoShowOnEnter && sa.panel && !sa.panel.activeSelf) sa.panel.SetActive(true);

        if (sa.animator && !string.IsNullOrEmpty(sa.onEnterTrigger)) sa.animator.SetTrigger(sa.onEnterTrigger);

    }



    private void PlayStageComplete(Stage st)

    {

        var sa = GetSA(st);

        if (sa == null) return;

        if (sa.animator && !string.IsNullOrEmpty(sa.onCompleteTrigger)) sa.animator.SetTrigger(sa.onCompleteTrigger);

        if (sa.autoHideOnComplete && sa.panel) StartCoroutine(HidePanelAfter(sa.panel, sa.hideDelay));

    }



    private IEnumerator HidePanelAfter(GameObject go, float delay)

    {

        if (delay > 0f) yield return new WaitForSeconds(delay);

        if (go) go.SetActive(false);

    }



    private void HideAllStagePanels()

    {

        foreach (var sa in stageAnimations)

            if (sa != null && sa.panel) sa.panel.SetActive(false);

    }



    [Header("Orden de etapas")]

    [SerializeField] private Stage startStage = Stage.Saludo;



    [Header("UI Principal")]

    [SerializeField] private TMP_Text instructionText;

    [SerializeField] private Toggle[] checklistToggles;

    [SerializeField] private GameObject checklistRoot;



    [Header("UI Extra")]

    [SerializeField] private GameObject welcomePanel;

    [SerializeField] private float welcomeDuration = 5f;



    [Header("Audio")]

    [SerializeField] private AudioSource musicSource;

    [SerializeField] private AudioClip musicClip;

    [SerializeField] private AudioSource voiceSource;

    [SerializeField] private AudioClip saludoClip, obsClip, ctrlClip, agarreClip, cierreClip;



    [Header("Aura / Feedback ")]

    [SerializeField] private Animator auraAnimator;



    [Header("Portal / Salida")]

    [SerializeField] private GameObject portalRoot;

    [SerializeField] private Animator portalAnimator;

    [SerializeField] private string sceneToLoad = "MainModel";

    [SerializeField] private Collider portalTrigger;



    [Header("Refs del Rig / Jugador")]

    [SerializeField] private Transform head;

    [SerializeField] private Transform leftController;

    [SerializeField] private Transform rightController;

    [SerializeField] private float nearHighlightRadius = 1.2f;



    [Header("Teleport Settings")]

    [Tooltip("Arrastra aquí el hijo 'Locomotion' dentro de OVRCameraRigInteraction. No desactives el rig completo.")]

    [SerializeField] private GameObject locomotionRoot;

    private bool teleportEnabled = false;

    private readonly List<Behaviour> cachedTeleportBehaviours = new List<Behaviour>();

    private bool teleportCacheBuilt = false;



    [Header("Observación 360°")]

    [SerializeField] private float requiredYaw = 300f;

    [SerializeField] private float minAngularSpeed = 5f;

    private float obsAccumYaw;

    private Vector3 lastForwardFlat;

    [Header("Agarre de objetos (Interaction SDK)")]

    [SerializeField] private List<GameObject> grabbables = new List<GameObject>();

    private bool anyGrabRegistered;

    [Header("Controladores detección")]
    [SerializeField] private bool useTriggersForFace = true;
    [SerializeField] private float moveDelta = 0.15f;        
    [SerializeField] private float faceRadius = 0.28f;       
    private bool leftMoved, rightMoved;
    private bool leftNearFace, rightNearFace;
    private Vector3 l0, r0;




    [Header("Teleport (joystick adelante)")]

    [SerializeField] private float thumbstickForwardThreshold = 0.6f;

    [SerializeField] private float holdTimeToConfirm = 0.5f;

    [SerializeField] private float teleportTransitionTime = 0.35f;

    [SerializeField] private AudioClip teleportClip;



    private float thumbHoldTimer;

    private int teleportStep;

    private bool teleportDone;



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

        if (startPanelsInactive) HideAllStagePanels();

        if (welcomePanel) welcomePanel.SetActive(true);

        if (checklistRoot) checklistRoot.SetActive(false);

        StartCoroutine(HideWelcomeThenShowChecklist());

        EnablePortal(false);



        SetTeleportActive(false);



        GoToStage(startStage);

    }



    private IEnumerator HideWelcomeThenShowChecklist()

    {

        yield return new WaitForSeconds(welcomeDuration);

        if (welcomePanel) welcomePanel.SetActive(false);

        if (checklistRoot) checklistRoot.SetActive(true);

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

                    if (CheckObservation360()) CompleteCurrentStage();

                    break;



                case Stage.Controladores:

                    if (CheckControllersMoved()) CompleteCurrentStage();

                    break;



                case Stage.Teleport:

                    if (UpdateTeleport()) CompleteCurrentStage();

                    break;



                case Stage.Agarre:

                    if (CheckGrabbedOnce()) CompleteCurrentStage();

                    break;

            }

        }



        if (portalTrigger && head && portalTrigger.enabled && AreAllTogglesOn())

        {

            if (portalTrigger.bounds.Contains(head.position))

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

                Say("¡Bienvenido al laboratorio! Aquí aprenderás a interactuar en Realidad Virtual.");

                PlayVoice(saludoClip);

                AuraTrigger("Salute");

                PlayStageEnter(st);

                SetTeleportActive(false);

                break;



            case Stage.Observacion:

                obsAccumYaw = 0f;

                lastForwardFlat = FlatForward(head ? head.forward : Vector3.forward);

                Say("Mira a tu alrededor para familiarizarte con el entorno.");

                PlayVoice(obsClip);

                AuraTrigger("Talk");

                PlayStageEnter(st);

                SetTeleportActive(false);

                break;



            case Stage.Controladores:

                InitControllers();

                Say("Mueve ambas manos para ver tus controladores virtuales.");

                PlayVoice(ctrlClip);

                AuraTrigger("Talk");

                PlayStageEnter(st);

                SetTeleportActive(false);

                break;



            case Stage.Teleport:

                teleportDone = false;

                teleportStep = 0;

                thumbHoldTimer = 0f;

                Say("Empuja el joystick derecho hacia adelante y mantenlo para activar el rayo de teletransporte hacia el frente.");

                PlayVoice(teleportClip);

                AuraTrigger("Talk");

                PlayStageEnter(st);

                SetTeleportActive(true);

                break;



            case Stage.Agarre:

                InitGrab();

                Say("Apunta al objeto y aprieta el gatillo para sujetarlo. Mantén presionado para sostenerlo y suelta para dejarlo.");

                PlayVoice(agarreClip);

                AuraTrigger("Talk");

                PlayStageEnter(st);

                SetTeleportActive(true);

                break;



            case Stage.Cierre:

                Say("¡Excelente! Dirígete a la puerta holográfica para continuar.");

                PlayVoice(cierreClip);

                AuraTrigger("ThumbsUp");

                EnablePortal(true);

                SetToggle(4, true);

                tutorialFinished = true;

                PlayStageEnter(st);

                SetTeleportActive(true);

                break;

        }

    }



    private void BuildTeleportCacheIfNeeded()

    {

        if (teleportCacheBuilt) return;

        teleportCacheBuilt = true;

        cachedTeleportBehaviours.Clear();



        if (!locomotionRoot) return;



        var behaviours = locomotionRoot.GetComponentsInChildren<Behaviour>(true);

        foreach (var b in behaviours)

        {

            if (b == null) continue;

            var typeName = b.GetType().Name;

            if (typeName.Contains("Teleport"))

            {

                cachedTeleportBehaviours.Add(b);

            }

        }

    }



    private void SetTeleportActive(bool on)

    {

        teleportEnabled = on;

        BuildTeleportCacheIfNeeded();



        if (locomotionRoot)

            locomotionRoot.SetActive(on);



        if (cachedTeleportBehaviours.Count > 0)

        {

            foreach (var b in cachedTeleportBehaviours)

                if (b) b.enabled = on;

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



    private int GetChecklistIndexFor(Stage st)

    {

        switch (st)

        {

            case Stage.Observacion: return 0;

            case Stage.Controladores: return 1;

            case Stage.Teleport: return 2;

            case Stage.Agarre: return 3;

            default: return -1;

        }

    }



    private void CompleteCurrentStage()

    {

        int idx = GetChecklistIndexFor(current);

        if (idx >= 0) SetToggle(idx, true);

        if (AreAllTogglesOn()) EnablePortal(true);

        PlayStageComplete(current);

        Advance();

    }



    private bool AreAllTogglesOn()

    {

        if (checklistToggles == null || checklistToggles.Length == 0) return false;

        for (int i = 0; i < checklistToggles.Length; i++)

            if (checklistToggles[i] == null || !checklistToggles[i].isOn) return false;

        return true;

    }



    private void Say(string text) { if (instructionText) instructionText.text = text; }



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



    private void InitControllers()
    {
        leftMoved = rightMoved = false;
        leftNearFace = rightNearFace = false;

        if (leftController) l0 = leftController.position;
        if (rightController) r0 = rightController.position;
    }
    public void NotifyFaceProximity(bool isLeft)
    {
        if (isLeft) leftNearFace = true;
        else rightNearFace = true;
        if (current == Stage.Controladores && CheckControllersMoved())
            CompleteCurrentStage();
    }




    private bool CheckControllersMoved()
    {
        if (leftController && Vector3.Distance(l0, leftController.position) >= moveDelta) leftMoved = true;
        if (rightController && Vector3.Distance(r0, rightController.position) >= moveDelta) rightMoved = true;
        if (!useTriggersForFace && head)
        {
            if (leftController && Vector3.Distance(leftController.position, head.position) <= faceRadius) leftNearFace = true;
            if (rightController && Vector3.Distance(rightController.position, head.position) <= faceRadius) rightNearFace = true;
        }
        bool leftOk = leftMoved || leftNearFace;
        bool rightOk = rightMoved || rightNearFace;

        return leftOk && rightOk;
    }




    private void InitGrab() { anyGrabRegistered = false; }



    private bool CheckGrabbedOnce()

    {

        if (anyGrabRegistered) return true;

        if (grabbables == null || grabbables.Count == 0) return false;



        foreach (var go in grabbables)

        {

            if (!go) continue;



            var gi = go.GetComponent<GrabInteractable>() ?? go.GetComponentInChildren<GrabInteractable>(true);

            var hgi = go.GetComponent<HandGrabInteractable>() ?? go.GetComponentInChildren<HandGrabInteractable>(true);

            var dgi = go.GetComponent<DistanceGrabInteractable>() ?? go.GetComponentInChildren<DistanceGrabInteractable>(true);

            var dhg = go.GetComponent<DistanceHandGrabInteractable>() ?? go.GetComponentInChildren<DistanceHandGrabInteractable>(true);



            if ((gi && gi.State == InteractableState.Select) ||

                (hgi && hgi.State == InteractableState.Select) ||

                (dgi && dgi.State == InteractableState.Select) ||

                (dhg && dhg.State == InteractableState.Select))

            {

                anyGrabRegistered = true; break;

            }

        }

        return anyGrabRegistered;

    }



    private bool UpdateTeleport()

    {

        if (!teleportEnabled) return false;



        Vector2 rs = Vector2.zero;

#if UNITY_ANDROID || UNITY_STANDALONE

        rs = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

#endif

        if (rs.y > thumbstickForwardThreshold)

        {

            thumbHoldTimer += Time.deltaTime;



            if (teleportStep == 0 && thumbHoldTimer > 0.05f)

            {

                TeleportAnimTrigger("Push");

                teleportStep = 1;

            }

            if (teleportStep == 1 && thumbHoldTimer >= holdTimeToConfirm)

            {

                TeleportAnimTrigger("Charge");

                teleportStep = 2;

                StartCoroutine(TeleportBeamFinishAfter(teleportTransitionTime));

            }

        }

        else

        {

            if (teleportStep < 2)

            {

                thumbHoldTimer = 0f;

                teleportStep = 0;

            }

        }

        return teleportDone;

    }



    private IEnumerator TeleportBeamFinishAfter(float t)

    {

        yield return new WaitForSeconds(t);

        TeleportAnimTrigger("Beam");

        teleportDone = true;

    }

    private void TeleportAnimTrigger(string trig)

    {

        var sa = GetSA(Stage.Teleport);

        if (sa != null && sa.animator && !string.IsNullOrEmpty(trig))

            sa.animator.SetTrigger(trig);

    }

    private void TryLoadNextScene()

    {

        if (!AreAllTogglesOn()) return;

        if (string.IsNullOrEmpty(sceneToLoad)) return;

        SceneManager.LoadScene(sceneToLoad);

    }

}