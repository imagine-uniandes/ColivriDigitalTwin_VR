using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public bool debugMode = true;

    public Difficulty selectedDifficulty = Difficulty.Easy;
    public bool difficultyChosen = false;

    private void Start()
    {
        if (helpText) helpText.gameObject.SetActive(false);
        if (playButton) playButton.interactable = false;
        if (debugMode && nameInputField) nameInputField.text = "pruebas";

        easyButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Easy, easyButton));
        normalButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Normal, normalButton));
        competitiveButton.onClick.AddListener(() => OnDifficultySelected(Difficulty.Competitive, competitiveButton));
        playButton.onClick.AddListener(OnPlayClicked);

        if (nameInputField)
            nameInputField.onValueChanged.AddListener(_ => { if (helpText) helpText.gameObject.SetActive(false); });
    }

    public void OnDifficultySelected(Difficulty diff, Button btn)
    {
        selectedDifficulty = diff;
        difficultyChosen = true;

        PlayerPrefs.SetInt("difficulty", (int)diff);

        if (playButton) playButton.interactable = true;
        ResetDifficultyButtons();
        if (btn && btn.image) btn.image.color = new Color(0.3f, 0.8f, 1f);
    }

    public void ResetDifficultyButtons()
    {
        if (easyButton && easyButton.image) easyButton.image.color = Color.white;
        if (normalButton && normalButton.image) normalButton.image.color = Color.white;
        if (competitiveButton && competitiveButton.image) competitiveButton.image.color = Color.white;
    }

    public void OnPlayClicked()
    {
        if (!nameInputField)
        {
            ShowError("Falta asignar el campo de nombre.");
            return;
        }

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

        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.SetInt("difficulty", (int)selectedDifficulty);

        var mgr = PlayerDataManager.Instance;
        if (mgr.PlayerExists(playerName)) mgr.LoginExistingPlayer(playerName);
        else mgr.CreateNewPlayer(playerName);

        PlayerPrefs.SetInt("PendingAutostart", 1);
        PlayerPrefs.Save();

        SceneLoader.LoadMain();
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
