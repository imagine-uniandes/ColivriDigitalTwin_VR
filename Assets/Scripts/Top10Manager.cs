
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json; 

[Serializable]
public class TimeRecord
{
    public string nombre;
    public float tiempoRecord;
}

public class Top10Manager : MonoBehaviour
{
    public static Top10Manager Instance { get; private set; }
    private List<TimeRecord> records = new List<TimeRecord>();
    private string path;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            path = Path.Combine(Application.persistentDataPath, "top10.json");
            LoadRecords();
        }
        else Destroy(gameObject);
    }

    private void LoadRecords()
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, TimeRecord>>(json);
            records = dict.Values.ToList();
            records = records.OrderBy(r => r.tiempoRecord).Take(10).ToList();
        }
    }

    public void AddRecord(TimeRecord newRec)
    {
        records.Add(newRec);
        records = records.OrderBy(r => r.tiempoRecord).Take(10).ToList();
        SaveRecords();
    }

    private void SaveRecords()
    {
        var dict = new Dictionary<string, TimeRecord>();
        for (int i = 0; i < records.Count; i++)
            dict[(i + 1).ToString()] = records[i];
        string json = JsonConvert.SerializeObject(dict, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public float GetBestTime()
    {
        return records.Count > 0 ? records[0].tiempoRecord : 60f; 
    }

    public List<TimeRecord> GetAll() => new List<TimeRecord>(records);
}
