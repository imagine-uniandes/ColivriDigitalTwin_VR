using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";

    private void OnTriggerEnter(Collider other)
    {
 
        if (!other.CompareTag(PLAYER_TAG) && other.GetComponent<Rigidbody>() == null)
        {
            return;
        }

        if (TutorialController.Instance != null)
        {
            
            TutorialController.Instance.NotifyTeleportTargetReached();
        }

        gameObject.SetActive(false);
    }
}
