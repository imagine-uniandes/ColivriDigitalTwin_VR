using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class TimerDef : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] public TextMeshProUGUI timerText;

    [Header("Timer Settings")]
    [SerializeField] public TimerMode mode = TimerMode.CountUp;
    [SerializeField] public float countdownTime = 600f;

    [Header("Events")]
    public UnityEvent OnTimerFinished;

    public enum TimerMode { CountUp, CountDown }

    private float elapsedTime;
    private float remainingTime;
    private bool isTimerRunning;
    private bool hasFinished;

    [Header("Color Behavior")]
    [Tooltip("Si es true, en modo CountDown cambia a amarillo/rojo según umbrales.")]
    [SerializeField] private bool useUrgentColors = true;

    [Tooltip("Si es true, ignora toda la lógica de colores y usa overrideColor.")]
    [SerializeField] private bool overrideAllTextColor = false;

    [SerializeField] private Color overrideColor = Color.white;

    [SerializeField] private float yellowThresholdSeconds = 300f; // 5 min
    [SerializeField] private float redThresholdSeconds = 60f;  // 1 min

    private void Awake()
    {
        isTimerRunning = false;
        hasFinished = false;
        InitializeTimer();
    }

    private void Update()
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
            remainingTime = Mathf.Max(0f, countdownTime);

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

    public void SetTimerMode(TimerMode newMode)
    {
        mode = newMode;
        InitializeTimer();
    }

    public void SetCountdownTime(float seconds)
    {
        countdownTime = Mathf.Max(0f, seconds);
        if (mode == TimerMode.CountDown)
        {
            remainingTime = countdownTime;
            UpdateTimerDisplay();
        }
    }

    public void BindLabel(TextMeshProUGUI newLabel)
    {
        timerText = newLabel;
        UpdateTimerDisplay();
    }

    public void SetUrgentColorsEnabled(bool enabled)
    {
        useUrgentColors = enabled;
        UpdateTimerDisplay();
    }

    public void SetColorOverride(bool enabled, Color color)
    {
        overrideAllTextColor = enabled;
        overrideColor = color;
        UpdateTimerDisplay();
    }

    public float GetCurrentTime() => (mode == TimerMode.CountUp) ? elapsedTime : remainingTime;
    public bool HasFinished() => hasFinished;


    private void UpdateCountUpTimer()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime < 0f) elapsedTime = 0f;
        UpdateTimerDisplay();
    }

    private void UpdateCountDownTimer()
    {
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            hasFinished = true;
            isTimerRunning = false;
            OnTimerFinished?.Invoke();
        }
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (!timerText) return;

        float t = (mode == TimerMode.CountUp) ? elapsedTime : remainingTime;
        if (t < 0f) t = 0f;

        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);

        // --- Color ---
        if (overrideAllTextColor)
        {
            timerText.color = overrideColor; 
        }
        else
        {
            if (mode == TimerMode.CountDown)
            {
                if (useUrgentColors)
                {
                    if (remainingTime <= redThresholdSeconds) timerText.color = Color.red;
                    else if (remainingTime <= yellowThresholdSeconds) timerText.color = Color.yellow;
                    else timerText.color = Color.white;
                }
                else
                {
                    timerText.color = Color.white;
                }
            }
            else
            {
                timerText.color = Color.white;
            }
        }

       
        timerText.text = $"{m:00}:{s:00}";
    }

    public float GetTimeForStats()
    {
       
        return (mode == TimerMode.CountUp) ? elapsedTime : (countdownTime - remainingTime);
    }

    public static string FormatMMSS(float seconds)
    {
        if (seconds < 0f) seconds = 0f;
        int total = Mathf.FloorToInt(seconds); 
        int m = total / 60;
        int s = total % 60;
        return $"{m:00}:{s:00}";
    }
}
