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

    public enum TimerMode { CountUp, CountDown }

    private float elapsedTime;
    private float remainingTime;
    private bool isTimerRunning;
    private bool hasFinished;

    void Awake()
    {
        // Nunca arranca solo al cargar la escena
        isTimerRunning = false;
        hasFinished     = false;
        InitializeTimer(); 
    }

    void Update()
    {
        if (!isTimerRunning || hasFinished) return;

        if (mode == TimerMode.CountUp)
            UpdateCountUpTimer();
        else
            UpdateCountDownTimer();
    }

    public void InitializeTimer()
    {
        hasFinished = false;
        isTimerRunning = false;

        if (mode == TimerMode.CountUp)
            elapsedTime = 0f;
        else
            remainingTime = countdownTime;

        UpdateTimerDisplay();
    }

    public void ResetTimer()
    {
        InitializeTimer();
        isTimerRunning = true;
    }

    public void StartTimer()
    {
        if (!hasFinished) isTimerRunning = true;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    private void UpdateCountUpTimer()
    {
        elapsedTime += Time.deltaTime;
        UpdateTimerDisplay();
    }

    private void UpdateCountDownTimer()
    {
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            hasFinished   = true;
            isTimerRunning = false;
            OnTimerFinished?.Invoke();
        }
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        
        float t = (mode == TimerMode.CountUp) ? elapsedTime : remainingTime;
        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);

        // Color según tiempo restante
        if (mode == TimerMode.CountDown)
        {
            if (remainingTime <= 60f) timerText.color = Color.red;
            else if (remainingTime <= 300f) timerText.color = Color.yellow;
            else timerText.color = Color.white;
        }
        else
        {
            timerText.color = Color.white;
        }

        timerText.text = $"{m:00}:{s:00}";
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

    public float GetCurrentTime() => (mode == TimerMode.CountUp) ? elapsedTime : remainingTime;
    public bool HasFinished()   => hasFinished;
}