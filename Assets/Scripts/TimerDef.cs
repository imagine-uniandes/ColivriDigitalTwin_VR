using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerDef : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float elapsedTime = 0;



    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        int miliseconds = Mathf.FloorToInt((elapsedTime *100) % 100);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds, miliseconds);
    }
}
