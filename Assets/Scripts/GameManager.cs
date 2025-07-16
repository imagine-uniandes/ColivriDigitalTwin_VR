using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField playerNameInput;
    public Button easyModeButton;
    public Button normalModeButton;
    public Button competitiveModeButton;
    public Button registerButton;
    public TextMeshProUGUI welcomeText;
    [Header("Game Objects")]
    public GameObject[] teleportHotspots; 
    public TimerDef timerScript;

    public enum GameMode
    {
        Easy,
        Normal,
        Competitive
    }

    private GameMode selectedMode = GameMode.Normal;
    private string playerName = "";

    void Start()
    {
        easyModeButton.onClick.AddListener(() => SelectGameMode(GameMode.Easy));
        normalModeButton.onClick.AddListener(() => SelectGameMode(GameMode.Normal));
        competitiveModeButton.onClick.AddListener(() => SelectGameMode(GameMode.Competitive));
        registerButton.onClick.AddListener(RegisterPlayer);
        LoadPlayerData();
        SetupScene();
    }

    public void SelectGameMode(GameMode mode)
    {
        selectedMode = mode;
        UpdateModeButtons();
        PlayerPrefs.SetInt("GameMode", (int)selectedMode);
        PlayerPrefs.Save();

        Debug.Log($"Modo seleccionado: {selectedMode}");
    }

    public void RegisterPlayer()
    {
        playerName = playerNameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Por favor ingresa un nombre válido");
            return;
        }
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.SetInt("GameMode", (int)selectedMode);
        PlayerPrefs.Save();
        welcomeText.text = $"¡Bienvenido, {playerName}!";
        SetupScene();

        Debug.Log($"Jugador registrado: {playerName}, Modo: {selectedMode}");
    }

    void LoadPlayerData()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            playerName = PlayerPrefs.GetString("PlayerName");
            playerNameInput.text = playerName;
            welcomeText.text = $"¡Bienvenido de nuevo, {playerName}!";
        }

        if (PlayerPrefs.HasKey("GameMode"))
        {
            selectedMode = (GameMode)PlayerPrefs.GetInt("GameMode");
            UpdateModeButtons();
        }
    }

    void UpdateModeButtons()
    {
        easyModeButton.GetComponent<Image>().color = Color.white;
        normalModeButton.GetComponent<Image>().color = Color.white;
        competitiveModeButton.GetComponent<Image>().color = Color.white;

        switch (selectedMode)
        {
            case GameMode.Easy:
                easyModeButton.GetComponent<Image>().color = Color.green;
                break;
            case GameMode.Normal:
                normalModeButton.GetComponent<Image>().color = Color.yellow;
                break;
            case GameMode.Competitive:
                competitiveModeButton.GetComponent<Image>().color = Color.red;
                break;
        }
    }

    void SetupScene()
    {
        switch (selectedMode)
        {
            case GameMode.Easy:
                SetupEasyMode();
                break;
            case GameMode.Normal:
                SetupNormalMode();
                break;
            case GameMode.Competitive:
                SetupCompetitiveMode();
                break;
        }
    }

    void SetupEasyMode()
    {
        Debug.Log("Configurando modo fácil");

        foreach (GameObject hotspot in teleportHotspots)
        {
            if (hotspot != null)
                hotspot.SetActive(true);
        }
        if (timerScript != null)
        {
            timerScript.SetTimerMode(TimerDef.TimerMode.CountUp);
        }
    }

    void SetupNormalMode()
    {
        Debug.Log("Configurando modo normal");
        foreach (GameObject hotspot in teleportHotspots)
        {
            if (hotspot != null)
                hotspot.SetActive(false);
        }
        if (timerScript != null)
        {
            timerScript.SetTimerMode(TimerDef.TimerMode.CountUp);
        }
    }

    void SetupCompetitiveMode()
    {
        Debug.Log("Configurando modo competitivo");
        foreach (GameObject hotspot in teleportHotspots)
        {
            if (hotspot != null)
                hotspot.SetActive(false);
        }
        if (timerScript != null)
        {
            timerScript.SetTimerMode(TimerDef.TimerMode.CountDown);
            timerScript.SetCountdownTime(600f); 
        }
    }
    public string GetPlayerName()
    {
        return playerName;
    }
    public GameMode GetCurrentGameMode()
    {
        return selectedMode;
    }

    public void ClearPlayerData()
    {
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.DeleteKey("GameMode");
        PlayerPrefs.Save();
        playerNameInput.text = "";
        welcomeText.text = "¡Bienvenido!";
        selectedMode = GameMode.Normal;
        UpdateModeButtons();
    }
}