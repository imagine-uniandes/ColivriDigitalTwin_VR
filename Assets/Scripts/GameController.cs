using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject initialPanel;
    public GameObject registrationPanel;
    public GameObject instructionsPanel;
    public GameObject codePanel;
    public GameObject timerPanel;
    public GameObject gameOverPanel;
    [SerializeField] private HighScoreTable highScoreTable;
    public GameObject statsRankingPanel;
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
    public TextMeshProUGUI timerText;
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

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
    }

    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null) playerStartPos = player.transform.position;

        initialPanel.SetActive(true);
        registrationPanel.SetActive(false);
        instructionsPanel.SetActive(true);
        codePanel.SetActive(false);
        timerPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        statsRankingPanel.SetActive(false);
        highScorePanel.SetActive(true);

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
        timerDef.StopTimer();
        var player = GameObject.FindWithTag("Player");
        if (player != null) player.transform.position = playerStartPos;
        gameOverPanel.SetActive(true);
        gameOverMessage.text = "¡Se acabó el tiempo!";
    }

    public void OnStartButtonClicked()
    {
        initialPanel.SetActive(false);
        registrationPanel.SetActive(true);
        instructionsPanel.SetActive(true);
        highScorePanel.SetActive(true);
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

        registrationPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        highScorePanel.SetActive(false);
        codePanel.SetActive(true);
        timerPanel.SetActive(true);
        gameOverPanel.SetActive(false);

        ApplyHotspotHelp(difficulty == Difficulty.Easy);
        extraTimeGiven = false;

        switch (difficulty)
        {
            case Difficulty.Easy:
                foreach (var hotspot in teleportHotspots) hotspot.SetActive(true);
                timerDef.SetTimerMode(TimerDef.TimerMode.CountUp);
                break;
            case Difficulty.Normal:
                foreach (var hotspot in teleportHotspots) hotspot.SetActive(false);
                timerDef.SetTimerMode(TimerDef.TimerMode.CountUp);
                break;
            case Difficulty.Competitive:
                float firstTime = GetFirstPlaceTimeOrDefault(60f);
                timerDef.SetTimerMode(TimerDef.TimerMode.CountDown);
                timerDef.SetCountdownTime(firstTime);
                break;
        }

        timerDef.ResetTimer();

        var cm = FindObjectOfType<CodeManager>();
        cm?.BeginSession(shuffle: false);
    }

    public void OnCodeSuccess(float elapsedTime)
    {
        if (audioSource != null && successClip != null)
            audioSource.PlayOneShot(successClip);

        timerDef.StopTimer();

        PlayerDataManager.Instance.UpdateCurrentSessionStats(
            elapsedTime, $"Partida {System.DateTime.Now:HH:mm:ss}");
        highScoreTable.RefreshTable();

        // Mostrar estadísticas y luego volver al registro (todas las dificultades)
        StartCoroutine(ShowStatsAndReturnToRegister(elapsedTime));
    }

    private IEnumerator ShowStatsAndReturnToRegister(float elapsedTime)
    {
        // 1) Mostrar stats por jugador
        statsRankingPanel.SetActive(true);
        var stats = statsRankingPanel.GetComponent<GameStatistics>();
        stats?.ShowEndGameStatistics(PlayerPrefs.GetString("PlayerName"), elapsedTime);

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

        // 7) Avanzar al siguiente reto y preparar UI de pistas
        if (retoLoader != null)
        {
            bool avanzado = retoLoader.LoadNextReto();
            retoLoader.UpdatePistasUI();

            // Si quieres reiniciar al primero cuando se acaben:
            if (!avanzado) { retoLoader.ResetSequence(shuffle: false); retoLoader.UpdatePistasUI(); }
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
        timerDef.InitializeTimer();

        // UI
        codePanel.SetActive(false);
        timerPanel.SetActive(false);
        registrationPanel.SetActive(true);
        instructionsPanel.SetActive(true);
        highScorePanel.SetActive(true);
    }

    public void TriggerGameOver()
    {
        timerDef.StopTimer();
        var player = GameObject.FindWithTag("Player");
        if (player != null) player.transform.position = playerStartPos;
        gameOverPanel.SetActive(true);
        gameOverMessage.text = "¡Se acabó el tiempo!";
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
        gameOverPanel.SetActive(false);
        timerDef.InitializeTimer();
        ResetToRegistration();
    }

    public void ResetSession()
    {
        var cm = FindObjectOfType<CodeManager>();
        if (cm != null) cm.ResetSession();

        timerDef.InitializeTimer();

        codePanel.SetActive(true);
        timerPanel.SetActive(true);
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

    public enum Difficulty
    {
        Easy,
        Normal,
        Competitive
    }
}
