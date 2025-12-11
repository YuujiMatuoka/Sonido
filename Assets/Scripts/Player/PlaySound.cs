using UnityEngine;

public class PlaySound : MonoBehaviour
{
    [Header("Footstep Event")]
    [SerializeField]
    private AK.Wwise.Event footstepsEvent;

    private bool isInCorridor = false;

    private void Start()
    {
        // Estado inicial por defecto
        AkSoundEngine.SetState("Room", "WaitRoom");
    }

    // Detecta entrada a Corridor
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Corridor"))
        {
            isInCorridor = true;
            AkSoundEngine.SetState("Room", "Corridor");
            Debug.Log("Room → Corridor");
        }
    }

    // Detecta salida de Corridor
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Corridor"))
        {
            isInCorridor = false;
            AkSoundEngine.SetState("Room", "WaitRoom");
            Debug.Log("Room → WaitRoom");
        }
    }

    // Footsteps
    public void PlayFootstepSound()
    {
        Footstep();
    }

    public void Footstep()
    {
        GroundSwitch();
        footstepsEvent.Post(gameObject);
    }

    public void GroundSwitch()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, -Vector3.up);

        if (Physics.Raycast(ray, out hit, 1.0f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            GameObject hitObject = hit.collider.gameObject;
            int hitLayer = hitObject.layer;
            string layerName = LayerMask.LayerToName(hitLayer);

            Debug.Log($"Hit: {hitObject.name}, Layer: {layerName}");

            if (layerName == "Default")
            {
                AkUnitySoundEngine.SetSwitch("Footstep", "Metal", gameObject);
            }
            else if (layerName == "Water")
            {
                AkUnitySoundEngine.SetSwitch("Footstep", "Stairs", gameObject);
            }
            else
            {
                AkUnitySoundEngine.SetSwitch("Footstep", "NoneMaterial", gameObject);
            }
        }
    }
}