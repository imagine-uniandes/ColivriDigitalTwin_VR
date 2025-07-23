using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles UI")]
    public GameObject panelMenuInicio;    
    public GameObject panelIngresoDatos;  
    [Header("Input")]
    public TMP_InputField inputPlayerName;
    [Header("Botones de Dificultad")]
    public Button facilButton;
    public Button normalButton;
    public Button competitivoButton;
    public Button jugarButton;
    int selectedDifficulty = 1;
    void Start()
    {
        panelMenuInicio.SetActive(true);
        if(panelIngresoDatos != null) panelIngresoDatos.SetActive(false);
        
        if (PlayerPrefs.HasKey("PlayerName"))
            inputPlayerName.text = PlayerPrefs.GetString("PlayerName");
        if (PlayerPrefs.HasKey("DifficultyLevel"))
            selectedDifficulty = PlayerPrefs.GetInt("DifficultyLevel");
        
   
        facilButton.onClick.AddListener(() => SelectDifficulty(1));
        normalButton.onClick.AddListener(() => SelectDifficulty(2));
        competitivoButton.onClick.AddListener(() => SelectDifficulty(3));
        jugarButton.onClick.AddListener(PlayGame);
    }
    void SelectDifficulty(int level)
    {
        selectedDifficulty = level;
    }

    void PlayGame()
    {
        string playerName = inputPlayerName.text.Trim();
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Por favor ingresa un nombre antes de comenzar");
            return;
        }
        //guardo en playerprefs
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.SetInt("DifficultyLevel", selectedDifficulty);
        PlayerPrefs.Save();

        panelMenuInicio.SetActive(false);
        panelIngresoDatos.SetActive(true);

        GameFlowManager.Instance.StartGameSession();

    }
   

}
