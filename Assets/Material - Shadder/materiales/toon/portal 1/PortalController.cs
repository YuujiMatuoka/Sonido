using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalController : MonoBehaviour
{
    public Material portalMaterial;
    public float startDelay = 2f;

    void OnEnable()
    {
        // Seteamos el tiempo de activación
        portalMaterial.SetFloat("_ActivationTime", Time.time);
        portalMaterial.SetFloat("_StartDelay", startDelay);
    }
}
