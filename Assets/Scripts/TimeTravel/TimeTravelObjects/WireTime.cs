using System.Collections.Generic;
using UnityEngine;

public class WireTime : TimeTravelObject, IProtected
{
    [Header("Cable Visual Settings")]
    [SerializeField] private GameObject visualL1Active;
    [SerializeField] private GameObject visualOriginActive;

    [Header("Wire Energy Settings")]
    [Tooltip("Si es true, el cable se activa con energía de RotateEnergy")]
    public bool receivesEnergyFromSwitches = true;

    private HashSet<RotateEnergy> energySources = new HashSet<RotateEnergy>();
    private bool hasEnergy = false;

    // Método para recibir energía de RotateEnergy
    public void AddWireEnergy(RotateEnergy source, bool energyState)
    {
        if (!receivesEnergyFromSwitches) return;

        if (energyState)
        {
            energySources.Add(source);
        }
        else
        {
            energySources.Remove(source);
        }

        // El cable tiene energía si AL MENOS UNA fuente lo energiza
        bool newEnergyState = energySources.Count > 0;

        if (hasEnergy != newEnergyState)
        {
            hasEnergy = newEnergyState;
            UpdateVisuals(TimeTravelManager.Instance.CurrentTimeState);
        }
    }

    public override void SetProtected(bool value)
    {
        if (isProtected == value) return;

        isProtected = value;
        UpdateVisuals(TimeTravelManager.Instance.CurrentTimeState);
    }

    protected override void UpdateVisuals(TimeState currentState)
    {
        // Desactivar todos los visuals primero
        if (visualL1Origen != null) visualL1Origen.SetActive(false);
        if (visualOriginBroken != null) visualOriginBroken.SetActive(false);
        if (visualL1 != null) visualL1.SetActive(false);
        if (visualL1Active != null) visualL1Active.SetActive(false);
        if (visualOriginActive != null) visualOriginActive.SetActive(false);

        // PRIORIDAD: Si tiene energía de RotateEnergy, mostrar estado activo
        if (hasEnergy)
        {
            if (currentState == TimeState.L1 && visualL1Active != null)
            {
                visualL1Active.SetActive(true);
            }
            else if (currentState == TimeState.Origin && visualOriginActive != null)
            {
                visualOriginActive.SetActive(true);
            }
        }
        // Si no tiene energía pero está protegido
        else if (isProtected)
        {
            if (currentState == TimeState.L1 && visualL1Active != null)
            {
                visualL1Active.SetActive(true);
            }
            else if (currentState == TimeState.Origin && visualOriginActive != null)
            {
                visualOriginActive.SetActive(true);
            }
        }
        else
        {
            // Estado normal sin energía ni protección
            if (currentState == TimeState.L1)
            {
                if (visualL1 != null) visualL1.SetActive(true);
            }
            else if (currentState == TimeState.Origin)
            {
                if (isBroken)
                {
                    if (visualOriginBroken != null) visualOriginBroken.SetActive(true);
                }
                else
                {
                    if (visualL1Origen != null) visualL1Origen.SetActive(true);
                }
            }
        }
    }

    // Limpiar fuentes cuando se destruye o resetea
    public void ClearEnergySources()
    {
        energySources.Clear();
        hasEnergy = false;
    }
}