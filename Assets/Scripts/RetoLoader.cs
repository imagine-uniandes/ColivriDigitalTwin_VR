using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetoLoader : MonoBehaviour
{
    public Reto reto;

    void Awake()
    {
        TextAsset ta = Resources.Load<TextAsset>("Reto1");
        reto = JsonUtility.FromJson<Reto>(ta.text);
    }
}
