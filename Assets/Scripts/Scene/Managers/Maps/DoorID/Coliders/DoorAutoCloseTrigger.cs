using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAutoCloseTrigger : MonoBehaviour
{
    [Tooltip("¿Es este el trigger del lado A? (Si no, se asume que es el lado B)")]
    [SerializeField] private bool isSideA = true;

    private TimeDoor parentDoor;
    private Collider triggerCollider;
    private bool playerPassedThrough = false;

    private void Start()
    {
        parentDoor = GetComponentInParent<TimeDoor>();
        if (parentDoor == null)
        {
            enabled = false;
            return;
        }

        triggerCollider = GetComponent<Collider>();
        if (!triggerCollider.isTrigger)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !playerPassedThrough)
        {
            playerPassedThrough = true;
            StartCoroutine(HandleDoorClosing());
        }
    }

    private IEnumerator HandleDoorClosing()
    {
        // Activar el closingBlocker inmediatamente
        if (parentDoor.closingBlocker != null)
        {
            parentDoor.closingBlocker.gameObject.SetActive(true);
            parentDoor.closingBlocker.isTrigger = false;
        }

        // Si la puerta ya está completamente abierta, cerrar inmediatamente
        if (parentDoor.IsOpen() && !parentDoor.IsMoving)
        {
            parentDoor.CloseDoor(isSideA);
        }
        else
        {
            // Si la puerta todavía se está abriendo, esperar a que termine
            while (parentDoor.IsMoving)
            {
                yield return null;
            }

            // Pequeña pausa para asegurar que está completamente abierta
            yield return new WaitForSeconds(0.1f);

            // Cerrar la puerta
            parentDoor.CloseDoor(isSideA);
        }

        // Resetear después de un tiempo para permitir futuros usos
        yield return new WaitForSeconds(2f);
        playerPassedThrough = false;
    }

    // Método para resetear manualmente si es necesario
    public void ResetTrigger()
    {
        playerPassedThrough = false;
    }
}