using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimePortalConsole : TimeTravelObject, IInteractable
{
    [SerializeField] private TimeTravelPortal portal;
    [SerializeField] private AudioSource myAudioSource;
    private bool wasPlayingOnPause = false;

    [Header("Proximity Activation Settings")]
    [SerializeField] private float activationRadius = 2.5f;
    [SerializeField] private float deactivationRadius = 2.5f; 
    [SerializeField] private LayerMask playerLayerMask = 1;
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool allowReactivate = true; 

    private Collider[] hitColliders = new Collider[1];
    private bool isPortalActive = false;
    private bool hasBeenUsed = false; 
    protected override void Start()
    {
        base.Start();

        if (myAudioSource != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.RegisterSource(myAudioSource);
        }

        GameManager.OnGameStateChanged += OnGameStateChanged;
    }
    public void Interact()
    {
        ActivatePortalManually();
    }
    private void OnGameStateChanged(GameState newState)
    {
        // Manejar pausa/reanudación del sonido
        if (myAudioSource == null) return;

        if (newState == GameState.Paused)
        {
            if (myAudioSource.isPlaying)
            {
                myAudioSource.Pause();
                wasPlayingOnPause = true;
            }
        }
        else if (newState == GameState.Playing && wasPlayingOnPause)
        {
            myAudioSource.UnPause();
            wasPlayingOnPause = false;
        }
    }
    protected override void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
            return;

        if (!hasBeenUsed || allowReactivate)
        {
            CheckProximityAndTogglePortal();
        }
    }

    private void CheckProximityAndTogglePortal()
    {
        int numColliders = Physics.OverlapSphereNonAlloc(
            transform.position,
            deactivationRadius, 
            hitColliders,
            playerLayerMask
        );

        bool playerInRange = numColliders > 0;

        if (playerInRange)
        {
            float distance = Vector3.Distance(transform.position, hitColliders[0].transform.position);

            if (distance <= activationRadius && !isPortalActive)
            {
                ActivatePortalByProximity();
            }
            else if (distance > activationRadius && isPortalActive)
            {
                DeactivatePortalByProximity();
            }
        }
        else if (isPortalActive)
        {
            DeactivatePortalByProximity();
        }
    }

    private void ActivatePortalByProximity()
    {
        isPortalActive = true;
        portal.ActivatePortal();

        if (myAudioSource != null)
            myAudioSource.Play();

    }

    private void DeactivatePortalByProximity()
    {
        portal.Desactivate();
        isPortalActive = false;

        portal.gameObject.SetActive(false);

    }

    public void ActivatePortalManually()
    {
        if (!isPortalActive)
        {
            isPortalActive = true;
            hasBeenUsed = true; 
            portal.ActivatePortal();
            myAudioSource.Play();
        }
    }

    public void ResetConsole()
    {
        isPortalActive = false;
        hasBeenUsed = false;
        portal.gameObject.SetActive(false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // Desuscribirse del evento
        GameManager.OnGameStateChanged -= OnGameStateChanged;

        // Desregistrar el AudioSource
        if (myAudioSource != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.UnregisterSource(myAudioSource);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (debugMode)
        {
            Gizmos.color = isPortalActive ? Color.green : new Color(0, 0.5f, 0, 0.5f);
            Gizmos.DrawWireSphere(transform.position, activationRadius);

            Gizmos.color = isPortalActive ? new Color(1, 0.5f, 0, 0.5f) : new Color(0.5f, 0, 0, 0.5f);
            Gizmos.DrawWireSphere(transform.position, deactivationRadius);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * activationRadius, "Activation Radius");
            UnityEditor.Handles.Label(transform.position + Vector3.up * deactivationRadius, "Deactivation Radius");
#endif
        }
    }
}
