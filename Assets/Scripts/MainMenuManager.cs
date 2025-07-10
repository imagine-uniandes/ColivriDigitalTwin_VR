using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_Dropdown difficultyDropdown;
    [SerializeField] private GameObject instruccionesPanel;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private GameObject menuPanel;
    public void StartGame()
    {

        SceneManager.LoadScene("MainModel");
    }
    public void ShowInstructions()
    {
        menuPanel.SetActive(false);
        instruccionesPanel.SetActive(true);


    }
    public void CloseInstructions()
    {
        instruccionesPanel.SetActive(false);

    }
    public void ShowLeaderboard()
    {
        menuPanel.SetActive(false);
        leaderboardPanel.SetActive(true);


    }
    public void CloseLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}