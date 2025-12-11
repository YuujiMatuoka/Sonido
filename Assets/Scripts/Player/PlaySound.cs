using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    [SerializeField]
    private AK.Wwise.Event footstepsEvent;

    public void Footstep()  
    {
        GroundSwitch();
        footstepsEvent.Post(gameObject);
    }

    public void PlayFootstepSound()
    {
        Footstep();
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

            Debug.Log($"Hit object: {hitObject.name}, Layer: {layerName} ({hitLayer})");

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
