using System.Collections.Generic;
using UnityEngine;

public class DoorSystemManager : MonoBehaviour
{
    public static DoorSystemManager Instance { get; private set; }

    [System.Serializable]
    public class SystemReference
    {
        public int systemID;
        public HolderShield holderShield;
        public RotateEnergy rotateEnergy;
        public List<TimeDoor> assignedDoors = new List<TimeDoor>();
        public List<CardConsole> assignedConsoles = new List<CardConsole>();
    }

    [Header("Referencias del Sistema")]
    public List<SystemReference> systemReferences = new List<SystemReference>();

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

    private void Start()
    {
    }

    public void RegisterHolderShield(int systemID, HolderShield holder)
    {
        if (systemID == 0)
        {
            return;
        }

        var existingRef = systemReferences.Find(r => r.systemID == systemID);

        if (existingRef != null)
        {
            existingRef.holderShield = holder;
            AssignDoorsToHolder(existingRef);
        }
        else
        {
            var newRef = new SystemReference
            {
                systemID = systemID,
                holderShield = holder
            };
            systemReferences.Add(newRef);
            AssignExistingDoorsToSystem(newRef);
        }
    }

    public void RegisterRotateEnergy(int systemID, RotateEnergy rotator)
    {
        if (systemID == 0)
        {
            return;
        }

        var existingRef = systemReferences.Find(r => r.systemID == systemID);

        if (existingRef != null)
        {
            existingRef.rotateEnergy = rotator;
            AssignDoorsToRotator(existingRef);
        }
        else
        {
            var newRef = new SystemReference
            {
                systemID = systemID,
                rotateEnergy = rotator
            };
            systemReferences.Add(newRef);
            AssignExistingDoorsToSystem(newRef);
        }
    }

    public void RegisterTimeDoor(TimeDoor timeDoor, int systemID, int phase)
    {
        if (systemID == 0)
        {
            return;
        }

        var existingRef = systemReferences.Find(r => r.systemID == systemID);

        if (existingRef != null)
        {
            if (!existingRef.assignedDoors.Contains(timeDoor))
            {
                existingRef.assignedDoors.Add(timeDoor);
                AssignDoorToSystem(timeDoor, existingRef, phase);
            }
        }
        else
        {
            var newRef = new SystemReference
            {
                systemID = systemID,
                assignedDoors = new List<TimeDoor> { timeDoor }
            };
            systemReferences.Add(newRef);
        }
    }

    public void RegisterCardConsole(CardConsole console, int systemID)
    {
        if (systemID == 0)
        {
            return;
        }

        var existingRef = systemReferences.Find(r => r.systemID == systemID);

        if (existingRef != null)
        {
            if (!existingRef.assignedConsoles.Contains(console))
            {
                existingRef.assignedConsoles.Add(console);
                AssignConsoleToSystem(console, existingRef);
            }
        }
        else
        {
            var newRef = new SystemReference
            {
                systemID = systemID,
                assignedConsoles = new List<CardConsole> { console }
            };
            systemReferences.Add(newRef);
        }
    }

    private void AssignExistingDoorsToSystem(SystemReference systemRef)
    {
        foreach (var otherSystemRef in systemReferences)
        {
            if (otherSystemRef != systemRef && otherSystemRef.systemID == systemRef.systemID)
            {
                if (otherSystemRef.assignedDoors.Count > 0)
                {

                    foreach (var door in otherSystemRef.assignedDoors.ToArray())
                    {
                        if (!systemRef.assignedDoors.Contains(door))
                        {
                            systemRef.assignedDoors.Add(door);
                            AssignDoorToSystem(door, systemRef, 0);
                        }
                    }
                    otherSystemRef.assignedDoors.Clear();
                }

                if (otherSystemRef.assignedConsoles.Count > 0)
                {

                    foreach (var console in otherSystemRef.assignedConsoles.ToArray())
                    {
                        if (!systemRef.assignedConsoles.Contains(console))
                        {
                            systemRef.assignedConsoles.Add(console);
                            AssignConsoleToSystem(console, systemRef);
                        }
                    }
                    otherSystemRef.assignedConsoles.Clear();
                }
            }
        }
    }

