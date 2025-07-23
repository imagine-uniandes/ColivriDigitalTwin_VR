using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerRegistrationManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Campo de entrada para el nombre del jugador")]
    public TMP_InputField nameInputField;

    [Tooltip("Texto para mostrar errores o ayuda al usuario")]
    public TextMeshProUGUI helpText;

    [Header("Botones de Dificultad")]
    public Button easyButton;
    public Button normalButton;
    public Button competitiveButton;

    [Header("Botón de Jugar")]
    public Button playButton;

    // Dificultad actualmente seleccionada
    public Difficulty selectedDifficulty = Difficulty.Easy;
    public bool difficultyChosen = false;

    public void Start()
    {
        // Ocultar ayuda y desactivar Play hasta elegir dificultad
        if (helpText != null) helpText.gameObject.SetActive(false);
        if (playButton != null) playButton.interactable = false;

        // Listeners de dificultad
        easyButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Easy, easyButton));
        normalButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Normal, normalButton));
        competitiveButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Competitive, competitiveButton));

        // Listener de Play
        playButton.onClick.AddListener(OnPlayClicked);
    }

    public void OnDifficultySelected(Difficulty diff, Button btn)
    {
        selectedDifficulty = diff;
        difficultyChosen = true;

        // Guardar en PlayerPrefs para que GameController lo use
        PlayerPrefs.SetInt("difficulty", (int)diff);

        // Activar Play y resaltar el botón
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

        // Validaciones
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

        // Guardar nombre
        PlayerPrefs.SetString("PlayerName", playerName);

        // Crear o hacer login del jugador usando la lógica original de futbol
        var mgr = PlayerDataManager.Instance;
        if (mgr.PlayerExists(playerName))
            mgr.LoginExistingPlayer(playerName);
        else
            mgr.CreateNewPlayer(playerName);

        // Ocultar panel de registro
        gameObject.SetActive(false);

        // Llamar al GameController para iniciar la partida
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
