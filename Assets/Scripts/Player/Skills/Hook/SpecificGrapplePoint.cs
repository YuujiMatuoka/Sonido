using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecificGrapplePoint : MonoBehaviour, IGrapplePoint
{
    public bool isActive = true;
    public Material activeMaterial;
    public Material inactiveMaterial;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        UpdateVisual();
    }

    public bool IsValid()
    {
        return isActive;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void OnGrappleStart()
    {
        // Puedes desactivar el punto después de usarlo
        // isActive = false;
        // UpdateVisual();
    }

    public void OnGrappleEnd()
    {
        // Lógica cuando se suelta el gancho
    }

    private void UpdateVisual()
    {
        if (rend)
        {
            rend.material = isActive ? activeMaterial : inactiveMaterial;
        }
    }

    // Para debug visual
    void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}