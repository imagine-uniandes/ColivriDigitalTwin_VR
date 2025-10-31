using UnityEngine;

public class RegistrationFlow : MonoBehaviour
{
    [Header("Panels en RegistrationScene")]
    public GameObject initialPanel;
    public GameObject registrationPanel;
    public GameObject instructionsPanel;
    public GameObject highScorePanel;
    public HighScoreTable highScoreTable; // Asigna el del HighScorePanel

    void Start()
    {
        // ¿Venimos de MainModel tras terminar una partida?
        bool showRanking = PlayerPrefs.GetInt("ShowRankingOnReturn", 0) == 1;

        if (showRanking)
        {
            // Mostrar registro + instrucciones + ranking
            if (initialPanel) initialPanel.SetActive(false);
            if (registrationPanel) registrationPanel.SetActive(true);
            if (instructionsPanel) instructionsPanel.SetActive(true);
            if (highScorePanel) highScorePanel.SetActive(true);

            // Refresca la tabla (ver paso 3 para refresco automático también)
            if (highScoreTable) highScoreTable.RefreshTable();

            // Limpia el flag para siguientes entradas a la escena
            PlayerPrefs.SetInt("ShowRankingOnReturn", 0);
            PlayerPrefs.Save();
        }
        else
        {
            // Estado “normal” al abrir el juego por primera vez
            if (initialPanel) initialPanel.SetActive(true);
            if (registrationPanel) registrationPanel.SetActive(false);
            if (instructionsPanel) instructionsPanel.SetActive(true); // si así lo quieres
            if (highScorePanel) highScorePanel.SetActive(true);    // opcional
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