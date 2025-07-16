
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportHotspot : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform teleportDestination;
    public float teleportRange = 2f;
    public string targetTag = "Player";
    [Header("Visual Effects")]
    public GameObject effectPrefab;
    public Color hotspotColor = Color.cyan;
    public float pulseSpeed = 2f;
    [Header("Audio")]
    public AudioClip teleportSound;
    private AudioSource audioSource;
    private Renderer hotspotRenderer;
    private Color originalColor;
    private bool playerInRange = false;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        hotspotRenderer = GetComponent<Renderer>();
        if (hotspotRenderer != null)
            originalColor = hotspotRenderer.material.color;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    void Update()
    {
        if (hotspotRenderer != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
            hotspotRenderer.material.color = Color.Lerp(originalColor, hotspotColor, pulse);
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TeleportPlayer();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            playerInRange = true;
            ShowTeleportUI(true);
            Debug.Log("Jugador en rango del teleport. Presiona E para teletransportarte.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            playerInRange = false;
            ShowTeleportUI(false);
        }
    }

    void TeleportPlayer()
    {
        if (teleportDestination == null)
        {
            Debug.LogWarning("No se ha asignado un destino de teletransporte!");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(targetTag);
        if (player != null)
        {
            if (teleportSound != null && audioSource != null)
                audioSource.PlayOneShot(teleportSound);
            if (effectPrefab != null)
            {
                Instantiate(effectPrefab, player.transform.position, Quaternion.identity);
                Instantiate(effectPrefab, teleportDestination.position, Quaternion.identity);
            }
            player.transform.position = teleportDestination.position;
            player.transform.rotation = teleportDestination.rotation;

            Debug.Log($"Jugador teletransportado a {teleportDestination.name}");
        }
    }

    void ShowTeleportUI(bool show)
    {
        
    }

    public void SetTeleportDestination(Transform destination)
    {
        teleportDestination = destination;
    }

    public void SetHotspotActive(bool active)
    {
        gameObject.SetActive(active);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = hotspotColor;
        Gizmos.DrawWireSphere(transform.position, teleportRange);

        if (teleportDestination != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, teleportDestination.position);
        }
    }
}