using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    [Header("Referencias")]
    public GameManager gameManager;

    [Header("Lista de textos para nombres")]
    public List<TMP_Text> nombreTexts;

    [Header("Lista de textos para tiempos")]
    public List<TMP_Text> tiempoTexts;

    void Start()
    {
        MostrarTop10();
    }

    public void MostrarTop10()
    {
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager no está asignado en LeaderboardManager.");
            return;
        }
        var playerData = gameManager.GetCurrentPlayerData();
        var topTiempos = playerData.topTimes;

        for (int i = 0; i < 10; i++)
        {
            string nombre = playerData.playerName;  
            string nombreMostrar = i < topTiempos.Count ? $"{i + 1}. {nombre}" : $"{i + 1}. -";
            string tiempoMostrar = i < topTiempos.Count ? $"{topTiempos[i]:F2} s" : "--";

            if (nombreTexts.Count > i && nombreTexts[i] != null)
                nombreTexts[i].text = nombreMostrar;

            if (tiempoTexts.Count > i && tiempoTexts[i] != null)
                tiempoTexts[i].text = tiempoMostrar;
        }
    }
}
