using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class TeleportController : MonoBehaviour
{
    [Header("OVR Components")]
    public OVRCameraRig ovrCameraRig;
    public GameObject leftTeleportHotspot;
    public GameObject rightTeleportHotspot;
    [Header("Teleport Visual Feedback")]
    public GameObject[] teleportAidObjects;  
    public Material facilMaterial;        
    public Material normalMaterial;         
    private GameManager gameManager;
    private bool teleportEnabled = false;
    private DifficultyLevel currentDifficulty;
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        GameManager.OnMenuStateChanged += OnMenuStateChanged;
        GameManager.OnGameStateChanged += OnGameStateChanged;
        GameManager.OnDifficultyChanged += OnDifficultyChanged;
        SetTeleportEnabled(false);
    }

    private void OnDestroy()
    {
        GameManager.OnMenuStateChanged -= OnMenuStateChanged;
        GameManager.OnGameStateChanged -= OnGameStateChanged;
        GameManager.OnDifficultyChanged -= OnDifficultyChanged;
    }

    private void OnMenuStateChanged(bool isInMenu)
    {
        if (isInMenu)
        {
            SetTeleportEnabled(false);
            Debug.Log("Teleporte deshabilitado - En menú");
        }
    }

    private void OnGameStateChanged(bool isGameActive)
    {
        SetTeleportEnabled(isGameActive);
        Debug.Log($"Teleporte {(isGameActive ? "habilitado" : "deshabilitado")} - Juego {(isGameActive ? "activo" : "inactivo")}");
    }

    private void OnDifficultyChanged(DifficultyLevel difficulty)
    {
        currentDifficulty = difficulty;
        ConfigureTeleportVisuals();
    }

    private void SetTeleportEnabled(bool enabled)
    {
        teleportEnabled = enabled;

        if (leftTeleportHotspot != null)
            leftTeleportHotspot.SetActive(enabled);

        if (rightTeleportHotspot != null)
            rightTeleportHotspot.SetActive(enabled);

        // Si tienes componentes específicos de teleporte, deshabilitarlos aquí
        // Por ejemplo, si usas OVR Interaction SDK:
        // EnableOVRTeleportComponents(enabled);
    }

    private void ConfigureTeleportVisuals()
    {
        if (teleportAidObjects == null) return;

        foreach (GameObject aidObj in teleportAidObjects)
        {
            if (aidObj != null)
            {
                bool showAid = (currentDifficulty == DifficultyLevel.Easy);
                aidObj.SetActive(showAid);

                if (showAid)
                {
                    Renderer renderer = aidObj.GetComponent<Renderer>();
                    if (renderer != null && facilMaterial != null)
                    {
                        renderer.material = facilMaterial;
                    }
                }
            }
        }

        Debug.Log($"Ayudas de teleporte configuradas para modo: {currentDifficulty}");
    }

    public bool CanTeleport()
    {
        return teleportEnabled && gameManager != null && gameManager.IsGameActive();
    }

    public bool IsTeleportAllowed()
    {
        if (!teleportEnabled)
        {
            Debug.Log("Teleporte bloqueado - No está habilitado");
            return false;
        }

        if (gameManager != null && gameManager.IsInMenu())
        {
            Debug.Log("Teleporte bloqueado - En menú");
            return false;
        }

        return true;
    }

    private void EnableOVRTeleportComponents(bool enabled)
    {
        // Buscar y controlar componentes de teleporte de OVR
        var teleportComponents = FindObjectsOfType<MonoBehaviour>().Where(comp =>
            comp.GetType().Name.Contains("Teleport") ||
            comp.GetType().Name.Contains("Locomotion"));

        foreach (var comp in teleportComponents)
        {
            comp.enabled = enabled;
        }
    }
}