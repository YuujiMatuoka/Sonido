using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldZone : MonoBehaviour, ITimeTravel
{
    [Header("Settings")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private LayerMask protectionLayers;

    private Collider zoneCollider;
    private HashSet<IProtected> protectedObjects = new HashSet<IProtected>();

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();
        if (zoneCollider == null)
        {
            
            enabled = false;
            return;
        }

        zoneCollider.isTrigger = true;
        //protectionLayers = LayerMask.GetMask("Props", "breakableLayer", "interactableLayer", "HeldObject", "NoCollsions");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidLayer(other.gameObject.layer)) return;

        if (other.TryGetComponent(out IProtected obj))
        {
            protectedObjects.Add(obj);
            obj.SetProtected(true);
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsValidLayer(other.gameObject.layer)) return;

        if (other.TryGetComponent(out IProtected obj))
        {
            protectedObjects.Remove(obj);
            obj.SetProtected(false);
            
        }
    }

    public void DisableAllProtections()
    {
        

        foreach (var obj in protectedObjects)
        {
            obj?.SetProtected(false);
        }

        protectedObjects.Clear();
    }

    private bool IsValidLayer(int layer)
    {
        return (protectionLayers & (1 << layer)) != 0;
    }

    private void OnDisable()
    {
        if (protectedObjects.Count > 0)
        {
            DisableAllProtections();
        }
    }

    // ITimeTravel implementation
    public void OnTimeChanged(TimeState newTimeState) { }
    public void PreTimeChange(TimeState newTimeState) { }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (zoneCollider == null) return;

        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;

        if (zoneCollider is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (zoneCollider is SphereCollider sphere)
        {
            Gizmos.DrawSphere(sphere.center, sphere.radius);
        }
    }
#endif
}