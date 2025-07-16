using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    private GameManager gameManager;
    private float startTime;
    private bool isRunning = false;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(bool isGameActive)
    {
        if (isGameActive)
        {
            StartTimer();
        }
        else
        {
            StopTimer();
        }
    }

    private void StartTimer()
    {
        startTime = Time.time;
        isRunning = true;
        Debug.Log("Timer iniciado");
    }

    private void StopTimer()
    {
        isRunning = false;
        Debug.Log("Timer detenido");
    }

    public float GetCurrentTime()
    {
        if (isRunning)
        {
            return Time.time - startTime;
        }
        return 0f;
    }

    public void CompleteGame()
    {
        if (isRunning && gameManager != null)
        {
            float completionTime = GetCurrentTime();
            gameManager.CompleteGame(completionTime);
            StopTimer();
        }
    }
}