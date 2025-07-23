using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class StatsRankingManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Panel que contiene el diálogo de estadísticas de la última partida")]
    public GameObject playerStatsDialog;

    [Tooltip("Panel que contiene el diálogo del ranking de jugadores anteriores")]
    public GameObject rankingDialog;

    [Header("Textos de estadística de la sesión")]
    [Tooltip("TextMeshPro para mostrar el nombre de la sesión")]
    public TextMeshProUGUI sessionNameText;

    [Tooltip("TextMeshPro para mostrar el tiempo jugado en mm:ss")]
    public TextMeshProUGUI sessionTimeText;

    [Header("Textos de Ranking")]
    [Tooltip("Array de TextMeshPro para mostrar los mejores jugadores anteriores")]
    public TextMeshProUGUI[] rankingTexts;

    [Header("Configuración de Ranking")]
    [Tooltip("Número máximo de anteriores a mostrar")]
    public int numeroMejoresEntradas = 10;

    // Datos de la última sesión
    private string currentSessionName = "";
    private float currentSessionTime = 0f;
    private bool initialized = false;

    private void Start()
    {
        ValidateReferences();

        // Inicialización por defecto si no llegó datos desde GameController
        if (!initialized)
        {
            Initialize("—", 0f);
        }
    }

    private void ValidateReferences()
    {
        if (playerStatsDialog == null)
            Debug.LogError("Falta referencia a playerStatsDialog");
        if (rankingDialog == null)
            Debug.LogError("Falta referencia a rankingDialog");
        if (sessionNameText == null)
            Debug.LogError("Falta referencia a sessionNameText");
        if (sessionTimeText == null)
            Debug.LogError("Falta referencia a sessionTimeText");
        if (rankingTexts == null || rankingTexts.Length == 0)
            Debug.LogWarning("No hay campos de texto configurados para el ranking");
    }

    /// <summary>
    /// Inicializa la pantalla con los datos de la última sesión y genera el ranking.
    /// </summary>
    /// <param name="nombreSesion">Nombre asignado a la sesión (por ej. timestamp o etiqueta).</param>
    /// <param name="tiempo">Duración de la sesión en segundos.</param>
    public void Initialize(string nombreSesion, float tiempo)
    {
        currentSessionName = nombreSesion;
        currentSessionTime = tiempo;
        initialized = true;

        DisplaySessionStats();
        GenerateAndDisplayRanking();
    }

    /// <summary>
    /// Botón “Jugar de nuevo”: oculta este panel y reinicia la sesión.
    /// </summary>
    public void PlayAgain()
    {
        // Ocultar este panel completo
        gameObject.SetActive(false);

        // Reiniciar la sesión (limpiar clave y reactivar timer si corresponde)
        GameController.Instance.ResetSession();
    }

    /// <summary>
    /// Botón “Menú principal”: oculta este panel y muestra el registro.
    /// </summary>
    public void ReturnToMainMenu()
    {
        // Ocultar este panel completo
        gameObject.SetActive(false);

        // Mostrar panel de registro
        GameController.Instance.registrationPanel.SetActive(true);

        // Ocultar otros paneles de juego
        GameController.Instance.codePanel.SetActive(false);
        GameController.Instance.timerPanel.SetActive(false);
        GameController.Instance.instructionsPanel.SetActive(false);
        GameController.Instance.gameOverPanel.SetActive(false);
    }

    public void DisplaySessionStats()
    {
        playerStatsDialog.SetActive(true);

        sessionNameText.text = $"Sesión: {currentSessionName}";

        TimeSpan ts = TimeSpan.FromSeconds(currentSessionTime);
        sessionTimeText.text = $"Tiempo: {ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    public void GenerateAndDisplayRanking()
    {
        rankingDialog.SetActive(true);

        // Obtener nombre del jugador actual desde PlayerPrefs
        string currentPlayer = PlayerPrefs.GetString("PlayerName", "Jugador");

        // Obtener ranking completo
        List<PlayerData> allPlayers = PlayerDataManager.Instance.GetRanking();

        // Filtrar al jugador actual
        List<PlayerData> previousPlayers = allPlayers
            .Where(p => !p.playerName.Equals(currentPlayer, StringComparison.OrdinalIgnoreCase))
            .Take(numeroMejoresEntradas)
            .ToList();

        int entries = Math.Min(previousPlayers.Count, rankingTexts.Length);

        for (int i = 0; i < rankingTexts.Length; i++)
        {
            if (rankingTexts[i] == null) continue;

            if (i < entries)
            {
                var player = previousPlayers[i];
                int puntuacion = player.GetPuntuacionTotal();  // tu métrica actual

                rankingTexts[i].text = $"{i + 1}. {player.playerName}: {puntuacion} pts";
                rankingTexts[i].gameObject.SetActive(true);
            }
            else
            {
                rankingTexts[i].gameObject.SetActive(false);
            }
        }
    }
}
