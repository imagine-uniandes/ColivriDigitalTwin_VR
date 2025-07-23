using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance { get; private set; }

    private void Awake()
    {
        // implementacion del patron Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
        public class SessionRankInfo
    {
        public string PlayerName { get; set; }
        public float SessionTime { get; set; }
        public DateTime Fecha { get; set; }

        public override string ToString()
        {
            TimeSpan ts = TimeSpan.FromSeconds(SessionTime);
            return $"{PlayerName}: {ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }

    
    public List<SessionRankInfo> GetAllSessions()
    {
        var all = new List<SessionRankInfo>();
        var mgr = PlayerDataManager.Instance;
        if (mgr == null) return all;

        //GetRanking para recorrer todos los jugadores
        foreach (var player in mgr.GetRanking())
        {
            //diccionario interno antes de iterar
            player.SyncToDictionary();
            foreach (var kvp in player.partidasJugadas)
            {
                var ses = kvp.Value;
                if (ses == null) continue;

                // fecha y añadimos el registro
                DateTime dt;
                DateTime.TryParse(ses.fecha, out dt);

                all.Add(new SessionRankInfo
                {
                    PlayerName = player.playerName,
                    SessionTime = ses.tiempoJugado,
                    Fecha = dt
                });
            }
        }
        return all;
    }
    public List<SessionRankInfo> GetTopSessions(int count)
    {
        return GetAllSessions().OrderBy(s => s.SessionTime).ThenBy(s => s.Fecha).Take(count).ToList();
    }

   
    public List<SessionRankInfo> GetTopSessionsFiltered(int count)
    {
        var topExpanded = GetTopSessions(count * 2);
        var filtered = new List<SessionRankInfo>();
        foreach (var ses in topExpanded)
        {
            bool dup = filtered.Any(e =>
                e.PlayerName == ses.PlayerName &&
                Math.Abs(e.SessionTime - ses.SessionTime) < 0.001f);

            if (!dup && filtered.Count < count)
                filtered.Add(ses);

            if (filtered.Count >= count)
                break;
        }
        return filtered;
    }
}
