using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerModel 
{
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 7f;
    public float mouseSensitivity = 1f;
    public GameManager gameManager;
    public Transform characterHand;

    [Header("Canvas")]
    public Animator Canvas_Animator;

    [Header("Grappling Hook")]
    public Transform gunTip;          // donde se engancha la cuerda
    public LayerMask grappleLayer;    // qué es grapleable
    public float maxGrappleDistance = 100f;
    public GrapplingGun grapplingGun; // referencia al script nuevo
    public GrapplingRope grapplingRope; // rope visual

    [Header("Grab FX")] // <-- AÑADE ESTA SECCIÓN
    public Lightning grabLightningEffect;

    [Header("Grappling Physics")]
    public float pullSpeed = 10f;     // Velocidad con la que atrae
    public float stopDistance = 2f;   // Distancia mínima al punto

    [Header("Head Bob Settings")]
    public bool enableHeadBob = true;
    public float walkBobSpeed = 14f;
    public float walkBobAmount = 0.05f;
    public float runBobSpeed = 18f;
    public float runBobAmount = 0.08f;
    [HideInInspector] public float defaultYPos = 0;
    [HideInInspector] public float timer = 0;

    [Header("Hand Zoom Settings")]
    public float handZoomSpeed = 2f;
    public float minHandDistance = 0.5f; // Distancia mínima (más cerca)
    public float maxHandDistance = 3f;   // Distancia máxima (más lejos)
    [HideInInspector] public float currentHandDistance; // Distancia actual

    [Header("Interaction Settings")]
    public bool useToggleGrab = true;

    [Header("Audio")]
    public AudioSource footstepSource;
    public AudioSource runLoopSource;
    public List<AudioClip> footstepClips; // Puedes tener múltiples sonidos para variedad
    public List<AudioClip> runFootstepClips; // correr 
    public float stepInterval = 0.5f; // Tiempo entre pasos al caminar
    public float runStepInterval = 0.35f;

    public Transform playerCamera;
    public LayerMask interactableLayer;
    public float interactRange = 2;

    [Header("Jump Settings")]
    public float jumpCooldown = 0.5f; // Tiempo entre saltos
    public float groundRadius = 0.75f; // Distancia del raycast
    public float maxSlopeAngle = 45f;
    public float groundCheckCooldown = 0f;

    [Header("Ground Check (BoxCast)")]
    public float groundCheckWidth = 0.5f;   // Ancho del box (ajustá según el tamaño del jugador)
    public float groundCheckDistance = 0.3f; // Distancia hasta el suelo
    public LayerMask groundLayers; // Capas que cuentan como suelo
    public float jumpCooldownAfterRelease = 0.3f; // Tiempo que no se puede saltar después de soltar
    public float lastReleaseTime; // Tiempo en que se soltó el último objeto

    [HideInInspector] public float lastJumpTime = -1f; // Tiempo del último salto
    [HideInInspector] public bool isGrounded;

    [HideInInspector] public Vector3 currentVelocity;
    [HideInInspector] public bool isRunning;
    [HideInInspector] public bool isHoldingObject = false;
    [HideInInspector] public float xRotation = 0f;

    private IPickable heldObject;

    // Conocimiento de tarjetas
    private HashSet<string> knownCards = new HashSet<string>();

    public bool IsHoldingObject()
    {
        return heldObject != null;
    }

    public void SetHeldObject(IPickable obj)
    {
        heldObject = obj;
    }
    public void LearnCard(string cardID)
    {
        if (!knownCards.Contains(cardID))
        {
            knownCards.Add(cardID);
            Debug.Log($"Card {cardID} has been added to known cards.");
        }
    }


    public bool KnowsCard(string cardID) => knownCards.Contains(cardID);

    public IPickable HeldObject => heldObject;

    public bool hasGrappleUpgrade = false; // Indica si el jugador tiene la mejora
}
