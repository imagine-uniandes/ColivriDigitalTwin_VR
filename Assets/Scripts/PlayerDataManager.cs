using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public enum Difficulty { Easy = 0, Normal = 1, Competitive = 2 }

// guarda los datos de la partida individual
enum SessionState { None, Running, Paused }
[Serializable]
public class GameSession
{
    public string nombre;
    public float tiempoJugado;
    public string fecha;

    public GameSession()
    {
        nombre = "Partida " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        tiempoJugado = 0f;
        fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

// Clase para almacenar los datos de un jugador individual
[Serializable]
public class PlayerData
{
    public string playerName;
    [NonSerialized]
    public Dictionary<string, GameSession> partidasJugadas = new Dictionary<string, GameSession>();
    public string ultimaPartida;

    [Serializable]
    public class SessionEntry
    {
        public string key;
        public GameSession value;
    }

    public List<SessionEntry> partidasList = new List<SessionEntry>();

    public PlayerData(string name)
    {
        playerName = name;
        ultimaPartida = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        partidasJugadas = new Dictionary<string, GameSession>();
        partidasJugadas.Add("partida1", new GameSession());
        SyncFromDictionary();
    }

    public void SyncFromDictionary()
    {
        partidasList.Clear();
        foreach (var kvp in partidasJugadas)
            partidasList.Add(new SessionEntry { key = kvp.Key, value = kvp.Value });
    }

    public void SyncToDictionary()
    {
        partidasJugadas = new Dictionary<string, GameSession>();
        foreach (var entry in partidasList)
            if (entry != null && entry.key != null && entry.value != null)
                partidasJugadas[entry.key] = entry.value;
    }

    public GameSession AddNewSession()
    {
        if (partidasJugadas == null || partidasJugadas.Count == 0) SyncToDictionary();
        int num = partidasJugadas.Count + 1;
        string key = "partida" + num;
        var gs = new GameSession();
        partidasJugadas[key] = gs;
        ultimaPartida = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        SyncFromDictionary();
        return gs;
    }

    public GameSession GetCurrentSession()
    {
        if (partidasJugadas == null || partidasJugadas.Count == 0) SyncToDictionary();
        if (partidasJugadas.Count == 0) return AddNewSession();
        string key = "partida" + partidasJugadas.Count;
        return partidasJugadas.ContainsKey(key) ? partidasJugadas[key] : AddNewSession();
    }

    public int GetPuntuacionTotal()
    {
        if (partidasJugadas == null || partidasJugadas.Count == 0) SyncToDictionary();
        int total = 0;
        foreach (var p in partidasJugadas.Values)
            total += (int)p.tiempoJugado;
        return total;
    }

    public GameSession GetBestSession()
    {
        if (partidasJugadas == null || partidasJugadas.Count == 0) SyncToDictionary();
        if (partidasJugadas.Count == 0) return null;
        return partidasJugadas.Values.OrderBy(p => p.tiempoJugado).First();
    }
}

[Serializable]
public class GameData
{
    public List<PlayerData> players = new List<PlayerData>();
    public void RebuildDictionaries() => players.ForEach(p => p.SyncToDictionary());
}

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }
    public Difficulty currentDifficulty { get; private set; } = Difficulty.Easy;

    public string dataPath;
    public GameData gameData;
    public PlayerData currentPlayer;
    public GameSession currentSession;

    public void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Cargar dificultad previa
        if (PlayerPrefs.HasKey("difficulty"))
            currentDifficulty = (Difficulty)PlayerPrefs.GetInt("difficulty");

        dataPath = Path.Combine(Application.persistentDataPath, "playerdata.json");
        LoadData();
    }

    public void SetDifficulty(Difficulty diff)
    {
        currentDifficulty = diff;
        PlayerPrefs.SetInt("difficulty", (int)diff);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        if (File.Exists(dataPath))
        {
            string json = File.ReadAllText(dataPath);
            gameData = JsonUtility.FromJson<GameData>(json) ?? new GameData();
            gameData.RebuildDictionaries();
        }
        else gameData = new GameData();
    }

    public void SaveData()
    {
        gameData.RebuildDictionaries();
        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(dataPath, json);
    }

    public bool PlayerExists(string name) => !string.IsNullOrWhiteSpace(name) &&
        gameData.players.Any(p => p.playerName.Equals(name, StringComparison.OrdinalIgnoreCase));

    public bool CreateNewPlayer(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || PlayerExists(name)) return false;
        currentPlayer = new PlayerData(name);
        gameData.players.Add(currentPlayer);
        currentSession = currentPlayer.GetCurrentSession();
        SaveData();
        return true;
    }

    public bool LoginExistingPlayer(string name)
    {
        if (!PlayerExists(name)) return false;
        currentPlayer = gameData.players.First(p => p.playerName.Equals(name, StringComparison.OrdinalIgnoreCase));
        currentPlayer.SyncToDictionary();
        currentSession = currentPlayer.AddNewSession();
        SaveData();
        return true;
    }

    /// <summary>
    /// Crea o selecciona jugador, inicia sesión.
    /// </summary>
    public bool CreateOrSelectPlayer(string name)
    {
        return PlayerExists(name)
            ? LoginExistingPlayer(name)
            : CreateNewPlayer(name);
    }

    /// <summary>
    /// Graba tiempo de la sesión solo en modo competitivo.
    /// </summary>
    public void RecordSessionTime(float time)
    {
        if (currentDifficulty != Difficulty.Competitive || currentPlayer == null || currentSession == null)
            return;
        currentSession.tiempoJugado = time;
        currentSession.fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        SaveData();
    }

    public void UpdateCurrentSessionStats(float tiempoJugado, string nuevoNombrePartida) =>
        RecordSessionTime(tiempoJugado);

    public PlayerData GetCurrentPlayer() => currentPlayer;
    public GameSession GetCurrentSession() => currentSession;
    public List<PlayerData> GetRanking() =>
        gameData.players.OrderBy(p => p.GetPuntuacionTotal()).ToList();

    public void DeleteAllData()
    {
        gameData = new GameData();
        currentPlayer = null;
        currentSession = null;
        SaveData();
    }
}
