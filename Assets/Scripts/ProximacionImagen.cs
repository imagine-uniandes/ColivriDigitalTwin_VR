using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximacionImagen : MonoBehaviour
{
    public Transform player;
    public GameObject imageObject;
    public float distanciaActivacion = 3f;

    void Update()
    {
        float distancia= Vector3.Distance(player.position, transform.position);
        bool mostrar = distancia <= distanciaActivacion;
        if(imageObject.activeSelf != mostrar)
            imageObject.SetActive(mostrar);
    }
}
