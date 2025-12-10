using UnityEngine;
using System.Collections.Generic;

public class HolderShield : TimeTravelObject
{
    [System.Serializable]
    public class ProtectedObjects
    {
        public List<GameObject> objects;
    }

    [Header("Dependencies")]
    [SerializeField] private RotateEnergy mainSwitch;

    [Header("Protection Settings")]
    [Tooltip("Objetos que SIEMPRE se protegen cuando hay un escudo")]
    [SerializeField] private ProtectedObjects defaultProtection;


    [Header("Panel Settings")]
    [SerializeField] private Transform shieldSocket;
    [SerializeField] private AudioClip insertSound;
    [SerializeField] private AudioClip timeDisconnectSound;

    [Header("Cooldown Settings")]
    [SerializeField] private float disconnectCooldown = 1.0f;

    [Header("System Identification")]
    [Tooltip("ID del sistema que debe coincidir con TimeDoors")]
    public int systemID;

    private TimeShield currentShield;
    private bool isReady = true;
    private bool isInCooldown = false;

    public Transform GetSocket() => shieldSocket;
    public bool HasShieldAttached() => currentShield != null;
    public bool CanAcceptShield() => isReady && currentShield == null && !isInCooldown;

    protected override void Start()
    {
        base.Start();

        if (DoorSystemManager.Instance != null && systemID != 0)
        {
            DoorSystemManager.Instance.RegisterHolderShield(systemID, this);
        }

        SetPowerState(currentShield != null);
    }

    public override void OnTimeChanged(TimeState newTimeState)
    {
        base.OnTimeChanged(newTimeState);
        if (currentShield != null && newTimeState == TimeState.L1)
        {
            var savedPosition = currentShield.GetLastSavedPosition();
            float distanceThreshold = 0.2f;
            if (Vector3.Distance(savedPosition, shieldSocket.position) > distanceThreshold)
            {
                currentShield.DetachFromPanel(true);
            }
        }
    }

    private void SetPowerState(bool isPowered)
    {
        SetObjectsProtection(defaultProtection?.objects, isPowered);

        mainSwitch?.SetPowered(isPowered);
    }

    private void SetObjectsProtection(List<GameObject> objects, bool protect)
    {
        if (objects == null) return;
        foreach (var obj in objects)
        {
            var protectedObj = obj?.GetComponent<IProtected>();
            protectedObj?.SetProtected(protect);
        }
    }

    public void OnShieldAttached(TimeShield shield)
    {
        currentShield = shield;
        isReady = false;

        SetPowerState(true); 

        if (insertSound != null)
        {
            AudioSource.PlayClipAtPoint(insertSound, transform.position);
        }
        Invoke(nameof(ResetReadyState), 0.5f);
    }

    public void OnShieldDetached(bool timeTravelDisconnect = false)
    {
        SetPowerState(false); 

        if (timeTravelDisconnect && timeDisconnectSound != null)
        {
            AudioSource.PlayClipAtPoint(timeDisconnectSound, transform.position);
        }
        currentShield = null;
        if (timeTravelDisconnect)
        {
            StartCooldown();
        }
    }

    public void AddTimeDoorToDefault(TimeDoor door)
    {
        if (door == null) return;

        // Inicializar la lista si es null
        if (defaultProtection.objects == null)
            defaultProtection.objects = new List<GameObject>();

        // Agregar la puerta si no existe
        if (!defaultProtection.objects.Contains(door.gameObject))
        {
            defaultProtection.objects.Add(door.gameObject);

            // Si el escudo está activo, aplicar protección inmediatamente
            if (HasShieldAttached())
            {
                var protectedObj = door.GetComponent<IProtected>();
                protectedObj?.SetProtected(true);
            }
        }
    }

    public void AddConsoleToDefault(CardConsole console)
    {
        if (console == null) return;

        // Inicializar la lista si es null
        if (defaultProtection.objects == null)
            defaultProtection.objects = new List<GameObject>();

        // Agregar la consola si no existe
        if (!defaultProtection.objects.Contains(console.gameObject))
        {
            defaultProtection.objects.Add(console.gameObject);

            // Si el escudo está activo, aplicar protección inmediatamente
            if (HasShieldAttached())
            {
                var protectedObj = console.GetComponent<IProtected>();
                protectedObj?.SetProtected(true);
            }
        }
    }


    private void StartCooldown()
    {
        isInCooldown = true;
        Invoke(nameof(EndCooldown), disconnectCooldown);
    }

    private void EndCooldown()
    {
        isInCooldown = false;
    }

    private void ResetReadyState()
    {
        isReady = true;
    }
}