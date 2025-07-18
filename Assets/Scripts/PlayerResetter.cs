using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;

public class PlayerResetter : MonoBehaviour
{
    public static PlayerResetter Instance { get; private set; }

    [Tooltip("Transform jugadorcito" +
        "")]
    public Transform playerRig;

    private Vector3 initialPos;
    private Quaternion initialRot;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            initialPos = playerRig.position;
            initialRot = playerRig.rotation;
        }
        else Destroy(gameObject);
    }

    public void ResetPosition()
    {
        playerRig.position = initialPos;
        playerRig.rotation = initialRot;
    }
}
