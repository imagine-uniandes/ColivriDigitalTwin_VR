using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PlayerData
{
    public string playerName;
    public DifficultyLevel difficultyLevel;
    public List<float> topTimes = new List<float>();
    public float bestTime;

    public PlayerData()
    {
        playerName = "";
        difficultyLevel = DifficultyLevel.Normal;
        topTimes = new List<float>();
        bestTime = float.MaxValue;
    }
}