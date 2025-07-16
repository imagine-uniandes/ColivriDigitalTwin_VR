using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerRegistrationMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject registrationPanel;
    public GameObject virtualKeyboard;
    [Header("Registration UI")]
    public TMP_InputField playerNameInput;
    public Button registerButton;
    public Button cancelButton;
    public TextMeshProUGUI statusText;
    [Header("Game Mode Buttons")]
    public Button easyModeButton;
    public Button normalModeButton;
    public Button competitiveModeButton;
    [Header("Main Menu Buttons")]
    public Button playButton;
    [Header("Settings")]
    public string gameSceneName = "GameScene";
    private GameManager.GameMode selectedMode = GameManager.GameMode.Normal;
    private string currentPlayerName = "";

    void Start()
    {
        InitializeUI();
        LoadPlayerData();
    }

    void InitializeUI()
    {
        playButton.onClick.AddListener(ShowRegistrationPanel);
        
       
        registerButton.onClick.AddListener(RegisterPlayer);
        cancelButton.onClick.AddListener(HideRegistrationPanel);
        easyModeButton.onClick.AddListener(() => SelectGameMode(GameManager.GameMode.Easy));
        normalModeButton.onClick.AddListener(() => SelectGameMode(GameManager.GameMode.Normal));
        competitiveModeButton.onClick.AddListener(() => SelectGameMode(GameManager.GameMode.Competitive));
        playerNameInput.onSelect.AddListener(OnInputFieldSelected);
        registrationPanel.SetActive(false);
        if (virtualKeyboard != null)
            virtualKeyboard.SetActive(false);
    }

    void LoadPlayerData()
    {
      
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            currentPlayerName = PlayerPrefs.GetString("PlayerName");
            playerNameInput.text = currentPlayerName;
            statusText.text = $"ultimo jugador: {currentPlayerName}";
        }
        else
        {
            statusText.text = "Nuevo jugador";
        }

        if (PlayerPrefs.HasKey("GameMode"))
        {
            selectedMode = (GameManager.GameMode)PlayerPrefs.GetInt("GameMode");
            UpdateModeButtons();
        }
    }

    public void ShowRegistrationPanel()
    {
        mainMenuPanel.SetActive(false);
        registrationPanel.SetActive(true);

        if (string.IsNullOrEmpty(currentPlayerName))
        {
            playerNameInput.text = "";
        }
    }

    public void HideRegistrationPanel()
    {
        registrationPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        if (virtualKeyboard != null)
            virtualKeyboard.SetActive(false);
    }

    public void OnInputFieldSelected(string value)
    {
        if (virtualKeyboard != null)
        {
            virtualKeyboard.SetActive(true);
        }
    }

    public void SelectGameMode(GameManager.GameMode mode)
    {
        selectedMode = mode;
        UpdateModeButtons();
        PlayerPrefs.SetInt("GameMode", (int)selectedMode);
        PlayerPrefs.Save();

        Debug.Log($"modo seleccionado: {selectedMode}");
    }

    void UpdateModeButtons()
    {
        easyModeButton.GetComponent<Image>().color = Color.white;
        normalModeButton.GetComponent<Image>().color = Color.white;
        competitiveModeButton.GetComponent<Image>().color = Color.white;
        switch (selectedMode)
        {
            case GameManager.GameMode.Easy:
                easyModeButton.GetComponent<Image>().color = Color.green;
                break;
            case GameManager.GameMode.Normal:
                normalModeButton.GetComponent<Image>().color = Color.yellow;
                break;
            case GameManager.GameMode.Competitive:
                competitiveModeButton.GetComponent<Image>().color = Color.red;
                break;
        }
    }

    public void RegisterPlayer()
    {
        string playerName = playerNameInput.text.Trim();
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.SetInt("GameMode", (int)selectedMode);
        PlayerPrefs.Save();
        currentPlayerName = playerName;
        statusText.text = $"¡Bienvenido, {playerName}!";
        statusText.color = Color.green;
        if (virtualKeyboard != null)
            virtualKeyboard.SetActive(false);
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnVirtualKeyPressed(string key)
    {
        if (key == "Backspace")
        {
            if (playerNameInput.text.Length > 0)
            {
                playerNameInput.text = playerNameInput.text.Substring(0, playerNameInput.text.Length - 1);
            }
        }
        else if (key == "Space")
        {
            playerNameInput.text += " ";
        }
        else if (key == "Enter")
        {
            RegisterPlayer();
        }
        else
        {
            playerNameInput.text += key;
        }
    }

    public void ClearPlayerData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        currentPlayerName = "";
        playerNameInput.text = "";
        statusText.text = "Datos borrados";
        statusText.color = Color.yellow;

        selectedMode = GameManager.GameMode.Normal;
        UpdateModeButtons();
    }
}