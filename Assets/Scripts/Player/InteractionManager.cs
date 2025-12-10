using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class InteractionManager
{
    private readonly Camera playerCamera;
    private readonly Transform playerHandTransform;
    private readonly LayerMask interactableLayer;
    private readonly float interactionRange;
    private readonly LayerMask objectLayer = LayerMask.NameToLayer("Walls");
    private readonly PlayerModel playerModel;

    public IPickable currentHeldObject;
    private RaycastHit lastHit;


    public InteractionManager(Camera camera, Transform handTransform, LayerMask layer, float range, PlayerModel model)
    {
        playerCamera = camera;
        playerHandTransform = handTransform;
        interactableLayer = layer;
        interactionRange = range;
        playerModel = model;
    }

    public void TryInteract(InteractionType type, bool isButtonDown)
    {
        // Primero verificar si estamos apuntando a un punto de hook
        if (type == InteractionType.LeftClick && playerModel.hasGrappleUpgrade)
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, playerModel.grappleLayer))
            {
                // Disparar el hook y salir sin procesar otras interacciones
                playerModel.grapplingGun.StartGrapple();
                return;
            }
        }

        // Solo hacer Raycast si no tenemos un objeto agarrado o es una interacción nueva
        if (currentHeldObject == null || type != InteractionType.LeftClick)
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            if (!Physics.Raycast(ray, out lastHit, interactionRange, interactableLayer))
            {
                return;
            }
        }

        switch (type)
        {
            case InteractionType.LeftClick:
                if (playerModel.useToggleGrab)
                {
                    TryToggleGrab();
                }
                else
                {
                    HandlePickupInteraction(isButtonDown);
                }
                break;

            case InteractionType.KeyPressE:
                lastHit.collider.GetComponent<IInteractable>()?.Interact();
                break;

            case InteractionType.RightClick:
                lastHit.collider.GetComponent<IInspectable>()?.GetDescription();
                break;
        }
    }
    //Grab por toggle
    public void TryToggleGrab()
    {
        // Si NO estamos agarrando nada, intentamos agarrar (con raycast)
        if (currentHeldObject == null)
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

            if (Physics.Raycast(ray, out lastHit, interactionRange, interactableLayer))
            {
                var pickable = lastHit.collider.GetComponent<IPickable>();
                if (pickable != null)
                {
                    currentHeldObject = pickable;
                    currentHeldObject.Grab(playerHandTransform, playerModel.grabLightningEffect);
                    playerModel.isHoldingObject = true;
                    playerModel.SetHeldObject(currentHeldObject); // Asegurar sincronización
                }
            }
        }
        // Si YA estamos agarrando algo, soltamos (sin condiciones)
        else
        {
            currentHeldObject.OnDrop();
            currentHeldObject = null;
            playerModel.lastReleaseTime = Time.time;
            playerModel.isHoldingObject = false;
            playerModel.SetHeldObject(null); // Asegurar sincronización
        }
    }
    //Grab por Holding
    private void HandlePickupInteraction(bool isButtonDown)
    {
        if (isButtonDown)
        {
            if (currentHeldObject == null)
            {
                currentHeldObject = lastHit.collider.GetComponent<IPickable>();
                currentHeldObject?.Grab(playerHandTransform, playerModel.grabLightningEffect);
            }
        }
        else
        {
            if (currentHeldObject != null)
            {
                currentHeldObject?.OnDrop();
                currentHeldObject = null;
                playerModel.lastReleaseTime = Time.time;
            }
        }
    }
    public void ForceReleaseObject()
    {
        if (currentHeldObject != null)
        {
            currentHeldObject.OnDrop();
            currentHeldObject = null;
            playerModel.lastReleaseTime = Time.time;
            playerModel.isHoldingObject = false;
            playerModel.SetHeldObject(null);
        }
    }

    public IPickable CurrentHedlObject => currentHeldObject;
}