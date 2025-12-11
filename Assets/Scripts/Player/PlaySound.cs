using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    [SerializeField]
    private AK.Wwise.Event footstepsEvent;

    public void PlayFootstepSound()
    {
        GroundSwitch();
        footstepsEvent.Post(gameObject);
    }
    public void GroundSwitch()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, -Vector3.up);
        Material surfaceMaterial;

        if (Physics.Raycast(ray, out hit, 1.0f,Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            Renderer surfaceRenderer;
        }
    }
}
