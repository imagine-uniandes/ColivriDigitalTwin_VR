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
    [SerializeField]
    public List<GameObject> teleportHotspots;
    public Color helpColor = Color.green;
    private Difficulty difficulty;
    private Vector3 playerStartPos;
    
    [SerializeField] private float successDisplayDuration = 2f;

    public void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
    }

    public void Start()
    {
        //guardo posición inicial del jugador
        var player = GameObject.FindWithTag("Player");
        if (player != null) playerStartPos = player.transform.position;
        initialPanel.SetActive(true);
        registrationPanel.SetActive(false);
        instructionsPanel.SetActive(true);
        codePanel.SetActive(false);
        timerPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        statsRankingPanel.SetActive(false);
        highScorePanel.SetActive(false);
        startGameButton.onClick.AddListener(OnStartButtonClicked);
        // Restaurar dificultad previa si existe
        difficulty = (Difficulty)PlayerPrefs.GetInt("difficulty", (int)Difficulty.Easy);

        easyButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Easy));
        normalButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Normal));
        competitiveButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Competitive));
        playButton.onClick.AddListener(OnPlayClicked);
        retryButton.onClick.AddListener(OnRetryClicked);

        // Suscribirnos al exito de código
        CodeManager.OnCodeSuccessEvent += OnCodeSuccess; 
        timerDef.OnTimerFinished.AddListener(OnTimerFinished);

    }
    
    public void OnTimerFinished()
{
    // Detener el timer por si acaso
    timerDef.StopTimer();
    // Mover al jugador a start
    var player = GameObject.FindWithTag("Player");
    if (player != null) player.transform.position = playerStartPos;
    // Mostrar panel de game over
    gameOverPanel.SetActive(true);
    gameOverMessage.text = "¡Se acabó el tiempo!";
}
    public void OnStartButtonClicked()
    {
        // Ocultar el panel de inicio
        initialPanel.SetActive(false);
        // Mostrar el panel de registro (u otras pantallas iniciales)
        registrationPanel.SetActive(true);
        instructionsPanel.SetActive(true);
    }
    public void OnDestroy()
    {
        CodeManager.OnCodeSuccessEvent -= OnCodeSuccess;
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
        codePanel.SetActive(true);
        timerPanel.SetActive(true);
        gameOverPanel.SetActive(false);

        ApplyHotspotHelp(difficulty == Difficulty.Easy);

        // Reset timer
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
    }

    
    

    public void OnCodeSuccess(float elapsedTime)
    {
     
        timerDef.StopTimer();

        
        PlayerDataManager.Instance.UpdateCurrentSessionStats(
            elapsedTime, $"Partida {System.DateTime.Now:HH:mm:ss}");
        highScoreTable.RefreshTable();

        // Mostrar ranking/estadísticas y luego reset
        StartCoroutine(ShowRankingAndReset(elapsedTime));
    }
    

    private IEnumerator ShowRankingAndReset(float elapsedTime)
{
    bool comp = (difficulty == Difficulty.Competitive);

    // 1) Mostrar only competitive panels
    if (comp)
    {
        highScorePanel.SetActive(true);
        statsRankingPanel.SetActive(true);
        var stats = statsRankingPanel.GetComponent<GameStatistics>();
        stats?.ShowEndGameStatistics(
            PlayerPrefs.GetString("PlayerName"), elapsedTime);
    }

  

    // 3) Girar cámara hacia el foco (para todos los modos, si quieres)
    Quaternion originalCamRotation = Camera.main.transform.rotation;
    if (highScoreFocusPoint != null)
    {
        Vector3 dir = (highScoreFocusPoint.position - Camera.main.transform.position).normalized;
        Camera.main.transform.rotation = Quaternion.LookRotation(dir);
    }

    // 4) Permitir al jugador ver la UI (ranking o “¡Correcto!”)
    yield return new WaitForSeconds(rankingDisplayDuration);

    // 5) Fundido a negro
    if (cameraBlink != null)
        yield return cameraBlink.DoFadeIn();

    // 6) Mientras está negro, recolocar jugador y restaurar cámara
    var player = GameObject.FindWithTag("Player");
    if (player != null) player.transform.position = playerStartPos;
    Camera.main.transform.rotation = originalCamRotation;

    // 7) Ocultar TODO lo mostrado
    if (comp)
    {
        highScorePanel.SetActive(false);
        statsRankingPanel.SetActive(false);
    }
    // if (correctPanel != null) correctPanel.SetActive(false);

    // 8) Volver al registro
    ResetToRegistration();

    // 9) Fundido de vuelta
    if (cameraBlink != null)
        yield return cameraBlink.DoFadeOut();
}
    private void ResetToRegistration()
    {
        // reinicia CodeManager y timer
        FindObjectOfType<CodeManager>()?.ResetSession();
    
    // REINICIALIZA y DETIENE el TimerDef
        timerDef.InitializeTimer();

    // UI
        codePanel.SetActive(false);
        timerPanel.SetActive(false);
        registrationPanel.SetActive(true);
        instructionsPanel.SetActive(true);
    }

    public void TriggerGameOver()
    {
        timerDef.StopTimer();
        GameObject.FindWithTag("Player").transform.position = playerStartPos;
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
        timerDef.InitializeTimer();  // resetea y detiene
        ResetToRegistration();
    }

    public void ResetSession()
{
    // 1. Reinicia la lógica de CodeManager
    var cm = FindObjectOfType<CodeManager>();
    if (cm != null) cm.ResetSession();

    // 2. Reinicia y detiene el TimerDef
    timerDef.InitializeTimer();

    // 3. UI: pon en marcha los paneles de juego
    codePanel.SetActive(true);
    timerPanel.SetActive(true);
    statsRankingPanel.SetActive(false);
    gameOverPanel.SetActive(false);

    // 4. Restablece hotspots según modo (solo para fácil se muestran)
    ApplyHotspotHelp(difficulty == Difficulty.Easy);

    // 5. Oculta UI de registro/instrucciones (volverá a mostrarse desde la corutina o OnRetry)
    registrationPanel.SetActive(false);
    instructionsPanel.SetActive(false);

    // 6. Limpia el campo de nombre
    nameInput.text = "";

    // 7. Recoloca al jugador en la posición inicial
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