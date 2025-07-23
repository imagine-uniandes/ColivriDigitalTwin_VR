using System;
using System.Collections;
using System.Collections.Generic;
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

    // Guarda el nombre y tiempo de la sesión actual
    private string _playerName;
    private float _sessionTime;
    private bool _initialized = false;

    private void Start()
    {
        // Al inicio, oculta ambos paneles
        playerStatsDialog?.SetActive(false);
        rankingDialog?.SetActive(false);
        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (playerStatsDialog == null) Debug.LogError("Falta referencia a playerStatsDialog");
        if (rankingDialog       == null) Debug.LogError("Falta referencia a rankingDialog");
        if (playerNameText      == null) Debug.LogError("Falta referencia a playerNameText");
        if (sessionTimeText     == null) Debug.LogError("Falta referencia a sessionTimeText");
        if (rankingTexts == null || rankingTexts.Length == 0)
            Debug.LogError("Falta configurar rankingTexts (array de TMP_Text)");
    }

    /// <summary>
    /// Llamar desde GameController.OnCodeSuccess(elapsedTime) para mostrar stats y ranking.
    /// </summary>
    public void ShowEndGameStatistics(string playerName, float sessionTime)
    {
        _playerName   = playerName;
        _sessionTime  = sessionTime;
        _initialized  = true;
        StartCoroutine(DisplayStatisticsSequence());
    }

    private IEnumerator DisplayStatisticsSequence()
    {
        // 1) Estadísticas de la sesión
        DisplayPlayerStats();

        // 2) Pequeño delay para asegurar render UI
        yield return new WaitForSeconds(rankingShowDelay);

        // 3) Ranking de mejores tiempos
        GenerateAndDisplayRanking();
    }

    /// <summary>
    /// Public para poder ocultar desde otro script (p.ej. ResetSession).
    /// </summary>
    public void HideAllDialogs()
    {
        playerStatsDialog?.SetActive(false);
        rankingDialog?.SetActive(false);
    }

    // ——— Métodos internos ———

    private void DisplayPlayerStats()
    {
        // Nombre de jugador
        playerNameText.text = _playerName;

        // Formato MM:SS
        int min = (int)(_sessionTime / 60f);
        int sec = (int)(_sessionTime % 60f);
        sessionTimeText.text = $"{min:D2}:{sec:D2}";

        // Mostrar panel
        playerStatsDialog.SetActive(true);
    }

    private void GenerateAndDisplayRanking()
    {
        // Obtener ranking de sesiones previas (filtrado de duplicados)
        var topSessions = RankingManager.Instance
            .GetTopSessionsFiltered(rankingTexts.Length);

        // Llenar textos
        for (int i = 0; i < rankingTexts.Length; i++)
        {
            if (rankingTexts[i] == null) continue;

            if (i < topSessions.Count)
            {
                var ses = topSessions[i];
                // Formatear tiempo
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

        // Mostrar panel sólo si hay al menos una entrada
        rankingDialog.SetActive(topSessions.Count > 0);
    }
}
