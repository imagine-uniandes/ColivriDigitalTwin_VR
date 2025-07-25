using System;
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
    private float countdownTime;
    private bool timerRunning;

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
        startGameButton.onClick.AddListener(OnStartButtonClicked);
        // Restaurar dificultad previa si existe
        difficulty = (Difficulty)PlayerPrefs.GetInt("difficulty", (int)Difficulty.Easy);

        easyButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Easy));
        normalButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Normal));
        competitiveButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Competitive));
        playButton.onClick.AddListener(OnPlayClicked);
        retryButton.onClick.AddListener(OnRetryClicked);

        // Suscribirnos al exito de código
        CodeManager.OnCodeSuccessEvent += OnCodeSuccess; //sospechoso
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

        timerRunning = false;

        switch (difficulty)
        {
            case Difficulty.Easy:
                foreach (var hotspot in teleportHotspots) hotspot.SetActive(true);
                countdownTime = 0f;
                break;

            case Difficulty.Normal:
                foreach (var hotspot in teleportHotspots) hotspot.SetActive(false);
                countdownTime = 0f;
                break;

            case Difficulty.Competitive:
                ApplyHotspotHelp(false);
                //al seleccionar el modo competitivo, tiene que cambiar el timer
                // Tomar el mejor tiempo menor de todos los jugadores anteriores
                //necesito cambiar el tiempo 
                // Oculta cualquier ayuda, inicializa la cuenta atrás y activa el timer

                countdownTime = 60f; // 60 segundos de cuenta atrás
                timerRunning = true;
                break;

        }
        int minutes = Mathf.FloorToInt(countdownTime / 60f);
        int seconds = Mathf.FloorToInt(countdownTime % 60f);
        timerText.text = $"{minutes:D2}:{seconds:D2}";
    }

    public void Update()
    {
        if (!timerRunning) return;

        countdownTime -= Time.deltaTime;
        if (countdownTime < 0) countdownTime = 0;
        int minutes = Mathf.FloorToInt(countdownTime / 60f);
        int seconds = Mathf.FloorToInt(countdownTime % 60f);
        timerText.text = $"{minutes:D2}:{seconds:D2}";

        if (countdownTime <= 0)
            TriggerGameOver();
    }

    public void OnCodeSuccess(float elapsedTime)
    {
        // 1. Detener el timer
        timerRunning = false;

        // 2. Guardar resultado en PlayerDataManager (y en PlayerPrefs)
        PlayerDataManager.Instance.UpdateCurrentSessionStats(elapsedTime,$"Partida {DateTime.Now:HH:mm:ss}");

        highScoreTable.RefreshTable();
        if (difficulty == Difficulty.Competitive)
        {
            var stats = statsRankingPanel.GetComponent<GameStatistics>();
            Debug.Log(stats);
            stats.ShowEndGameStatistics(PlayerPrefs.GetString("PlayerName", "Jugador"), elapsedTime);
            //tengo que llamar a highscore para guardar los mismos datos del jugador y el nombre


        }
        else
        {
            ResetSession();
        }
    }

    public void TriggerGameOver()
    {
        timerRunning = false;
        // Mover jugador a posición inicial
        var player = GameObject.FindWithTag("Player");
        if (player != null) player.transform.position = playerStartPos;
        gameOverPanel.SetActive(true);
        gameOverMessage.text = "¡Se acabo el tiempo! Vuelve a intentarlo";
    }
    //poner nuevamente el timer en reset
    public void OnRetryClicked()
    {
        gameOverPanel.SetActive(false);
        ResetSession();
    }

    public void ResetSession()
    {

        // Reinicia la logica de CodeManager 
        var cm = FindObjectOfType<CodeManager>();
        if (cm != null) cm.ResetSession();
        // Reinicia timer si es competitivo
        if (difficulty == Difficulty.Competitive)
            timerRunning = true;
        codePanel.SetActive(true);
        timerPanel.SetActive(true);
        statsRankingPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        timerText.text = "00:00";
        countdownTime = 60f;
        if (timerRunning)
            timerText.text = $"{Mathf.FloorToInt(countdownTime / 60):D2}:{Mathf.FloorToInt(countdownTime % 60):D2}";
        ApplyHotspotHelp(difficulty == Difficulty.Easy);


        registrationPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        nameInput.text = "";
        var player = GameObject.FindWithTag("Player");
        if (player != null) player.transform.position = playerStartPos;

        var feedbackPanels = FindObjectsOfType<CodeManager>();
        foreach (var panel in feedbackPanels)
        {
            panel.ResetSession();
        }


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