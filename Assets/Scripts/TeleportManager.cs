using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance;

    private TeleportHotspot[] hotspots;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        hotspots = FindObjectsOfType<TeleportHotspot>();
    }

    
    public void ShowHelperTeleports(bool show)
    {
        foreach (var h in hotspots)
            h.gameObject.SetActive(show);
    }
}