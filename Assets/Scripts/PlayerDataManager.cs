
using System;
using System.Collections.Generic;
using System.Linq;     // ← esta línea es imprescindible para usar OrderBy/Where con lambdas
using UnityEngine;

/// <summary>
/// Representa una única sesión de juego del jugador.
/// </summary>
[Serializable]
public class SessionData {
    public string sessionName;
    public float tiempoJugado;
}

/// <summary>
/// Contiene la lista de sesiones de un jugador concreto.
/// </summary>
[Serializable]
public class PlayerData
{
    public string playerName;
    public List<SessionData> sesiones = new List<SessionData>();

    //public float BestTime => sesiones.Count > 0 ? sesiones.Min(s => s.tiempoJugado) : float.MaxValue;
    public float BestTime {
        get {
            var tiemposValidos = sesiones.Where(s => s.tiempoJugado > 0f);
            return tiemposValidos.Any() ? tiemposValidos.Min(s => s.tiempoJugado) : float.MaxValue;
        }
    }

    public float LastSessionTime => sesiones.Count > 0 ? sesiones[^1].tiempoJugado : float.MaxValue;


    public SessionData GetCurrentSession() => sesiones.Count > 0 ? sesiones[^1] : null;
    
}

/// <summary>
/// Contenedor para serializar/deserializar una lista de jugadores.
/// </summary>
[Serializable]
public class PlayerDataList {
    public List<PlayerData> players = new List<PlayerData>();
}

/// <summary>
/// Gestor de datos de jugadores. Permite crear jugadores, iniciar sesiones,
/// guardar y cargar la lista de jugadores y consultar el ranking.
/// </summary>
public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }
    private const string SaveKey = "PlayerDataList";
    private PlayerDataList dataList = new PlayerDataList();
    private PlayerData currentPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
        Load();
    }

    /// <summary>
    /// Crea o selecciona un jugador y añade una nueva sesión.
    /// </summary>
    public void CreateOrSelectPlayer(string playerName)
    {
        currentPlayer = dataList.players.Find(p => p.playerName.Equals(playerName, StringComparison.OrdinalIgnoreCase));
        if (currentPlayer == null)
        {
            currentPlayer = new PlayerData { playerName = playerName };
            dataList.players.Add(currentPlayer);
        }
        // Al iniciar una partida se crea una sesión vacía que luego se actualizará
        currentPlayer.sesiones.Add(new SessionData());
        Save();
    }

    /// <summary>
    /// Actualiza la última sesión del jugador actual con el tiempo jugado y un nombre descriptivo.
    /// </summary>
    public void UpdateCurrentSessionStats(float elapsedTime, string sessionName)
    {
        if (currentPlayer == null) return;
        var session = currentPlayer.GetCurrentSession();
        if (session != null)
        {
            session.tiempoJugado = elapsedTime;
            session.sessionName = sessionName;
            Save();
        }
    }
    /// <summary>
    /// Devuelve la lista de jugadores ordenada por su mejor tiempo (menor es mejor).
    /// </summary>
    public List<PlayerData> GetRanking()
    {
        // Filtrar los jugadores que tienen al menos una sesión registrada.
        // Se ordenan por su mejor tiempo (menor a mayor) y se devuelven en una lista nueva.
        //return dataList.players.Where(player => player.sesiones != null && player.sesiones.Count > 0).OrderBy(player => player.BestTime).ToList();
        return dataList.players
                   .Where(player => player.sesiones.Any(s => s.tiempoJugado > 0f))
                   .OrderBy(player => player.BestTime)
                   .ToList();
    }

    private void Save()
    {
        string json = JsonUtility.ToJson(dataList);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            dataList = JsonUtility.FromJson<PlayerDataList>(json);
        }
    }

    public bool PlayerExists(string playerName)
    {
        return dataList.players.Exists(player => player.playerName.Equals(playerName, StringComparison.OrdinalIgnoreCase));
    }
    public void LoginExistingPlayer(string playerName)
    {
        CreateOrSelectPlayer(playerName);
    }
    public void CreateNewPlayer(string playerName)
    {
        CreateOrSelectPlayer(playerName);
    }
}