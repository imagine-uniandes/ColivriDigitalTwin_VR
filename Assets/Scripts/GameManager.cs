using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

using System.Collections;                    
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    

    [Header("Ayudas TP (nivel 1)")]
    public GameObject[] helpHighlights;

    [Header("UI Juego")]
    public TextMeshProUGUI txtPlayerName;
    public TextMeshProUGUI txtDifficulty;
    public TextMeshProUGUI txtCountdown;
    public GameObject panelTimeUp;
    public Image imgSenecaSad;
    public Button btnRetry;

    [Header("UI Top 10")]
    public GameObject top10Panel;              
    public LeaderboardManager leaderboardManager;

    private int difficulty;
    private float countdownTime;

    void Start()
    {
        

        string playerName = PlayerPrefs.GetString("PlayerName", "Player");
        difficulty = PlayerPrefs.GetInt("DifficultyLevel", 1);

        txtPlayerName.text = playerName;
        txtDifficulty.text = $"Nivel {difficulty}";
        panelTimeUp.SetActive(false);
        imgSenecaSad.enabled = false;
        foreach (var go in helpHighlights) go.SetActive(difficulty == 1);

        if (difficulty == 3)
        {
            countdownTime = Top10Manager.Instance.GetBestTime();
            StartCoroutine(CountdownCoroutine());
        }

        btnRetry.onClick.AddListener(OnRetry);

        List<float> topTimes = new List<float>();
        foreach (var rec in Top10Manager.Instance.GetAll())
            topTimes.Add(rec.tiempoRecord);

        top10Panel.SetActive(true);
        leaderboardManager.MostrarTop10(playerName, topTimes);
    }

    private System.Collections.IEnumerator CountdownCoroutine()
    {
        while (countdownTime > 0f)
        {
            txtCountdown.text = countdownTime.ToString("F1");
            yield return new WaitForSeconds(0.1f);
            countdownTime -= 0.1f;
        }
        OnTimeUp();
    }

    private void OnTimeUp()
    {
        panelTimeUp.SetActive(true);
        imgSenecaSad.enabled = true;
        PlayerResetter.Instance.ResetPosition();
    }

    private void OnRetry()
    {
        SceneManager.LoadScene("MainModel");
    }
}
