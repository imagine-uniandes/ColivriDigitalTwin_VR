using UnityEngine;
using TMPro;

public class GameStatistics : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI infoText;

    public void ShowEndGameStatistics(string playerName, float elapsedTime)
    {
        // Mismo formateo que el label del TimerDef (usa floor)
        string timeString = TimerDef.FormatMMSS(elapsedTime);

        titleText.text = $"Resultados para {playerName}";
        infoText.text = $"Tiempo: {timeString}";
        gameObject.SetActive(true);
    }
}