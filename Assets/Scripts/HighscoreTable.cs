using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class HighScoreTable : MonoBehaviour
{

    private Transform entryContainer;
    private Transform entryTemplate;
    private List<HighscoreEntry> highscoreEntryList;
    private List<Transform> highscoreEntryTransformList;
        private void Awake()
    {
        entryContainer = transform.Find("highscoreEntryContainer");
        entryTemplate = entryContainer.Find("highscoreEntryTemplate");
        entryTemplate.gameObject.SetActive(false);
        //AddHighscoreEntry();
        /*
        highscoreEntryList = new List<HighscoreEntry>()
        {
            new HighscoreEntry{tiempo= 58f, name = "Carla"},
            new HighscoreEntry{tiempo = 125.5f, name = "David"},
            new HighscoreEntry{tiempo = 98.2f, name = "Luis"},
            new HighscoreEntry{tiempo = 180.0f, name = "Ana"},
            new HighscoreEntry{tiempo = 120f, name = "Julian" },
            new HighscoreEntry{tiempo = 10f, name = "Juanes"},
            new HighscoreEntry{tiempo = 43f, name = "Mafe"},
            new HighscoreEntry{tiempo = 158f, name = "Alvaro"},
            new HighscoreEntry{tiempo = 122f, name = "Pepper"},
            new HighscoreEntry{tiempo = 30f, name = "Aura"}


        };
        */


        string jsonString = PlayerPrefs.GetString("highscoreTable");
        Highscores highscores = JsonUtility.FromJson<Highscores>(jsonString);


        //sort entry list by time
        for (int i = 0; i < highscores.highscoreEntryList.Count; i++)
        {
            for (int j = i + 1; j < highscores.highscoreEntryList.Count; j++)
            {
                if (highscores.highscoreEntryList[j].tiempo < highscores.highscoreEntryList[i].tiempo)
                {
                    //swap
                    HighscoreEntry tmp = highscoreEntryList[i];
                    highscores.highscoreEntryList[i] = highscores.highscoreEntryList[j];
                    highscores.highscoreEntryList[j] = tmp;
                }
            }
        }


        highscoreEntryTransformList = new List<Transform>();
        foreach (HighscoreEntry highscoreEntry in highscores.highscoreEntryList)
        {
            CreateHighscoreEntryTransform(highscoreEntry, entryContainer, highscoreEntryTransformList);
        }
        /*
        Highscores highscores = new Highscores { highscoreEntryList = highscoreEntryList };
        string json = JsonUtility.ToJson(highscores);
        PlayerPrefs.SetString("highscoreTable", json);
        PlayerPrefs.Save();
        Debug.Log(PlayerPrefs.GetString("highscoreTable"));
        */

    }

    private void CreateHighscoreEntryTransform(HighscoreEntry highscoreEntry, Transform container, List<Transform> transformList)
    {
        float templateHeight = 30f;

        Transform entryTransform = Instantiate(entryTemplate, container);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
        entryTransform.gameObject.SetActive(true);

        int rank = transformList.Count + 1;
        string rankString;
        switch (rank)
        {
            default:
                rankString = rank + "TH"; break;
            case 1: rankString = "1st"; break;
            case 2: rankString = "2nd"; break;
            case 3: rankString = "3rd"; break;

        }
        entryTransform.Find("posText").GetComponent<TextMeshProUGUI>().text = rankString;
        float tiempo = highscoreEntry.tiempo;
        int minutos = Mathf.FloorToInt(tiempo / 60f);
        int segundos = Mathf.FloorToInt(tiempo % 60f);
        string tiempoTexto = minutos.ToString("00") + ":" + segundos.ToString("00");
        entryTransform.Find("timeText").GetComponent<TextMeshProUGUI>().text = tiempoTexto;
        string name = highscoreEntry.name;
        entryTransform.Find("nameText").GetComponent<TextMeshProUGUI>().text = name;
        //poner en el panel bakcground
        //set background visible odds and evens, easier to read
        entryTransform.Find("background").gameObject.SetActive(rank % 2 == 1);
        if (rank == 1)
        {
            //highlight first
            entryTransform.Find("posText").GetComponent<TextMeshProUGUI>().color = Color.green;
            entryTransform.Find("timeText").GetComponent<TextMeshProUGUI>().color = Color.green;
            entryTransform.Find("nameText").GetComponent<TextMeshProUGUI>().color = Color.green;
            
        }
        
        transformList.Add(entryTransform);

    }
    public void AddHighscoreEntry(float tiempo, string name)
    {
        //creo un highscoreentry
        HighscoreEntry highscoreEntry = new HighscoreEntry { tiempo = tiempo, name = name };
        //load saved highscores
        string jsonString = PlayerPrefs.GetString("highscoreTable");
        Highscores highscores = JsonUtility.FromJson<Highscores>(jsonString);
        highscores.highscoreEntryList.Add(highscoreEntry);
        //save updated highscores
        string json = JsonUtility.ToJson(highscores);
        PlayerPrefs.SetString("highscoreTable", json);
        PlayerPrefs.Save();

    }




    private class Highscores
    {
        public List<HighscoreEntry> highscoreEntryList;
    }



    //represents a single high score entry
    [System.Serializable]
        private class HighscoreEntry
    {
        public float tiempo;
        public string name;
    }
}