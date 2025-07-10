using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintActivation : MonoBehaviour
{
    public GameObject hint;
    // Start is called before the first frame update
    void Start()
    {
        hint.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        print(other.gameObject.name);

        if (other.name.Equals("CenterEyeAnchor"))

        {
            hint.SetActive(true);
        }
    }

    
}
