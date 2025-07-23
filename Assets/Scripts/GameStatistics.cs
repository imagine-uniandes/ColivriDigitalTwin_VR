using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using TMPro;

public class GameStatistics : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Panel que contiene las estadísticas de la última sesión")]
    public GameObject playerStatsDialog;

    [Tooltip("Panel que contiene el ranking de mejores tiempos anteriores")]
    public GameObject rankingDialog;

    [Header("Textos de Estadísticas de la Sesión")]
    [Tooltip("TextMeshPro para mostrar el nombre del jugador")]
    public TextMeshProUGUI playerNameText;

    [Tooltip("TextMeshPro para mostrar el tiempo de la sesión en MM:SS")]
    public TextMeshProUGUI sessionTimeText;

    [Header("Textos de Ranking")]
    [Tooltip("Array de TextMeshPro para mostrar los mejores tiempos")]
    public TextMeshProUGUI[] rankingTexts;

    [Header("Configuración")]
    [Tooltip("Delay breve antes de desplegar el ranking")]
    public float rankingShowDelay = 0.1f;

    // Valores de la sesion a mostrar
    private string _playerName;
    private float _sessionTime;

    private void Start()
    {
        if (playerStatsDialog == null)
            Debug.LogError("Falta referencia a playerStatsDialog");
        if (rankingDialog == null)
            Debug.LogError("Falta referencia a rankingDialog");
        if (playerNameText == null)
            Debug.LogError("Falta referencia a playerNameText");
        if (sessionTimeText == null)
            Debug.LogError("Falta referencia a sessionTimeText");
        if (rankingTexts == null || rankingTexts.Length == 0)
            Debug.LogError("Falta configurar rankingTexts (array de TMP_Text)");
        playerStatsDialog.SetActive(false);
        rankingDialog.SetActive(false);
    }

   
    public void ShowEndGameStatistics(string playerName, float sessionTime)
    {
        _playerName  = playerName;
        _sessionTime = sessionTime;
        StartCoroutine(DisplayStatisticsSequence());
    }

    private IEnumerator DisplayStatisticsSequence()
    {
        DisplayPlayerStats();

        // pequeño delay para refrescar la UI
        yield return new WaitForSeconds(rankingShowDelay);

        GenerateAndDisplayRanking();
    }

    
    public void HideAllDialogs()
    {
        playerStatsDialog.SetActive(false);
        rankingDialog.SetActive(false);
    }

    private void DisplayPlayerStats()
    {
        playerNameText.text = _playerName;
        int minutes = (int)(_sessionTime / 60f);
        int seconds = (int)(_sessionTime % 60f);
        sessionTimeText.text = $"{minutes:D2}:{seconds:D2}";
        playerStatsDialog.SetActive(true);
    }

    private void GenerateAndDisplayRanking()
    {
        var topSessions = RankingManager.Instance
            .GetTopSessionsFiltered(rankingTexts.Length);

        for (int i = 0; i < rankingTexts.Length; i++)
        {
            if (rankingTexts[i] == null) continue;

            if (i < topSessions.Count)
            {
                var ses = topSessions[i];
                int m = (int)(ses.SessionTime / 60f);
                int s = (int)(ses.SessionTime % 60f);
                rankingTexts[i].text = $"{i+1}. {ses.PlayerName}: {m:D2}:{s:D2}";
                rankingTexts[i].gameObject.SetActive(true);
            }
            else
            {
                rankingTexts[i].gameObject.SetActive(false);
            }
        }

        // Solo muestro el panel de ranking si hay al menos una entrada
        rankingDialog.SetActive(topSessions.Count > 0);
    }
}
