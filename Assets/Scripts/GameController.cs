using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;



public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject registrationPanel;      // Contiene nombre + dificultad
    public GameObject instructionsPanel;      // Botón Play
    public GameObject codePanel;              // Panel de ingreso de clave
    public GameObject timerPanel;             // Panel con TextMeshProUGUI timerText
    public GameObject gameOverPanel;          // Panel “¡Se acabó el tiempo!”
    public GameObject statsRankingPanel;      // Panel de StatsRankingManager

    [Header("UI Elements")]
    public TMP_InputField nameInput;          // Entrada de nombre
    public Button easyButton, normalButton, competitiveButton;
    public Button playButton;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI gameOverMessage;   // “¡Se acabó el tiempo!”
    public Button retryButton;

    [Header("Gameplay Objects")]
    [SerializeField]
    public List<GameObject> teleportHotspots;    // Arrastra aquí los Renderers de tus hotspots
    public Color helpColor = Color.green;     // Color de ayuda en nivel fácil

    private Difficulty difficulty;
    private Vector3 playerStartPos;
    private float countdownTime;
    private bool timerRunning;

    public void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        // Guardar posición inicial del jugador
        var player = GameObject.FindWithTag("Player");
        if (player != null) playerStartPos = player.transform.position;

        // Inicializar UIs
        registrationPanel.SetActive(true);
        instructionsPanel.SetActive(false);
        codePanel.SetActive(false);
        timerPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        statsRankingPanel.SetActive(false);

        // Restaurar dificultad previa si existe
        difficulty = (Difficulty)PlayerPrefs.GetInt("difficulty", (int)Difficulty.Easy);
        HighlightDifficultyButton(difficulty);

        // Botones de selección
        easyButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Easy));
        normalButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Normal));
        competitiveButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Competitive));
        playButton.onClick.AddListener(OnPlayClicked);
        retryButton.onClick.AddListener(OnRetryClicked);

        // Suscribirnos al éxito de código
        CodeManager.OnCodeSuccessEvent += OnCodeSuccess;
    }

    public void OnDestroy()
    {
        CodeManager.OnCodeSuccessEvent -= OnCodeSuccess;
    }

    public void SelectDifficulty(Difficulty d)
    {
        difficulty = d;
        PlayerPrefs.SetInt("difficulty", (int)d);
        HighlightDifficultyButton(d);
    }

    public void HighlightDifficultyButton(Difficulty d)
    {
        // Simplemente tintamos el fondo del botón seleccionado
        easyButton.image.color        = d == Difficulty.Easy        ? helpColor : Color.white;
        normalButton.image.color      = d == Difficulty.Normal      ? helpColor : Color.white;
        competitiveButton.image.color = d == Difficulty.Competitive ? helpColor : Color.white;
    }

    public void OnPlayClicked()
    {
        string playerName = nameInput.text.Trim();
        if (string.IsNullOrEmpty(playerName))
        {
            // Podrías mostrar un mensaje de error aquí
            Debug.LogWarning("Debes ingresar un nombre de jugador.");
            return;
        }

        // Registrar o seleccionar jugador
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerDataManager.Instance.CreateOrSelectPlayer(playerName);

        // Ocultar UIs de registro/instrucciones
        registrationPanel.SetActive(false);
        instructionsPanel.SetActive(false);

        // Mostrar sólo ingreso de clave y timer
        codePanel.SetActive(true);
        timerPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        statsRankingPanel.SetActive(false);

        // Configurar ayudas y timer según dificultad
        switch (difficulty)
        {
            case Difficulty.Easy:
                foreach (var hotspot in teleportHotspots)
            {
                hotspot.SetActive(true);
            }
            timerRunning = false;
            break;

            case Difficulty.Normal:
                ApplyHotspotHelp(false);
                timerRunning = false;
                break;

            case Difficulty.Competitive:
                ApplyHotspotHelp(false);
                
                // Tomar el mejor tiempo (menor) de todos los jugadores anteriores
                var ranking = PlayerDataManager.Instance.GetRanking();
                if (ranking.Count > 0)
                {
                    // Cada PlayerData acumula en GetPuntuacionTotal() un int, pero como aquí guardamos tiempo,
                    // necesitamos extraer el tiempo de la mejor sesión de cada jugador.
                    // Para simplificar, asumimos que GetRanking ordena por menor tiempo:
                    countdownTime = ranking[0].GetCurrentSession().tiempoJugado;
                }
                else countdownTime = 60f; // fallback 60s
                timerRunning = true;
                break;
        }
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
        
        // Se disparó desde CodeManager cuando el jugador acierta la clave
        timerRunning = false;

        // Guardar en PlayerDataManager
        PlayerDataManager.Instance.UpdateCurrentSessionStats(elapsedTime, $"Partida {DateTime.Now:HH:mm:ss}");

        if (difficulty == Difficulty.Competitive)
        {
            // Mostrar ranking competitivo
            var stats = statsRankingPanel.GetComponent<GameStatistics>();
            stats.ShowEndGameStatistics(
            PlayerPrefs.GetString("PlayerName"), elapsedTime);

        }
        else
        {
            // En modo fácil / normal podrías simplemente reiniciar la sesión
            ResetSession();
        }
    }

    public void TriggerGameOver()
    {
        timerRunning = false;

        // Mover jugador a posición inicial
        var player = GameObject.FindWithTag("Player");
        if (player != null) player.transform.position = playerStartPos;

        // Mostrar panel de Game Over
        gameOverPanel.SetActive(true);
        gameOverMessage.text = "¡Se acabó el tiempo! Vuelve a intentarlo";
    }

    public void OnRetryClicked()
    {
        // Ocultar panel GameOver y reiniciar la misma partida
        gameOverPanel.SetActive(false);
        ResetSession();
    }

    public void ResetSession()
    {
        // Reinicia la lógica de CodeManager (limpiar dígitos y reiniciar startTime)
        var cm = FindObjectOfType<CodeManager>();
        if (cm != null) cm.ResetSession();

        // Reinicia timer si es competitivo
        if (difficulty == Difficulty.Competitive)
            timerRunning = true;

        // Mostrar solo clave y timer, ocultar ranking/GameOver
        codePanel.SetActive(true);
        timerPanel.SetActive(true);
        statsRankingPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        timerText.text = "00:00"; // Resetear timer visual
        countdownTime = 60f; // Resetear tiempo a 60s por defecto
        if (timerRunning)
            timerText.text = $"{Mathf.FloorToInt(countdownTime / 60):D2}:{Mathf.FloorToInt(countdownTime % 60):D2}";
        // Aplicar ayudas según dificultad
        ApplyHotspotHelp(difficulty == Difficulty.Easy);


        // Reiniciar UI de registro
        registrationPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        nameInput.text = ""; // Limpiar nombre ingresado
        HighlightDifficultyButton(difficulty);
        // Reiniciar posición del jugador
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
        foreach (var go in teleportHotspots){
    var rend = go.GetComponent<Renderer>();
    if (rend != null) 
        rend.material.color = show ? helpColor : Color.white;}
    }
}