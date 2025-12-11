using UnityEngine;

public class ReverbZone : MonoBehaviour
{
    [Header("Wwise Room State Configuration")]
    [Tooltip("Nombre exacto del State Group en Wwise")]
    [SerializeField] private string stateGroup = "Room";

    [Tooltip("Estado cuando se ENTRA en la zona (Corridor)")]
    [SerializeField] private string onState = "Corridor";

    [Tooltip("Estado cuando se SALE de la zona (WaitRoom)")]
    [SerializeField] private string offState = "WaitRoom";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AkSoundEngine.SetState(stateGroup, onState);
            Debug.Log($"Wwise State: {stateGroup} -> {onState}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AkSoundEngine.SetState(stateGroup, offState);
            Debug.Log($"Wwise State: {stateGroup} -> {offState}");
        }
    }
}