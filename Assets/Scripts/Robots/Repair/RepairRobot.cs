using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairRobot : MonoBehaviour
{
    [SerializeField] private Transform teleportTarget;
    [SerializeField] private Transform union;
    [SerializeField] private string partTag = "RobotPart";
    [SerializeField] private float prepareTime = 1f; // Tiempo para rotar y levantarse
    [SerializeField] private float liftForce = 5f; // Fuerza para levantar el robot

    private Rigidbody rb;
    private int partsCollected = 0;
    private bool isPreparing = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("RepairRobot necesita un Rigidbody para usar físicas.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(partTag))
        {
            PartRobot robotPart = other.GetComponent<PartRobot>();
            if (robotPart != null && !robotPart.IsAttached() && !isPreparing)
            {
                Debug.Log($"Robot collecting part: {other.gameObject.name}");
                robotPart.AttachToRobot(union);
                partsCollected++;

                if (partsCollected >= 1 && teleportTarget != null)
                {
                    isPreparing = true;
                    StartCoroutine(PrepareAndTeleport());
                }

                Destroy(other.GetComponent<PickableTimeTravelObject>());
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

    private IEnumerator PrepareAndTeleport()
    {
        Debug.Log($"Robot preparing for teleport in {prepareTime} seconds.");

        // Rotar el robot para que esté de pie (ejemplo: rotar alrededor del eje X)
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0f); // Rotar solo en z para levantarse
        float rotationTimer = 0f;
        while (rotationTimer < prepareTime)
        {
            rotationTimer += Time.deltaTime;
            float rotationProgress = Mathf.Clamp01(rotationTimer / prepareTime);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotationProgress));
            // O podrías usar AddTorque para una rotación basada en física más realista
            // Vector3 torque = Vector3.right * rotationSpeed * Time.deltaTime;
            // rb.AddTorque(torque);
            yield return null;
        }
        rb.MoveRotation(targetRotation); // Asegurar la rotación final

        // Mover el robot ligeramente hacia arriba (ejemplo: aplicar una fuerza)
        rb.AddForce(Vector3.up * liftForce, ForceMode.Impulse);
        yield return new WaitForSeconds(0.5f); // Esperar un poco para que se eleve

        //Teleport();
    }

    private void Teleport()
    {
        if (teleportTarget != null)
        {
            rb.isKinematic = true; // Desactivar la física antes de la teletransportación
            transform.position = teleportTarget.position;
            transform.rotation = teleportTarget.rotation;
            rb.isKinematic = false; // Reactivar la física después de la teletransportación (si es necesario)
            Debug.Log("Robot teleported!");
        }
        else
        {
            Debug.LogError("Teleport target not assigned in the Inspector!");
        }
    }
}
