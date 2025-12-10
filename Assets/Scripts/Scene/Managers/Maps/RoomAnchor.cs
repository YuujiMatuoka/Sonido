using UnityEngine;

public class RoomAnchor : MonoBehaviour
{
    [Header("Identificador único del ancla (por ejemplo: 'Door_A_B')")]
    public string anchorID;

    [Header("Escena destino al cruzar esta puerta")]
    public string targetScene;

    [Header("Ancla destino en la otra habitación")]
    public string targetAnchorID;
}
