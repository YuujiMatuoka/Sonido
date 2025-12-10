using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Este manager se encarga de buscar anclajes de DoorID en las escenas
/// recién cargadas y reemplazarlos por el Prefab de TimeDoor.
/// </summary>
public class DoorSpawnerManager : MonoBehaviour
{
    public static DoorSpawnerManager Instance { get; private set; }

    [SerializeField]
    private GameObject timeDoorPrefab; // Arrastra aquí tu prefab de la puerta (el que tiene TimeDoor.cs)

    private Dictionary<string, TimeDoor> activeDoors = new Dictionary<string, TimeDoor>();

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

    /// <summary>
    /// Busca todos los DoorID en una escena específica y los reemplaza
    /// por el prefab de TimeDoor, transfiriendo el ID.
    /// </summary>
    /// <param name="scene">La escena que acaba de ser cargada.</param>
    public void ProcessSceneForDoorAnchors(Scene scene)
    {
        if (timeDoorPrefab == null)
        {
            return;
        }

        List<DoorID> doorAnchors = new List<DoorID>();
        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject root in rootObjects)
        {
            doorAnchors.AddRange(root.GetComponentsInChildren<DoorID>(true));
        }

        

        foreach (DoorID anchor in doorAnchors)
        {
            if (string.IsNullOrEmpty(anchor.uniqueAnchorID))
            {
                continue;
            }

            if (activeDoors.ContainsKey(anchor.uniqueAnchorID))
            {
                Destroy(anchor.gameObject);
            }
            else
            {
                GameObject doorInstance = Instantiate(
                    timeDoorPrefab,
                    anchor.transform.position,
                    anchor.transform.rotation
                );

                TimeDoor timeDoor = doorInstance.GetComponentInChildren<TimeDoor>(true);
                CardConsole cardConsole = doorInstance.GetComponentInChildren<CardConsole>(true);

                if (timeDoor != null)
                {
                    timeDoor.Scene_A = anchor.ID_A;
                    timeDoor.Scene_B = anchor.ID_B;
                    timeDoor.uniqueAnchorID = anchor.uniqueAnchorID;

                    // ASIGNAR systemID y phase A LA TIMEDOOR
                    timeDoor.systemID = anchor.systemID;
                    timeDoor.phase = anchor.phase;

                    if (DoorSystemManager.Instance != null && anchor.systemID != 0)
                    {
                        DoorSystemManager.Instance.RegisterTimeDoor(timeDoor, anchor.systemID, anchor.phase);
                    }

                    activeDoors.Add(anchor.uniqueAnchorID, timeDoor);
                }

                if (cardConsole != null)
                {
                    // ASIGNAR systemID y phase A LA CARDCONSOLE
                    cardConsole.systemID = anchor.systemID;
                    cardConsole.phase = anchor.phase;

                    if (DoorSystemManager.Instance != null && anchor.systemID != 0)
                    {
                        DoorSystemManager.Instance.RegisterCardConsole(cardConsole, anchor.systemID);
                    }
                }

                Destroy(anchor.gameObject);
            }
        }
    }


// --- CAMBIO CLAVE (3/3): Método para de-registrar ---
/// <summary>
/// Permite que una puerta se elimine del registro si es destruida.
/// </summary>
public void UnregisterDoor(string uniqueID)
    {
        if (!string.IsNullOrEmpty(uniqueID) && activeDoors.ContainsKey(uniqueID))
        {
            activeDoors.Remove(uniqueID);
        }
    }
}