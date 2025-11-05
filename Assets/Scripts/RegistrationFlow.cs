using UnityEngine;

public class RegistrationFlow : MonoBehaviour
{
    [Header("Panels en RegistrationScene")]
    public GameObject initialPanel;
    public GameObject registrationPanel;
    public GameObject instructionsPanel;
    public GameObject highScorePanel;
    public HighScoreTable highScoreTable; 

    void Start()
    {
        bool showRanking = PlayerPrefs.GetInt("ShowRankingOnReturn", 0) == 1;

        if (showRanking)
        {
            if (initialPanel) initialPanel.SetActive(false);
            if (registrationPanel) registrationPanel.SetActive(true);
            if (instructionsPanel) instructionsPanel.SetActive(true);
            if (highScorePanel) highScorePanel.SetActive(true);

            if (highScoreTable) highScoreTable.RefreshTable();

            PlayerPrefs.SetInt("ShowRankingOnReturn", 0);
            PlayerPrefs.Save();
        }
        else
        {
            if (initialPanel) initialPanel.SetActive(true);
            if (registrationPanel) registrationPanel.SetActive(false);
            if (instructionsPanel) instructionsPanel.SetActive(true); 
            if (highScorePanel) highScorePanel.SetActive(true);   
        }



    }

    public void OnStartClicked()
    {
        ShowRegistration();
    }
    private void ShowRegistration()
    {
        if (initialPanel) initialPanel.SetActive(false);
        if (registrationPanel) registrationPanel.SetActive(true);
        if (instructionsPanel) instructionsPanel.SetActive(true);
        if (highScorePanel) highScorePanel.SetActive(true);

        if (highScoreTable) highScoreTable.RefreshTable();

    }

}