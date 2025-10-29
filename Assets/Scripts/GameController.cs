using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject initialPanel;
    public GameObject registrationPanel;
    public GameObject instructionsPanel;
    public GameObject codePanel;
    // [OBSOLETO] public GameObject timerPanel; // ahora usamos los dos de abajo
    public GameObject gameOverPanel;
    [SerializeField] private HighScoreTable highScoreTable;
    public GameObject statsRankingPanel;

    [Header("Timer Panels")]
    [SerializeField] private GameObject timerPanelDefault;        // Fácil / Normal
    [SerializeField] private GameObject timerPanelCompetitive;    // Competitivo
    [SerializeField] private TextMeshProUGUI timerTextDefault;    // Label del panel default
    [SerializeField] private TextMeshProUGUI timerTextCompetitive;// Label del panel competitivo

    [Header("Reto Loader")]
    [SerializeField] private RetoLoader retoLoader;

    [Header("Ranking Display")]
    public GameObject highScorePanel;
    public Transform highScoreFocusPoint;
    public float rankingDisplayDuration = 3f;
    [SerializeField] private CameraBlink cameraBlink;

    [Header("Timer")]
    [SerializeField] private TimerDef timerDef;
    private bool extraTimeGiven = false;

    [Header("UI Elements")]
    public TMP_InputField nameInput;
    public Button easyButton, normalButton, competitiveButton;
    public Button playButton;
    public Button startGameButton;
    // [OBSOLETO] public TextMeshProUGUI timerText; // lo maneja TimerDef con BindLabel
    public TextMeshProUGUI gameOverMessage;
    public Button retryButton;

    [Header("Gameplay Objects")]
    [SerializeField] public List<GameObject> teleportHotspots;
    public Color helpColor = Color.green;
    private Difficulty difficulty;
    private Vector3 playerStartPos;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip successClip;
    [SerializeField] private float successDisplayDuration = 2f;

    [Header("Audio (Suspenso)")]
    [SerializeField] private AudioSource suspenseSource;
    [SerializeField] private AudioClip suspenseClip;
    [SerializeField, Range(0f, 1f)] private float suspenseVolume = 0.6f;
    private bool suspenseActive = false;
    private Coroutine suspenseGuardRoutine;

    [Header("Locomotion / Teleport")]
    [Tooltip("Arrastra aquí el objeto raíz del Teleport/Locomotion (p.ej. 'LocomotionController (Interaction)')")]
    [SerializeField] private GameObject locomotionControllerRoot;

    [Tooltip("componentes a activar/desactivar junto con el teleport")]
    [SerializeField] private List<MonoBehaviour> teleportScriptsToToggle = new List<MonoBehaviour>();

    [Header("Animators a controlar")]
    [Tooltip("Arrastra aquí los Animator de los modelos que quieres pausar/reproducir.")]
    [SerializeField] private List<Animator> animatorsToControl = new List<Animator>();

    [Tooltip("se reproducirá el estado por defecto del Animator.")]
    [SerializeField] private string stateToPlayOnSuccess = "";

    [Header("Escenas")]
    [SerializeField] private string tutorialSceneName = "TutorialScene";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
        EnsureSuspenseSource();
    }

    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null) playerStartPos = player.transform.position;

        initialPanel.SetActive(true);
        registrationPanel.SetActive(false);
        instructionsPanel.SetActive(true);
        codePanel.SetActive(false);
        // timerPanel.SetActive(false); // obsoleto
        gameOverPanel.SetActive(false);
        statsRankingPanel.SetActive(false);
        highScorePanel.SetActive(true);

        // Oculta ambos paneles de timer al inicio
        if (timerPanelDefault) timerPanelDefault.SetActive(false);
        if (timerPanelCompetitive) timerPanelCompetitive.SetActive(false);

        // === DESACTIVAR TELEPORT EN MENÚ INICIAL ===
        SetTeleportEnabled(false);
        FreezeModelAnimators();

        startGameButton.onClick.AddListener(OnStartButtonClicked);

        difficulty = (Difficulty)PlayerPrefs.GetInt("difficulty", (int)Difficulty.Easy);
        easyButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Easy));
        normalButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Normal));
        competitiveButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Competitive));
        playButton.onClick.AddListener(OnPlayClicked);
        retryButton.onClick.AddListener(OnRetryClicked);

        CodeManager.OnCodeSuccessEvent += OnCodeSuccess;
        timerDef.OnTimerFinished.AddListener(OnTimerFinished);
    }

    private void OnDestroy()
    {
        CodeManager.OnCodeSuccessEvent -= OnCodeSuccess;
    }

    public Difficulty GetCurrentDifficulty() => difficulty;

    public void OnTimerFinished()
    {
        StopSuspenseBed();
        timerDef.StopTimer();
        var player = GameObject.FindWithTag("Player");
        if (player != null) player.transform.position = playerStartPos;
        gameOverPanel.SetActive(true);
        gameOverMessage.text = "¡Se acabó el tiempo!";

        // Volvemos a bloquear teleport en pantallas de resultado/menú
        SetTeleportEnabled(false);
        FreezeModelAnimators();
    }

    public void OnStartButtonClicked()
    {
        initialPanel.SetActive(false);
        registrationPanel.SetActive(true);
        instructionsPanel.SetActive(true);
        highScorePanel.SetActive(true);

        // En el registro también debe estar desactivado
        SetTeleportEnabled(false);
        FreezeModelAnimators();
    }

    public void SelectDifficulty(Difficulty d)
    {
        difficulty = d;
        PlayerPrefs.SetInt("difficulty", (int)d);
    }

    public void OnPlayClicked()
    {
        string playerName = nameInput.text.Trim();
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Debes ingresar un nombre de jugador.");
            return;
        }

        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerDataManager.Instance.CreateOrSelectPlayer(playerName);

        // === ACTIVAR TELEPORT AL COMENZAR LA PARTIDA ===
        SetTeleportEnabled(true);
        FreezeModelAnimators();

        registrationPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        highScorePanel.SetActive(false);
        codePanel.SetActive(true);
        gameOverPanel.SetActive(false);

        // Apaga ambos paneles de timer antes de decidir
        if (timerPanelDefault) timerPanelDefault.SetActive(false);
        if (timerPanelCompetitive) timerPanelCompetitive.SetActive(false);

        ApplyHotspotHelp(difficulty == Difficulty.Easy);
        extraTimeGiven = false;

        // Timer según dificultad
        switch (difficulty)
        {
            case Difficulty.Easy:
                foreach (var hotspot in teleportHotspots) hotspot.SetActive(true);
                if (timerPanelDefault) timerPanelDefault.SetActive(true);
                if (timerDef && timerTextDefault) timerDef.BindLabel(timerTextDefault);
                if (timerDef)
                {
                    timerDef.SetUrgentColorsEnabled(true);                // por si algún día usas countdown aquí
                    timerDef.SetColorOverride(false, Color.white);        // sin override global
                    timerDef.SetTimerMode(TimerDef.TimerMode.CountUp);
                }
                break;

            case Difficulty.Normal:
                foreach (var hotspot in teleportHotspots) hotspot.SetActive(false);
                if (timerPanelDefault) timerPanelDefault.SetActive(true);
                if (timerDef && timerTextDefault) timerDef.BindLabel(timerTextDefault);
                if (timerDef)
                {
                    timerDef.SetUrgentColorsEnabled(true);
                    timerDef.SetColorOverride(false, Color.white);
                    timerDef.SetTimerMode(TimerDef.TimerMode.CountUp);
                }
                break;

            case Difficulty.Competitive:
                if (timerPanelCompetitive) timerPanelCompetitive.SetActive(true);
                if (timerDef && timerTextCompetitive) timerDef.BindLabel(timerTextCompetitive);
                if (timerDef)
                {
                    float firstTime = GetFirstPlaceTimeOrDefault(60f);
                    timerDef.SetUrgentColorsEnabled(false);               // SIN rojos/amarillos
                    timerDef.SetColorOverride(true, Color.white);         // fuerza blanco
                    timerDef.SetTimerMode(TimerDef.TimerMode.CountDown);
                    timerDef.SetCountdownTime(firstTime);
                }
                break;
        }

        if (timerDef) timerDef.ResetTimer();

        // === CLAVE: configurar RetoLoader y fijar reto de la sesión ===
        if (retoLoader == null) retoLoader = FindObjectOfType<RetoLoader>();
        if (retoLoader != null)
        {
            retoLoader.ConfigureModeByDifficulty(difficulty);
            retoLoader.PrepareForNewSession(); // fija Reto 1 / aleatorio / mantiene secuencia
        }
        else
        {
            Debug.LogError("GameController: RetoLoader no asignado ni encontrado en escena.");
        }

        var cm = FindObjectOfType<CodeManager>();
        cm?.BeginSession(shuffle: false);
        StartSuspenseBed();
    }

    public void OnCodeSuccess(float elapsedTimeParam)
    {
        PlayModelAnimatorsFromStart();

        StopSuspenseBed();
        if (timerDef) timerDef.StopTimer();

        float tFromTimer = timerDef ? timerDef.GetTimeForStats() : elapsedTimeParam;
        float elapsedTime = tFromTimer;
        if (audioSource != null && successClip != null)
            audioSource.PlayOneShot(successClip);

        timerDef.StopTimer();

        PlayerDataManager.Instance.UpdateCurrentSessionStats(
            elapsedTime, $"Partida {System.DateTime.Now:HH:mm:ss}");
        highScoreTable.RefreshTable();

        StartCoroutine(ShowStatsAndReturnToRegister(elapsedTime));
    }

    private IEnumerator ShowStatsAndReturnToRegister(float elapsedTime)
    {
        // 1) Mostrar stats por jugador
        statsRankingPanel.SetActive(true);
        /*
        var stats = statsRankingPanel.GetComponent<GameStatistics>();
        stats?.ShowEndGameStatistics(PlayerPrefs.GetString("PlayerName"), elapsedTime);
        */
        var stats = statsRankingPanel.GetComponentInChildren<GameStatistics>(true);
        if (stats == null)
        {
            Debug.LogError("GameController: GameStatistics no encontrado en StatsRankingPanel ni en sus hijos.");
        }
        else
        {
            stats.ShowEndGameStatistics(PlayerPrefs.GetString("PlayerName"), elapsedTime);
            Debug.Log($"[Stats] Tiempo mostrado: {elapsedTime:F2} -> {TimerDef.FormatMMSS(elapsedTime)}");
        }

        // 2) Orientar cámara (opcional)
        Quaternion originalCamRotation = Quaternion.identity;
        if (Camera.main != null)
        {
            originalCamRotation = Camera.main.transform.rotation;
            if (highScoreFocusPoint != null)
            {
                Vector3 dir = (highScoreFocusPoint.position - Camera.main.transform.position).normalized;
                Camera.main.transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        // 3) Permitir ver las stats
        yield return new WaitForSeconds(rankingDisplayDuration);

        // 4) Fade a negro
        if (cameraBlink != null)
            yield return cameraBlink.DoFadeIn();

        // 5) Recolocar jugador y restaurar cámara
        var player = GameObject.FindWithTag("Player");
        if (player != null) player.transform.position = playerStartPos;
        if (Camera.main != null) Camera.main.transform.rotation = originalCamRotation;

        // 6) Ocultar stats
        statsRankingPanel.SetActive(false);

        // 7) Avanzar reto SOLO en competitivo
        if (retoLoader != null)
        {
            if (difficulty == Difficulty.Competitive)
            {
                bool avanzado = retoLoader.LoadNextReto();
                retoLoader.UpdatePistasUI();

                // Si quieres reiniciar al primero cuando se acaben:
                if (!avanzado) { retoLoader.ResetSequence(shuffle: false); retoLoader.UpdatePistasUI(); }
            }
            // En Fácil y Normal: no avanzamos (cada sesión es 1 reto y vuelve al registro).
        }

        // 8) Volver al registro
        ResetToRegistration();

        // 9) Fade de vuelta
        if (cameraBlink != null)
            yield return cameraBlink.DoFadeOut();
    }

    private void ResetToRegistration()
    {
        FindObjectOfType<CodeManager>()?.ResetSession();

        // REINICIALIZA y DETIENE el TimerDef
        if (timerDef) timerDef.InitializeTimer();
        StopSuspenseBed();

        // UI
        codePanel.SetActive(false);

        // Apaga ambos paneles de timer
        if (timerPanelDefault) timerPanelDefault.SetActive(false);
        if (timerPanelCompetitive) timerPanelCompetitive.SetActive(false);

        registrationPanel.SetActive(true);
        instructionsPanel.SetActive(true);
        highScorePanel.SetActive(true);

        // Al volver al registro, bloquear teleport
        SetTeleportEnabled(false);
        FreezeModelAnimators();
    }

    public void TriggerGameOver()
    {
        StopSuspenseBed();
        timerDef.StopTimer();
        var player = GameObject.FindWithTag("Player");
        if (player != null) player.transform.position = playerStartPos;
        gameOverPanel.SetActive(true);
        gameOverMessage.text = "¡Se acabó el tiempo!";

        // En game over estamos en UI -> teleport off
        SetTeleportEnabled(false);
        FreezeModelAnimators();
    }

    private float CalculateExtraTime()
    {
        var r = PlayerDataManager.Instance.GetRanking();
        if (r.Count >= 2)
            return Mathf.Max(0f, r[1].BestTime - r[0].BestTime);
        return 0f;
    }

    private float GetFirstPlaceTimeOrDefault(float @default)
    {
        var r = PlayerDataManager.Instance.GetRanking();
        return (r.Count > 0) ? r[0].BestTime : @default;
    }

    public void OnRetryClicked()
    {
        StopSuspenseBed();
        gameOverPanel.SetActive(false);
        if (timerDef) timerDef.InitializeTimer();
        ResetToRegistration();
        // ResetToRegistration ya desactiva el teleport
    }

    public void ResetSession()
    {
        var cm = FindObjectOfType<CodeManager>();
        if (cm != null) cm.ResetSession();

        if (timerDef) timerDef.InitializeTimer();

        codePanel.SetActive(true);
        // Mostrar el panel correcto según dificultad y re-enlazar label
        if (timerPanelDefault) timerPanelDefault.SetActive(false);
        if (timerPanelCompetitive) timerPanelCompetitive.SetActive(false);

        switch (difficulty)
        {
            case Difficulty.Easy:
            case Difficulty.Normal:
                if (timerPanelDefault) timerPanelDefault.SetActive(true);
                if (timerDef && timerTextDefault) timerDef.BindLabel(timerTextDefault);
                if (timerDef)
                {
                    timerDef.SetUrgentColorsEnabled(true);
                    timerDef.SetColorOverride(false, Color.white);
                    timerDef.SetTimerMode(TimerDef.TimerMode.CountUp);
                }
                break;

            case Difficulty.Competitive:
                if (timerPanelCompetitive) timerPanelCompetitive.SetActive(true);
                if (timerDef && timerTextCompetitive) timerDef.BindLabel(timerTextCompetitive);
                if (timerDef)
                {
                    timerDef.SetUrgentColorsEnabled(false);
                    timerDef.SetColorOverride(true, Color.white);
                    timerDef.SetTimerMode(TimerDef.TimerMode.CountDown);
                }
                break;
        }

        statsRankingPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        ApplyHotspotHelp(difficulty == Difficulty.Easy);

        registrationPanel.SetActive(false);
        instructionsPanel.SetActive(false);

        nameInput.text = "";

        var player = GameObject.FindWithTag("Player");
        if (player != null)
            player.transform.position = playerStartPos;
    }

    public void ApplyHotspotHelp(bool show)
    {
        foreach (var go in teleportHotspots)
        {
            var rend = go.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = show ? helpColor : Color.white;
        }
    }

    private void EnsureSuspenseSource()
    {
        if (suspenseSource == null)
        {
            suspenseSource = gameObject.AddComponent<AudioSource>();
            suspenseSource.playOnAwake = false;
            suspenseSource.loop = true;        // loop por defecto
            suspenseSource.spatialBlend = 0f;  // 2D
            suspenseSource.volume = suspenseVolume;
            suspenseSource.dopplerLevel = 0f;  // por si algún día usas 3D
        }
    }

    private void StartSuspenseBed()
    {
        if (suspenseClip == null) return;
        EnsureSuspenseSource();

        suspenseSource.clip = suspenseClip;
        suspenseSource.loop = true;
        suspenseSource.spatialBlend = 0f;
        suspenseSource.ignoreListenerPause = true;
        if (suspenseSource.isPlaying) suspenseSource.Stop();
        suspenseSource.Play();

        suspenseActive = true;

        // Guardia: relanza si algo externo lo detiene
        if (suspenseGuardRoutine == null)
            suspenseGuardRoutine = StartCoroutine(SuspenseGuard());
    }

    private void StopSuspenseBed()
    {
        suspenseActive = false;

        if (suspenseGuardRoutine != null)
        {
            StopCoroutine(suspenseGuardRoutine);
            suspenseGuardRoutine = null;
        }

        if (suspenseSource != null && suspenseSource.isPlaying)
            suspenseSource.Stop();
    }

    private IEnumerator SuspenseGuard()
    {
        while (suspenseActive)
        {
            if (suspenseSource != null && suspenseClip != null)
            {
                if (!suspenseSource.isPlaying)
                {
                    suspenseSource.loop = true;
                    suspenseSource.Play();
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    // ===================== NUEVO: MÉTODO CENTRAL =====================
    private void SetTeleportEnabled(bool enabled)
    {
        if (locomotionControllerRoot)
            locomotionControllerRoot.SetActive(enabled);

        if (teleportScriptsToToggle != null)
        {
            foreach (var s in teleportScriptsToToggle)
            {
                if (s) s.enabled = enabled;
            }
        }
    }
    private void FreezeModelAnimators()
    {
        foreach (var a in animatorsToControl)
        {
            if (!a) continue;
            // Deja el Animator en el frame 0 de su estado por defecto y lo pausa
            a.speed = 0f;
            a.Rebind();
            a.Update(0f);
        }
    }

    private void PlayModelAnimatorsFromStart()
    {
        foreach (var a in animatorsToControl)
        {
            if (!a) continue;
            // Arranca desde el principio
            if (!string.IsNullOrEmpty(stateToPlayOnSuccess))
                a.Play(stateToPlayOnSuccess, 0, 0f); // play estado indicado
            else
            {
                // Si no se indicó un estado, usa el estado por defecto (ya está en t=0 por Rebind)
            }
            a.speed = 1f;
        }
    }
    public void ReturnToTutorialScene()
    {
        StopSuspenseBed();
        SetTeleportEnabled(false);


        Destroy(transform.root.gameObject);

        SceneManager.LoadScene(tutorialSceneName, LoadSceneMode.Single);
    }

    public enum Difficulty
    {
        Easy,
        Normal,
        Competitive
    }
}
