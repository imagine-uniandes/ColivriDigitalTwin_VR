using System;         // ← añade esto
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HighScoreTable : MonoBehaviour
{
    [SerializeField] private Transform rowContainer;    // contenedor de filas
    [SerializeField] private Transform rowTemplate;     // plantilla de una fila (inicialmente inactiva)

    private void Awake()
    {
        rowTemplate.gameObject.SetActive(false);
    }

    /// <summary>
    /// Actualiza la tabla de clasificación a partir del ranking almacenado.
    /// </summary>
    public void RefreshTable()
    {
        // Eliminar filas antiguas
        foreach (Transform child in rowContainer)
        {
            if (child != rowTemplate) Destroy(child.gameObject);
        }

        List<PlayerData> ranking = PlayerDataManager.Instance.GetRanking();
        for (int i = 0; i < ranking.Count; i++)
        {
            Transform row = Instantiate(rowTemplate, rowContainer);
            row.gameObject.SetActive(true);

            // Obtener referencias a los textos; se espera que el orden sea Posición- Nombre - Tiempo
            TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 3)
            {
                int position = i + 1;
                string playerName = ranking[i].playerName;
                float bestTime = ranking[i].BestTime;

                // Formatear el tiempo a mm:ss (puedes ajustar el formato según necesidad)
                TimeSpan t = TimeSpan.FromSeconds(bestTime);
                string timeString = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);

                texts[0].text = position.ToString();
                texts[1].text = playerName;
                texts[2].text = timeString;
            }
        }
    }
}