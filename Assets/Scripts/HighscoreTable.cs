using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class HighScoreTable : MonoBehaviour
{

    [SerializeField] private Transform entryContainer;
    [SerializeField] private Transform entryTemplate;

    private readonly List<Transform> entryTransformList = new List<Transform>();

    private void Awake()
    {
        entryTemplate.gameObject.SetActive(false);
        RefreshTable();
    }

    public void RefreshTable()
    {
        // Limpiar UI
        foreach (var t in entryTransformList)
            Destroy(t.gameObject);
        entryTransformList.Clear();

        // Obtener ranking desde PlayerDataManager
        var ranking = PlayerDataManager.Instance.GetRanking();

        // Mapear a entries y ordenarlos
        var entries = ranking.Select(p => new {
                name   = p.playerName,
                tiempo = p.GetBestSession().tiempoJugado
            })
            .OrderBy(e => e.tiempo)
            .ToList();

        // Crear filas
        float templateHeight = 30f;
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            var tr = Instantiate(entryTemplate, entryContainer);
            tr.gameObject.SetActive(true);
            var rect = tr.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, -templateHeight * i);

            // Rango
            string rank = i == 0 ? "1st"
                        : i == 1 ? "2nd"
                        : i == 2 ? "3rd"
                        : $"{i+1}th";
            tr.Find("posText")
              .GetComponent<TextMeshProUGUI>().text = rank;

            // Tiempo
            int m = Mathf.FloorToInt(e.tiempo / 60f);
            int s = Mathf.FloorToInt(e.tiempo % 60f);
            tr.Find("timeText")
              .GetComponent<TextMeshProUGUI>().text = $"{m:00}:{s:00}";

            // Nombre
            tr.Find("nameText")
              .GetComponent<TextMeshProUGUI>().text = e.name;

            // Resaltar primer lugar
            if (i == 0)
                foreach (var f in new[]{ "posText","timeText","nameText" })
                    tr.Find(f).GetComponent<TextMeshProUGUI>().color = Color.green;

            entryTransformList.Add(tr);
        }
    }
}