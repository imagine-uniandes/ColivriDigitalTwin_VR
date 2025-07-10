using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
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
        for (int i = 0; i < 10; i++)
        {
            string nombre = PlayerPrefs.GetString("Nombre" + i, "-");
            float tiempo = PlayerPrefs.GetFloat("Tiempo" + i, -1f);

            if (nombreTexts.Count > i)
                nombreTexts[i].text = $"{i + 1}. {nombre}";
            if (tiempoTexts.Count > i)
                tiempoTexts[i].text = tiempo >= 0 ? $"{tiempo:F2} s" : "--";
        }
    }
}