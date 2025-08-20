using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum Difficulty { Easy = 0, Normal = 1, Competitive = 2 }

public class PlayerRegistrationManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField nameInputField;
    public TextMeshProUGUI helpText;

    [Header("Botones de Dificultad")]
    public Button easyButton;
    public Button normalButton;
    public Button competitiveButton;

    [Header("Botón de Jugar")]
    public Button playButton;

    public bool debugMode;

    public Difficulty selectedDifficulty = Difficulty.Easy;
    public bool difficultyChosen = false;

    private void Start()
    {
        // Al iniciar, ocultar el mensaje de ayuda y desactivar el Play
        if (helpText != null) helpText.gameObject.SetActive(false);
        if (playButton != null) playButton.interactable = false;
        if (debugMode) nameInputField.text = "pruebas";
        easyButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Easy, easyButton));
        normalButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Normal, normalButton));
        competitiveButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Competitive, competitiveButton));

        playButton.onClick.AddListener(OnPlayClicked);
    }

   
  
    
    public void OnDifficultySelected(Difficulty diff, Button btn)
    {
        selectedDifficulty = diff;
        difficultyChosen = true;

        // Guardar la dificultad en PlayerPrefs para que GameController la pueda leer
        PlayerPrefs.SetInt("difficulty", (int)diff);
        // Activar el botón Play y resaltar visualmente el botón elegido
        playButton.interactable = true;
        ResetDifficultyButtons();
        btn.image.color = new Color(0.3f, 0.8f, 1f);
    }

    public void ResetDifficultyButtons()
    {
        easyButton.image.color = Color.white;
        normalButton.image.color = Color.white;
        competitiveButton.image.color = Color.white;
    }
    public void OnPlayClicked()
    {
        string playerName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            ShowError("El nombre no puede estar vacío.");
            return;
        }

        if (!difficultyChosen)
        {
            ShowError("Seleccione un nivel de dificultad.");
            return;
        }

        // Guardar el nombre para otros componentes
        PlayerPrefs.SetString("PlayerName", playerName);

        // Registrar o seleccionar jugador usando PlayerDataManager
        var mgr = PlayerDataManager.Instance;
        if (mgr.PlayerExists(playerName))
            mgr.LoginExistingPlayer(playerName);
        else
            mgr.CreateNewPlayer(playerName);

        // Ocultar el panel de registro 
        gameObject.SetActive(false);

        // Delegar en GameController la preparación de la partida
        GameController.Instance.OnPlayClicked();
    }
    public void ShowError(string message)
    {
        if (helpText != null)
        {
            helpText.text = message;
            helpText.gameObject.SetActive(true);
        }
    }

}