    private void AssignDoorsToHolder(SystemReference systemRef)
    {
        if (systemRef.holderShield == null) return;


        foreach (var door in systemRef.assignedDoors)
        {
            systemRef.holderShield.AddTimeDoorToDefault(door);
        }

        AssignConsolesToHolder(systemRef);
    }

    private void AssignDoorsToRotator(SystemReference systemRef)
    {
        if (systemRef.rotateEnergy == null) return;


        foreach (var door in systemRef.assignedDoors)
        {
            int doorPhase = GetDoorPhase(door);
            systemRef.rotateEnergy.AssignDoorToPhase(door, doorPhase);
        }

        AssignConsolesToRotator(systemRef);
    }

    private void AssignConsolesToHolder(SystemReference systemRef)
    {
        if (systemRef.holderShield == null) return;


        foreach (var console in systemRef.assignedConsoles)
        {
            systemRef.holderShield.AddConsoleToDefault(console);
        }
    }

    private void AssignConsolesToRotator(SystemReference systemRef)
    {
        if (systemRef.rotateEnergy == null) return;


        foreach (var console in systemRef.assignedConsoles)
        {
            int consolePhase = GetConsolePhase(console);
            systemRef.rotateEnergy.AssignConsoleToPhase(console, consolePhase);
        }
    }

    private void AssignDoorToSystem(TimeDoor door, SystemReference systemRef, int phase)
    {
        if (systemRef.holderShield != null)
        {
            systemRef.holderShield.AddTimeDoorToDefault(door);
        }

        if (systemRef.rotateEnergy != null)
        {
            systemRef.rotateEnergy.AssignDoorToPhase(door, phase);
        }
    }

    private void AssignConsoleToSystem(CardConsole console, SystemReference systemRef)
    {
        if (systemRef.holderShield != null)
        {
            systemRef.holderShield.AddConsoleToDefault(console);
        }

        if (systemRef.rotateEnergy != null)
        {
            int consolePhase = GetConsolePhase(console);
            systemRef.rotateEnergy.AssignConsoleToPhase(console, consolePhase);
        }
    }

    private int GetDoorPhase(TimeDoor door)
    {
        return door.phase;
    }

    private int GetConsolePhase(CardConsole console)
    {
        return console.phase;
    }

    public void UnregisterTimeDoor(TimeDoor timeDoor)
    {
        foreach (var systemRef in systemReferences)
        {
            if (systemRef.assignedDoors.Contains(timeDoor))
            {
                systemRef.assignedDoors.Remove(timeDoor);
            }
        }
    }

    public void UnregisterCardConsole(CardConsole console)
    {
        foreach (var systemRef in systemReferences)
        {
            if (systemRef.assignedConsoles.Contains(console))
            {
                systemRef.assignedConsoles.Remove(console);
            }
        }
    }

    public SystemReference GetSystemReference(int systemID)
    {
        return systemReferences.Find(r => r.systemID == systemID);
    }

    public bool SystemExists(int systemID)
    {
        return systemReferences.Exists(r => r.systemID == systemID);
    }

    public void CleanNullReferences()
    {
        for (int i = systemReferences.Count - 1; i >= 0; i--)
        {
            var systemRef = systemReferences[i];

            systemRef.assignedDoors.RemoveAll(door => door == null);
            systemRef.assignedConsoles.RemoveAll(console => console == null);

            if (systemRef.holderShield == null && systemRef.rotateEnergy == null && systemRef.assignedDoors.Count == 0 && systemRef.assignedConsoles.Count == 0)
            {
                systemReferences.RemoveAt(i);
                if (showDebugLogs) Debug.Log($"Sistema {systemRef.systemID} removido por no tener referencias válidas");
            }
        }
    }
}