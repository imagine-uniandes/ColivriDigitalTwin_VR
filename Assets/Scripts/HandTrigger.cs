using UnityEngine;

public class HandTrigger : MonoBehaviour
{
    public enum Hand { Left, Right }
    [SerializeField] private Hand hand;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            if (TutorialController.Instance != null)
                TutorialController.Instance.NotifyFaceProximity(hand == Hand.Left);
            return;
        }

    }
}
