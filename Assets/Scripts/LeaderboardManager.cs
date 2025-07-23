using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json; 

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    [Header("UI Top 10 (arrays de 10)")]
    public TMP_Text[] nameTexts;
    public TMP_Text[] timeTexts;
    string fileName = "top10.json";
    List<LeaderEntry> entries = new List<LeaderEntry>();
    [System.Serializable]
    public class LeaderEntry
    {
        public string nombre;
        public float tiempoRecord;
    }
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        LoadLeaderboard();
        DisplayLeaderboard();
    }
    void LoadLeaderboard()
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, LeaderEntry>>(File.ReadAllText(path));
            entries = dict.Values.OrderBy(e => e.tiempoRecord).Take(10).ToList();
        }
    }

    public void DisplayLeaderboard()
    {
        for (int i = 0; i < 10; i++)
        {
            if (i < entries.Count)
            {
                nameTexts[i].text = entries[i].nombre;
                timeTexts[i].text = entries[i].tiempoRecord.ToString("F2");
            }
            else
            {
                nameTexts[i].text = "---";
                timeTexts[i].text = "--:--";
            }
        }
    }

    public float GetBestTime()
    {
        return entries.Any() ? entries[0].tiempoRecord : 60f;
    }

    public void AddEntry(string nombre, float tiempo)
    {
        entries.Add(new LeaderEntry { nombre = nombre, tiempoRecord = tiempo });
        entries = entries.OrderBy(e => e.tiempoRecord).Take(10).ToList();
        SaveLeaderboard();
    }

    void SaveLeaderboard()
    {
        var dict = new Dictionary<string, LeaderEntry>();
        for (int i = 0; i < entries.Count; i++)
            dict[(i + 1).ToString()] = entries[i];

        File.WriteAllText(
            Path.Combine(Application.persistentDataPath, fileName),
            JsonConvert.SerializeObject(dict, Formatting.Indented)
        );
    }
}