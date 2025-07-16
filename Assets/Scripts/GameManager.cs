using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    [Header("UI Panel References")]
    public GameObject menuPrincipalPanel;
    public GameObject modoJuegoPanel;
    public GameObject gameplayUI;
    public GameObject gameOverPanel;
    public GameObject topTiemposPanel;
    [Header("Menu Principal UI")]
    public InputField playerNameInput;
    public Button jugarButton;
    [Header("Modo Juego UI")]
    public Button facilButton;
    public Button normalButton;
    public Button competitivoButton;
    public Button backToMenuButton;
    [Header("Gameplay UI")]
    public Text playerNameDisplay;
    public Text timerText;
    public Text difficultyIndicator;
    [Header("Game Over UI")]
    public Text gameOverMessage;
    public Image sadSenecaImage;
    public Button volverJugarButton;
    public Button volverMenuButton;
    [Header("Top Tiempos UI")]
    public Text topTimesText;
    public Button cerrarTopButton;
    [Header("Teleport System")]
    public GameObject[] teleportHelpObjects;
    public Material easyModeMaterial;
    public Material normalModeMaterial;
    [Header("Locomotion Control")]
    public MonoBehaviour locomotionController;
    [Header("Game Settings")]
    public float defaultGameTime = 300f;
    private PlayerData currentPlayerData;
    private float currentGameTime;
    private bool isGameActive = false;
    private bool isInMenu = true;
    private Vector3 initialPlayerPosition;
    private Quaternion initialPlayerRotation;
    private Transform playerTransform;
    public static event Action<DifficultyLevel> OnDifficultyChanged;
    public static event Action<bool> OnGameStateChanged;
    public static event Action<bool> OnMenuStateChanged;

    private void Start()
    {
        if (Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }
        else
        {
            OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
            if (cameraRig != null)
            {
                playerTransform = cameraRig.transform;
            }
        }

        LoadPlayerData();
        SetupUI();
        ShowMenuPrincipal();
    }

    private void Update()
    {
        if (isGameActive && currentPlayerData.difficultyLevel == DifficultyLevel.Competitive)
        {
            UpdateGameTimer();
        }
    }

    #region PlayerPreferences Management

    private void LoadPlayerData()
    {
        currentPlayerData = new PlayerData();
        currentPlayerData.playerName = PlayerPrefs.GetString("PlayerName", "");
        int difficultyInt = PlayerPrefs.GetInt("DifficultyLevel", 2);
        currentPlayerData.difficultyLevel = (DifficultyLevel)difficultyInt;
        currentPlayerData.bestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);
        LoadTopTimes();
        Debug.Log($"Datos cargados - Jugador: {currentPlayerData.playerName}, Dificultad: {currentPlayerData.difficultyLevel}");
    }

    private void SavePlayerData()
    {
        PlayerPrefs.SetString("PlayerName", currentPlayerData.playerName);
        PlayerPrefs.SetInt("DifficultyLevel", (int)currentPlayerData.difficultyLevel);
        PlayerPrefs.SetFloat("BestTime", currentPlayerData.bestTime);
        SaveTopTimes();
        PlayerPrefs.Save();
        Debug.Log($"Datos guardados - Jugador: {currentPlayerData.playerName}, Dificultad: {currentPlayerData.difficultyLevel}");
    }

    private void LoadTopTimes()
    {
        currentPlayerData.topTimes.Clear();

        for (int i = 0; i < 10; i++)
        {
            float time = PlayerPrefs.GetFloat($"TopTime_{i}", float.MaxValue);
            if (time != float.MaxValue)
            {
                currentPlayerData.topTimes.Add(time);
            }
        }

        Debug.Log($"Top times cargados: {currentPlayerData.topTimes.Count} registros");
    }

    private void SaveTopTimes()
    {
        for (int i = 0; i < currentPlayerData.topTimes.Count && i < 10; i++)
        {
            PlayerPrefs.SetFloat($"TopTime_{i}", currentPlayerData.topTimes[i]);
        }

        for (int i = currentPlayerData.topTimes.Count; i < 10; i++)
        {
            PlayerPrefs.DeleteKey($"TopTime_{i}");
        }
    }

    #endregion

    #region UI Management

    private void SetupUI()
    {
        if (playerNameInput != null)
        {
            playerNameInput.text = currentPlayerData.playerName;
            playerNameInput.onValueChanged.AddListener(OnPlayerNameChanged);
        }

        if (jugarButton != null)
            jugarButton.onClick.AddListener(ShowModoJuego);

        if (facilButton != null)
            facilButton.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Easy));

        if (normalButton != null)
            normalButton.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Normal));

        if (competitivoButton != null)
            competitivoButton.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Competitive));

        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(ShowMenuPrincipal);

        if (volverJugarButton != null)
            volverJugarButton.onClick.AddListener(RestartGame);

        if (volverMenuButton != null)
            volverMenuButton.onClick.AddListener(ShowMenuPrincipal);

        if (cerrarTopButton != null)
            cerrarTopButton.onClick.AddListener(CloseTopTimes);
    }

    private void ShowMenuPrincipal()
    {
        isInMenu = true;
        SetLocomotionEnabled(false);
        if (menuPrincipalPanel != null) menuPrincipalPanel.SetActive(true);
        if (modoJuegoPanel != null) modoJuegoPanel.SetActive(false);
        if (gameplayUI != null) gameplayUI.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (topTiemposPanel != null) topTiemposPanel.SetActive(false);

        OnMenuStateChanged?.Invoke(true);
        OnGameStateChanged?.Invoke(false);

        Debug.Log("Mostrando menú principal");
    }

    private void ShowModoJuego()
    {
        if (string.IsNullOrEmpty(currentPlayerData.playerName))
        {
            Debug.LogWarning("Debes ingresar un nombre para continuar");
            return;
        }

        SavePlayerData();

        if (menuPrincipalPanel != null) menuPrincipalPanel.SetActive(false);
        if (modoJuegoPanel != null) modoJuegoPanel.SetActive(true);
        Debug.Log("Mostrando selección de modo de juego");
    }

    private void SelectDifficulty(DifficultyLevel difficulty)
    {
        currentPlayerData.difficultyLevel = difficulty;
        SavePlayerData();

        Debug.Log($"Dificultad seleccionada: {difficulty}");

        StartGame();
    }

    private void ShowGameplay()
    {
        isInMenu = false;
        isGameActive = true;
        SetLocomotionEnabled(true);
        if (menuPrincipalPanel != null) menuPrincipalPanel.SetActive(false);
        if (modoJuegoPanel != null) modoJuegoPanel.SetActive(false);
        if (gameplayUI != null) gameplayUI.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (topTiemposPanel != null) topTiemposPanel.SetActive(false);
        if (playerNameDisplay != null)
            playerNameDisplay.text = $"Jugador: {currentPlayerData.playerName}";

        if (difficultyIndicator != null)
            difficultyIndicator.text = $"Modo: {GetDifficultyName(currentPlayerData.difficultyLevel)}";

        OnMenuStateChanged?.Invoke(false);
        OnGameStateChanged?.Invoke(true);

        Debug.Log("Iniciando gameplay");
    }

    private void ShowGameOver(bool timeOut = false)
    {
        isGameActive = false;
        isInMenu = true;
        SetLocomotionEnabled(false);

        if (gameplayUI != null) gameplayUI.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (gameOverMessage != null)
        {
            if (timeOut)
            {
                gameOverMessage.text = "¡Se acabó el tiempo!\nVuelve a intentarlo";
            }
            else
            {
                gameOverMessage.text = "¡Juego terminado!";
            }
        }

        if (sadSenecaImage != null)
            sadSenecaImage.gameObject.SetActive(true);

        OnMenuStateChanged?.Invoke(true);
        OnGameStateChanged?.Invoke(false);

        Debug.Log($"Game Over - Timeout: {timeOut}");
    }

    private void ShowTopTimes()
    {
        if (topTiemposPanel != null)
        {
            topTiemposPanel.SetActive(true);
            UpdateTopTimesDisplay();
        }
    }

    private void CloseTopTimes()
    {
        if (topTiemposPanel != null)
            topTiemposPanel.SetActive(false);
    }

    private void UpdateTopTimesDisplay()
    {
        if (topTimesText != null)
        {
            string topTimesDisplay = "TOP 10 MEJORES TIEMPOS\n\n";

            for (int i = 0; i < currentPlayerData.topTimes.Count && i < 10; i++)
            {
                float time = currentPlayerData.topTimes[i];
                string timeString = FormatTime(time);
                topTimesDisplay += $"{i + 1}. {currentPlayerData.playerName} - {timeString}\n";
            }

            if (currentPlayerData.topTimes.Count == 0)
            {
                topTimesDisplay += "¡Aún no hay tiempos registrados!\n¡Sé el primero en completar el juego!";
            }

            topTimesText.text = topTimesDisplay;
        }
    }

    #endregion

    #region Game Logic

    private void StartGame()
    {
        if (playerTransform != null)
        {
            initialPlayerPosition = playerTransform.position;
            initialPlayerRotation = playerTransform.rotation;
        }

        ConfigureGameForDifficulty();

        if (currentPlayerData.difficultyLevel == DifficultyLevel.Competitive)
        {
            SetupCompetitiveTimer();
        }

        ShowGameplay();
    }

    private void ConfigureGameForDifficulty()
    {
        switch (currentPlayerData.difficultyLevel)
        {
            case DifficultyLevel.Easy:
                ConfigureTeleportHelpers(true);
                if (timerText != null) timerText.gameObject.SetActive(false);
                break;

            case DifficultyLevel.Normal:
                ConfigureTeleportHelpers(false);
                if (timerText != null) timerText.gameObject.SetActive(false);
                break;

            case DifficultyLevel.Competitive:
                ConfigureTeleportHelpers(false);
                if (timerText != null) timerText.gameObject.SetActive(true);
                break;
        }

        OnDifficultyChanged?.Invoke(currentPlayerData.difficultyLevel);
    }

    private void ConfigureTeleportHelpers(bool showHelpers)
    {
        if (teleportHelpObjects != null)
        {
            foreach (GameObject helpObj in teleportHelpObjects)
            {
                if (helpObj != null)
                {
                    helpObj.SetActive(showHelpers);

                    if (showHelpers)
                    {
                        Renderer renderer = helpObj.GetComponent<Renderer>();
                        if (renderer != null && easyModeMaterial != null)
                        {
                            renderer.material = easyModeMaterial;
                        }
                    }
                }
            }
        }
    }

    private void SetupCompetitiveTimer()
    {
        if (currentPlayerData.topTimes.Count > 0)
        {
            float bestTopTime = currentPlayerData.topTimes[0];
            currentGameTime = bestTopTime;
        }
        else
        {
            currentGameTime = defaultGameTime;
        }

        Debug.Log($"Timer configurado para: {currentGameTime} segundos");
    }

    private void UpdateGameTimer()
    {
        if (currentGameTime > 0)
        {
            currentGameTime -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = $"Tiempo: {FormatTime(currentGameTime)}";
            }

            if (currentGameTime <= 0)
            {
                TimeUp();
            }
        }
    }

    private void TimeUp()
    {
        Debug.Log("¡Tiempo agotado!");

        if (playerTransform != null)
        {
            playerTransform.position = initialPlayerPosition;
            playerTransform.rotation = initialPlayerRotation;
        }

        ShowGameOver(true);
    }

    private void RestartGame()
    {
        Debug.Log("Reiniciando juego");
        StartGame();
    }

    public void CompleteGame(float completionTime)
    {
        Debug.Log($"Juego completado en {completionTime} segundos");

        if (completionTime < currentPlayerData.bestTime)
        {
            currentPlayerData.bestTime = completionTime;
            Debug.Log($"¡Nuevo mejor tiempo personal: {completionTime}!");
        }

        currentPlayerData.topTimes.Add(completionTime);
        currentPlayerData.topTimes.Sort();

        if (currentPlayerData.topTimes.Count > 10)
        {
            currentPlayerData.topTimes.RemoveAt(10);
        }

        SavePlayerData();

        StartCoroutine(ShowCelebrationAndMenu(completionTime));
    }

    private IEnumerator ShowCelebrationAndMenu(float time)
    {
        if (gameOverMessage != null)
        {
            gameOverMessage.text = $"¡Felicitaciones {currentPlayerData.playerName}!\n\nTiempo: {FormatTime(time)}\nModo: {GetDifficultyName(currentPlayerData.difficultyLevel)}";
        }

        if (sadSenecaImage != null)
            sadSenecaImage.gameObject.SetActive(false);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameplayUI != null) gameplayUI.SetActive(false);

        yield return new WaitForSeconds(5f);

        ShowMenuPrincipal();
    }

    #endregion

    #region Event Handlers

    private void OnPlayerNameChanged(string newName)
    {
        currentPlayerData.playerName = newName;
        Debug.Log($"Nombre cambiado a: {newName}");
    }

    private void SetLocomotionEnabled(bool enabled)
    {
        if (locomotionController != null)
        {
            locomotionController.enabled = enabled;
            Debug.Log($"Locomotion {(enabled ? "habilitada" : "deshabilitada")}");
        }
    }

    #endregion

    #region Utility Methods

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:00}:{seconds:00}";
    }

    private string GetDifficultyName(DifficultyLevel difficulty)
    {
        switch (difficulty)
        {
            case DifficultyLevel.Easy: return "Fácil";
            case DifficultyLevel.Normal: return "Normal";
            case DifficultyLevel.Competitive: return "Competitivo";
            default: return "Normal";
        }
    }

    #endregion

    #region Public Methods

    public PlayerData GetCurrentPlayerData()
    {
        return currentPlayerData;
    }

    public bool IsInMenu()
    {
        return isInMenu;
    }

    public bool IsGameActive()
    {
        return isGameActive;
    }

    public DifficultyLevel GetCurrentDifficulty()
    {
        return currentPlayerData.difficultyLevel;
    }

    public void ShowTopTimesPanel()
    {
        ShowTopTimes();
    }

    public void OnGameCompleted()
    {
        if (isGameActive)
        {
            float completionTime = Time.time;
            CompleteGame(completionTime);
        }
    }

    #endregion
}