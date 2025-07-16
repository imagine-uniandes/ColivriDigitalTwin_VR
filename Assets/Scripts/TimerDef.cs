using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class TimerDef : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;
    [Header("Timer Settings")]
    public TimerMode mode = TimerMode.CountUp;
    public float countdownTime = 600f; 

    [Header("Events")]
    public UnityEvent OnTimerFinished;

    public enum TimerMode
    {
        CountUp,
        CountDown
    }
    private float elapsedTime = 0;
    private float remainingTime = 0;
    private bool isTimerRunning = true;
    private bool hasFinished = false;

    void Start()
    {
        InitializeTimer();
    }

    void Update()
    {
        if (!isTimerRunning || hasFinished)
            return;

        switch (mode)
        {
            case TimerMode.CountUp:
                UpdateCountUpTimer();
                break;
            case TimerMode.CountDown:
                UpdateCountDownTimer();
                break;
        }
    }

    void InitializeTimer()
    {
        switch (mode)
        {
            case TimerMode.CountUp:
                elapsedTime = 0;
                break;
            case TimerMode.CountDown:
                remainingTime = countdownTime;
                break;
        }

        UpdateTimerDisplay();
        hasFinished = false;
    }

    void UpdateCountUpTimer()
    {
        elapsedTime += Time.deltaTime;
        UpdateTimerDisplay();
    }

    void UpdateCountDownTimer()
    {
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0)
        {
            remainingTime = 0;
            hasFinished = true;
            OnTimerFinished?.Invoke();
            Debug.Log("¡Tiempo agotado!");
        }

        UpdateTimerDisplay();
    }

    void UpdateTimerDisplay()
    {
        float timeToDisplay = (mode == TimerMode.CountUp) ? elapsedTime : remainingTime;

        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);
        int milliseconds = Mathf.FloorToInt((timeToDisplay * 100) % 100);

        if (mode == TimerMode.CountDown && remainingTime <= 60f)
        {
            timerText.color = Color.red;
        }
        else if (mode == TimerMode.CountDown && remainingTime <= 300f)
        {
            timerText.color = Color.yellow;
        }
        else
        {
            timerText.color = Color.white;
        }

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void SetTimerMode(TimerMode newMode)
    {
        mode = newMode;
        InitializeTimer();
    }

    public void SetCountdownTime(float seconds)
    {
        countdownTime = seconds;
        if (mode == TimerMode.CountDown)
        {
            remainingTime = countdownTime;
            UpdateTimerDisplay();
        }
    }

    public void StartTimer()
    {
        isTimerRunning = true;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void ResetTimer()
    {
        InitializeTimer();
        isTimerRunning = true;
    }

    public float GetCurrentTime()
    {
        return (mode == TimerMode.CountUp) ? elapsedTime : remainingTime;
    }

    public bool HasFinished()
    {
        return hasFinished;
    }

    public TimerMode GetTimerMode()
    {
        return mode;
    }
}