using UnityEngine;

[System.Serializable]
public class GrapplingHookSettings
{
    public float maxDistance = 15f;
    public float hookSpeed = 20f;
    public float returnSpeed = 15f;
    public float swingForce = 10f;
    public LayerMask grappleLayer;
    public GameObject hookVisualPrefab;
    public Transform handTransform;

    [HideInInspector] public bool isGrappling;
    [HideInInspector] public bool isReturning;
    [HideInInspector] public Vector3 grapplePoint;
    [HideInInspector] public GameObject hookVisualInstance; // Cambiado: Instancia del gancho
    [HideInInspector] public SpringJoint joint;
    [HideInInspector] public IGrapplePoint currentGrapplePoint;
    [HideInInspector] public Rigidbody hookRb; // Para movimiento kinemático
}