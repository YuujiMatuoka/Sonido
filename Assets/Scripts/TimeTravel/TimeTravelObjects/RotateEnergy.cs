using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateEnergy : TimeTravelObject, IInteractable
{
    [System.Serializable]
    public class PhaseOutput
    {
        [Tooltip("Objetos que se energizarán en esta fase")]
        public List<GameObject> protectedObjects;

        [Tooltip("El 'siguiente' switch en la cadena que recibirá energía (opcional)")]
        public RotateEnergy nextSwitch;

        [Tooltip("Cables que se energizarán en esta fase")]
        public List<WireTime> energizedWires;
    }

    [Header("Switch Settings")]
    [Tooltip("Las 4 salidas de energía, una para cada fase")]
    [SerializeField] private PhaseOutput[] phaseOutputs = new PhaseOutput[4];

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 5f;

    [Header("System Identification")]
    [Tooltip("ID del sistema que debe coincidir con TimeDoors")]
    public int systemID;

    private int currentPhase = 0;
    private bool isPowered = false;
    private Quaternion targetRotation;
    private Quaternion initialRotation;

    private bool isProcessingEnergy = false;
    private RotateEnergy energySource = null;

    protected override void Start()
    {
        base.Start();

        // Registrar este RotateEnergy en el sistema
        if (DoorSystemManager.Instance != null && systemID != 0)
        {
            DoorSystemManager.Instance.RegisterRotateEnergy(systemID, this);
        }

        initialRotation = transform.rotation;
        UpdateRotation();
        UpdateEnergyFlow();
    }

    // NUEVO MÉTODO: Agregar puerta a una fase específica
    public void AssignDoorToPhase(TimeDoor door, int phase)
    {
        if (door == null) return;

        // Validar que la fase esté en rango
        if (phase < 0 || phase >= phaseOutputs.Length)
        {
            return;
        }

        // Inicializar la fase si es null
        if (phaseOutputs[phase] == null)
        {
            phaseOutputs[phase] = new PhaseOutput();
        }

        // Inicializar la lista de objetos si es null
        if (phaseOutputs[phase].protectedObjects == null)
        {
            phaseOutputs[phase].protectedObjects = new List<GameObject>();
        }

        // Agregar la puerta si no existe
        if (!phaseOutputs[phase].protectedObjects.Contains(door.gameObject))
        {
            phaseOutputs[phase].protectedObjects.Add(door.gameObject);

            // Si estamos energizados en esta fase, aplicar protección inmediatamente
            if (isPowered && currentPhase == phase)
            {
                var protectedObj = door.GetComponent<IProtected>();
                protectedObj?.SetProtected(true);
            }
        }
    }

    public void AssignConsoleToPhase(CardConsole console, int phase)
    {
        if (console == null) return;

        // Validar que la fase esté en rango
        if (phase < 0 || phase >= phaseOutputs.Length)
        {
            return;
        }

        // Inicializar la fase si es null
        if (phaseOutputs[phase] == null)
        {
            phaseOutputs[phase] = new PhaseOutput();
        }

        // Inicializar la lista de objetos si es null
        if (phaseOutputs[phase].protectedObjects == null)
        {
            phaseOutputs[phase].protectedObjects = new List<GameObject>();
        }

        // Agregar la consola si no existe
        if (!phaseOutputs[phase].protectedObjects.Contains(console.gameObject))
        {
            phaseOutputs[phase].protectedObjects.Add(console.gameObject);

            // Si estamos energizados en esta fase, aplicar protección inmediatamente
            if (isPowered && currentPhase == phase)
            {
                var protectedObj = console.GetComponent<IProtected>();
                protectedObj?.SetProtected(true);
            }
        }
    }

    public void Interact()
    {
        // No se puede interactuar si no tiene energía
        if (!isPowered) return;

        currentPhase = (currentPhase + 1) % 4;
        UpdateRotation();

        // Al cambiar de fase, redirige la energía
        UpdateEnergyFlow();
    }

    public void SetPowered(bool powerState, RotateEnergy source = null)
    {
        if (isPowered == powerState) return;

        // Evitar bucles detectando si somos la fuente de energía
        if (source == this) return;

        // Si ya estamos procesando energía, evitar recursión
        if (isProcessingEnergy) return;

        isProcessingEnergy = true;

        isPowered = powerState;
        energySource = source; // Guardar quién nos dio energía

        UpdateEnergyFlow();

        isProcessingEnergy = false;
    }


    private void UpdateEnergyFlow()
    {
        // Apagar todos los objetos y cables en todas las fases
        for (int i = 0; i < phaseOutputs.Length; i++)
        {
            if (phaseOutputs[i] == null) continue;

            // Apaga los objetos protegidos
            SetObjectsProtection(phaseOutputs[i].protectedObjects, false);

            // Apaga los cables (pasa this como fuente)
            SetWiresEnergy(phaseOutputs[i].energizedWires, false);

            // Apaga el siguiente switch en la cadena (evitando bucles)
            if (phaseOutputs[i].nextSwitch != null && phaseOutputs[i].nextSwitch != energySource)
            {
                phaseOutputs[i].nextSwitch?.SetPowered(false, this);
            }
        }

        if (!isPowered) return;

        // Encender solo los objetos y cables de la fase activa
        if (currentPhase >= 0 && currentPhase < phaseOutputs.Length)
        {
            PhaseOutput activeOutput = phaseOutputs[currentPhase];
            if (activeOutput == null) return;

            // Energiza los objetos protegidos
            SetObjectsProtection(activeOutput.protectedObjects, true);

            // Energiza los cables (pasa this como fuente)
            SetWiresEnergy(activeOutput.energizedWires, true);

            // Energiza el siguiente switch en la cadena (evitando bucles)
            if (activeOutput.nextSwitch != null && activeOutput.nextSwitch != energySource)
            {
                activeOutput.nextSwitch?.SetPowered(true, this);
            }
        }
    }


    private void SetWiresEnergy(List<WireTime> wires, bool energize)
    {
        if (wires == null) return;
        foreach (var wire in wires)
        {
            wire?.AddWireEnergy(this, energize);
        }
    }

    // Método para asignar cables a una fase específica
    public void AssignWireToPhase(WireTime wire, int phase)
    {
        if (wire == null) return;

        // Validar que la fase esté en rango
        if (phase < 0 || phase >= phaseOutputs.Length)
        {
            return;
        }

        // Inicializar la fase si es null
        if (phaseOutputs[phase] == null)
        {
            phaseOutputs[phase] = new PhaseOutput();
        }

        // Inicializar la lista de cables si es null
        if (phaseOutputs[phase].energizedWires == null)
        {
            phaseOutputs[phase].energizedWires = new List<WireTime>();
        }

        // Agregar el cable si no existe
        if (!phaseOutputs[phase].energizedWires.Contains(wire))
        {
            phaseOutputs[phase].energizedWires.Add(wire);

            // Si estamos energizados en esta fase, aplicar energía inmediatamente
            if (isPowered && currentPhase == phase)
            {
                wire.AddWireEnergy(this, true); // CAMBIO AQUÍ
            }
        }
    }


    private void UpdateRotation()
    {
        float targetZRotation = 0f;
        switch (currentPhase)
        {
            case 0: targetZRotation = 0f; break;
            case 1: targetZRotation = 90f; break;
            case 2: targetZRotation = 180f; break;
            case 3: targetZRotation = 270f; break;
        }
        targetRotation = Quaternion.Euler(
            initialRotation.eulerAngles.x,
            initialRotation.eulerAngles.y,
            targetZRotation
        );
    }

    protected override void Update()
    {
        base.Update();
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
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

    protected override void OnDestroy()
    {
        // Limpiar nuestra energía de todos los cables
        for (int i = 0; i < phaseOutputs.Length; i++)
        {
            if (phaseOutputs[i]?.energizedWires != null)
            {
                foreach (var wire in phaseOutputs[i].energizedWires)
                {
                    wire?.AddWireEnergy(this, false);
                }
            }
        }

        base.OnDestroy();
    }

}