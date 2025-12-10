using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorID : MonoBehaviour
{
    [Header("NameOfDoor")]
    public string uniqueAnchorID;
    [Header("Configuración de Escenas")]
    [Tooltip("La escena que se carga/descarga con la interacción normal (Panel A)")]
    public string ID_A;

    [Tooltip("La escena a la que pertenece este anclaje (Panel B)")]
    public string ID_B;

    [Header("Identificación del Sistema")]
    [Tooltip("Número ID que debe coincidir con HolderShield o RotateEnergy")]
    public int systemID;

    [Header("Configuración de Fase")]
    [Tooltip("Fase a la que pertenece esta puerta (0-3)")]
    [Range(0, 3)]
    public int phase = 0;
}
