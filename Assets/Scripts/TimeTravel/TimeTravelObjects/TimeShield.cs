using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeShield : PickableTimeTravelObject
{
    [Header("Shield Settings")]
    [SerializeField] private ShieldZone shieldEffect;
    [SerializeField] private string panelTag = "Panel";
    [SerializeField] private float reattachCooldown = 1.5f;
    [SerializeField] private ParticleSystem disconnectParticles;
    [SerializeField] private bool maintainEffect = false;

    private HolderShield currentPanel;
    private Transform currentSocket;
    private bool isReadyToAttach = true;
    private SphereCollider shieldCollider;

    protected override void Start()
    {
        base.Start();
        shieldCollider = GetComponent<SphereCollider>();
        // El escudo nunca está roto
        isBroken = false;
        isShield = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isReadyToAttach || currentSocket != null || !other.CompareTag(panelTag)) return;

        var panel = other.GetComponent<HolderShield>();
        if (panel != null && panel.CanAcceptShield())
        {
            AttachToPanel(panel.GetSocket(), panel);
        }
    }

    public Vector3 GetLastSavedPosition()
    {
        return lastL1Position;
    }

    public void AttachToPanel(Transform socket, HolderShield panel)
    {
        if (!isReadyToAttach) return;
        if (isHeld)
        {
            ForceDropFromHand();
        }
        currentSocket = socket;
        currentPanel = panel;

        transform.SetParent(socket);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Desactivar efecto solo cuando está insertado
        if(!maintainEffect)
            SetShieldActive(false);
        panel.OnShieldAttached(this);

        SavePosition();
        StartCoroutine(ForcePhysicsUpdate());
    }

    private void SavePosition()
    {
        lastL1Position = transform.position;
        lastL1Rotation = transform.rotation;
        hasBeenToL1 = true;
    }

    private IEnumerator ForcePhysicsUpdate()
    {
        yield return new WaitForFixedUpdate();
        if (shieldCollider != null)
        {
            shieldCollider.enabled = false;
            shieldCollider.enabled = true;
        }
    }

    public void DetachFromPanel(bool timeTravelDisconnect = false)
    {
        if (currentSocket == null) return;

        transform.SetParent(null);
        currentSocket = null;

        if (timeTravelDisconnect && disconnectParticles != null)
        {
            disconnectParticles.Play();
        }

        if (currentPanel != null)
        {
            currentPanel.OnShieldDetached(timeTravelDisconnect);
            currentPanel = null;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        SetShieldActive(true);

        // Solo aplicar cooldown de reconexión si no fue por viaje temporal
        if (!timeTravelDisconnect)
        {
            StartCoroutine(ReattachCooldown());
        }
    }

    private IEnumerator ReattachCooldown()
    {
        isReadyToAttach = false;
        yield return new WaitForSeconds(reattachCooldown);

        if (shieldCollider != null)
        {
            shieldCollider.enabled = false;
            shieldCollider.enabled = true;
        }

        isReadyToAttach = true;
    }

    private void SetShieldActive(bool active)
    {
        if (shieldEffect != null)
        {
            shieldEffect.gameObject.SetActive(active);

            // Si se está desactivando, forzar cancelación de protecciones
            if (!active)
            {
                shieldEffect.DisableAllProtections();
            }
        }
    }

    public override void Grab(Transform handTransform, Lightning lightningEffect)
    {
        if (!isPickable) return;
        if (currentSocket != null)
        {
            DetachFromPanel();
        }
        base.Grab(handTransform, lightningEffect);
    }

    public override void OnDrop()
    {
        base.OnDrop();
    }

    public override void OnTimeChanged(TimeState newTimeState)
    {
        base.OnTimeChanged(newTimeState);

        if (newTimeState == TimeState.L1 && hasBeenToL1)
        {
            // La lógica de posición ahora la maneja el BateryPanel
            // Solo nos aseguramos de que la posición/rotación sea correcta
            if (!IsInPanel())
            {
                transform.position = lastL1Position;
                transform.rotation = lastL1Rotation;
            }
        }
        else if (newTimeState == TimeState.Origin)
        {
            // Guardar posición actual como última en L1
            SavePosition();
        }
    }

    // Eliminamos el método Break() ya que no lo necesitamos

    public bool IsInPanel() => currentSocket != null;
}