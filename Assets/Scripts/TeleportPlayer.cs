using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
    public Transform playerOrigin;
    

    public void Teleport(Transform destination)
    {
        if (playerOrigin != null && destination != null)
        {
            playerOrigin.position=destination.position;
            Debug.Log("Teletransportando a:" + destination.name);
        }
    }
}