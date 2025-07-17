
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [Header("Referencias UI")]
    public List<TMP_Text> nombreTexts;  
    public List<TMP_Text> tiempoTexts;  

    [Header("Colores")]
    public Color defaultColor = Color.white;
    public Color highlightColor = Color.yellow;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

   
    public void MostrarTop10(string playerName, List<float> topTimes)
    {
        int rows = Mathf.Min(10, Mathf.Min(nombreTexts.Count, tiempoTexts.Count));
        for (int i = 0; i < rows; i++)
        {
            if (i < topTimes.Count)
            {
                nombreTexts[i].text = $"{i + 1}. {playerName}";
                tiempoTexts[i].text = $"{topTimes[i]:F2} s";
                bool isMe = nombreTexts[i].text.Contains(playerName);
                nombreTexts[i].color = isMe ? highlightColor : defaultColor;
                tiempoTexts[i].color = isMe ? highlightColor : defaultColor;
            }
            else
            {
                nombreTexts[i].text = $"{i + 1}. -";
                tiempoTexts[i].text = "--";
                nombreTexts[i].color = defaultColor;
                tiempoTexts[i].color = defaultColor;
            }
        }
    }

 
    public void HighlightPlayer()
    {
        // Si guardas internamente el playerName y topTimes, podrías
        // volver a llamar a MostrarTop10 aquí. O simplemente no lo uses.
    }
}
