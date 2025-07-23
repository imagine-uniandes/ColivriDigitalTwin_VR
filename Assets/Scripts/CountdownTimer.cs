using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public TMP_Text timerText;
    float timeLeft;
    bool isCountingDown;

    void Start()
    {
        InitializeTimer();
    }

    void InitializeTimer()
    {
        int lvl = PlayerPrefs.GetInt("DifficultyLevel", 1);
        isCountingDown = (lvl == 3);
        timeLeft = isCountingDown
            ? LeaderboardManager.Instance.GetBestTime()
            : 0f;
    }

    void Update()
    {
        if (isCountingDown)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
                GameFlowManager.Instance.HandleTimeUp();
                enabled = false;
            }
        }
        else
        {
            timeLeft += Time.deltaTime;
        }

        timerText.text = timeLeft.ToString("F2");
    }

    public void ResetTimer()
    {
        InitializeTimer();
        enabled = true;
    }
}