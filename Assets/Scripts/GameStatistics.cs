using System;         
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameStatistics : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI infoText;

    public void ShowEndGameStatistics(string playerName, float elapsedTime)
    {
        TimeSpan t = TimeSpan.FromSeconds(elapsedTime);
        string timeString = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);

        titleText.text = $"Resultados para {playerName}";
        infoText.text  = $"Tiempo: {timeString}";
        gameObject.SetActive(true);
    }
}