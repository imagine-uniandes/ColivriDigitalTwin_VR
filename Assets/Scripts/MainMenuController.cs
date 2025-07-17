using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
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
   

    void Start()
    {
        panelMenuInicio.SetActive(true);
        panelIngresoDatos.SetActive(false);
   
        facilButton.onClick.AddListener(() => OnDifficultySelected(1));
        normalButton.onClick.AddListener(() => OnDifficultySelected(2));
        competitivoButton.onClick.AddListener(() => OnDifficultySelected(3));
    }

    public void OnPlayButton()
    {
        panelMenuInicio.SetActive(false);
        panelIngresoDatos.SetActive(true);
    }

    private void OnDifficultySelected(int level)
    {
        string playerName = inputPlayerName.text.Trim();
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Por favor ingresa un nombre antes de jugar.");
            return;
        }

        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.SetInt("DifficultyLevel", level);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainModel");
    }
   

}
