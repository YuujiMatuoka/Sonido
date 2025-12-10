using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnyPathFollower : MonoBehaviour
{
    [Header("Waypoints")]
    public List<Transform> points;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float distanceThreshold = 0.1f;

    [Header("Jump")]
    public float jumpForce = 5f;

    [Header("Ground Detection")]
    public string groundTag = "Ground";
    public float extraGroundDistance = 0.1f;

    [Header("Rotation")]
    public float rotationSpeed = 360f; // Velocidad de rotación en grados/seg

    [Header("Cooldown")]
    public float postLandingCooldown = 0.3f;

    private int currentIndex = 0;
    private bool isJumping = false;
    private bool isWaitingAfterLanding = false;
    private Rigidbody rb;
    private Collider objectCollider;
    private float groundDistance;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();
        if (objectCollider != null)
            groundDistance = objectCollider.bounds.extents.y;
        else
            groundDistance = 0.5f;

        if (points == null || points.Count == 0)
            Debug.LogWarning("No se han asignado waypoints en BunnyPathFollower.");
    }

    void Update()
    {
        if (points == null || points.Count == 0)
            return;

        // Mientras NO esté saltando y NO esté esperando cooldown, se mueve
        if (!isJumping && !isWaitingAfterLanding)
        {
            Vector3 targetPos = points[currentIndex].position;
            Vector3 targetPosXZ = new Vector3(targetPos.x, transform.position.y, targetPos.z);

            transform.position = Vector3.MoveTowards(transform.position, targetPosXZ, moveSpeed * Time.deltaTime);

            // Rotar hacia el waypoint mientras se mueve
            RotateTowards(targetPosXZ);

            if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                 new Vector3(targetPos.x, 0, targetPos.z)) < distanceThreshold)
            {
                TriggerJump();
            }
        }

        // 🔥 NUEVO: si está esperando después de aterrizar, solo rota
        if (isWaitingAfterLanding)
        {
            Vector3 nextTarget = points[currentIndex].position;
            Vector3 nextTargetXZ = new Vector3(nextTarget.x, transform.position.y, nextTarget.z);
            RotateTowards(nextTargetXZ);
        }
    }

    void RotateTowards(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        if (direction.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void TriggerJump()
    {
        if (rb != null)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumping = true;
        }
    }

    void FixedUpdate()
    {
        if (isJumping && IsGrounded())
        {
            isJumping = false;
            StartCoroutine(PostLandingCooldown());
        }
    }

    IEnumerator PostLandingCooldown()
    {
        isWaitingAfterLanding = true;

        // Avanzar al siguiente waypoint
        currentIndex++;
        if (currentIndex >= points.Count)
            currentIndex = 0;

        yield return new WaitForSeconds(postLandingCooldown);

        isWaitingAfterLanding = false;
    }

    bool IsGrounded()
    {
        RaycastHit hit;
        float checkDistance = groundDistance + extraGroundDistance;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, checkDistance))
        {
            if (hit.collider.CompareTag(groundTag))
                return true;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (points == null || points.Count == 0)
            return;

        Gizmos.color = Color.green;
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i] != null)
            {
                Gizmos.DrawSphere(points[i].position, 0.2f);
                int nextIndex = (i + 1) % points.Count;
                if (points[nextIndex] != null)
                    Gizmos.DrawLine(points[i].position, points[nextIndex].position);
            }
        }
    }
}