using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnyModel: MonoBehaviour, IRobotModel
{
    [SerializeField] float speed = 3f;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] private Transform union;
    [SerializeField] private string partTag = "RobotPart";
    [SerializeField] private LayerMask groundMask;  
    [SerializeField] private float groundRayLength = 0.2f;

    Rigidbody _rb;
    Collider myCollider;
    public Rigidbody Rb => _rb;

    private int partsCollected = 0;
    private bool isRepairing;
    private bool finishedRepair;
    private bool isWaitingForJump = false;
    private bool playerWantsJump = false;

    public void SetPlayerWantsJump(bool value) => playerWantsJump = value;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
        isRepairing = false;
        finishedRepair = false;
    }

    public void Move(Vector3 dir)
    {
        Vector3 velocity = dir.normalized * speed;
        velocity.y = _rb.velocity.y; // mantener la gravedad
        _rb.velocity = velocity;
    }

    public void LookDir(Vector3 dir)
    {
        if (dir == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    public Vector3 GetDir()
    {
        return transform.forward;
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(partTag))
        {
            PartRobot robotPart = other.GetComponent<PartRobot>();
            if (robotPart != null && !robotPart.IsAttached() && !isRepairing)
            {
                Debug.Log($"Robot collecting part: {other.gameObject.name}");
                robotPart.AttachToRobot(union);
                partsCollected++;

                if (partsCollected >= 1)
                {
                    isRepairing = true;
                }

            }
            else if (robotPart == null)
            {
                Debug.LogWarning($"Object with tag '{partTag}' is missing PartRobot script: {other.gameObject.name}");
            }
            else if (robotPart.IsAttached())
            {
                Debug.Log($"Robot collided with already attached part: {other.gameObject.name}");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if (!isWaitingForJump) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // Verificamos si el jugador viene desde arriba
            ContactPoint contact = collision.contacts[0];
            Vector3 contactNormal = contact.normal;

            // Si la normal apunta hacia abajo, el contacto fue desde arriba
            if (Vector3.Dot(contactNormal, Vector3.down * 50f) > 0.5f)
            {
                Rigidbody playerRb = collision.rigidbody;
                if (playerRb != null)
                {
                    Vector3 jumpImpulse = Vector3.up * 8f; // Ajustá fuerza
                    playerRb.AddForce(jumpImpulse, ForceMode.Impulse);
                    Debug.Log("¡Impulso aplicado al jugador desde el conejo!");
                }
            }
        }
    }

    public void DesactiveCollider()
    {
        _rb.isKinematic = true;
        myCollider.enabled = false; 
    }

    public void ActiveCollider()
    {
        myCollider.enabled = true;
        _rb.isKinematic = false;
    }

    public void ChangeValueBool(bool value)
    {
        finishedRepair = value;
    }

    public bool IsRepairing() => isRepairing;
    public bool HasFinishedRepair() => finishedRepair;
    public bool IsWaitingForJump() => isWaitingForJump;
    public bool PlayerWantsJump() => playerWantsJump;

    public void SetWaitingForJump(bool waiting)
    {
        isWaitingForJump = waiting;
    }

    public bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, groundRayLength, groundMask);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawLine(origin, origin + Vector3.down * groundRayLength);
    }
}

