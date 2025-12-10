using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectRescueTrigger : MonoBehaviour
{
    [Header("Rescue Settings")]
    [SerializeField] private List<string> rescueObjectTags = new List<string> { "Throwable", "Grabbable" };
    [SerializeField] private DialogueData rescueDialogues;
    [SerializeField] private float rescueSpeed = 5f;
    [SerializeField] private float returnSpeed = 3f;
    [SerializeField] private Vector3 finalOffset = new Vector3(0, 0, 2f);

    [Header("Rescue Point")]
    [SerializeField] private Transform rescuePoint;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem rescueParticles;
    [SerializeField] private ParticleSystem returnParticles;

    private bool isRescuing = false;
    private Transform playerTransform;
    private GameObject currentRescueObject;
    private InteractionManager interactionManager;
    private Dictionary<GameObject, (MonoBehaviour pickableComponent, Collider[] originalColliders)> originalObjectStates = new Dictionary<GameObject, (MonoBehaviour, Collider[])>();
    private bool wasRescueTriggerOriginally;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;

            PlayerManager playerManager = player.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                interactionManager = playerManager.interactionManager;
            }
        }

        // Guardar estado original del trigger
        Collider rescueCollider = GetComponent<Collider>();
        if (rescueCollider != null)
        {
            wasRescueTriggerOriginally = rescueCollider.isTrigger;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isRescuing || !rescueObjectTags.Contains(other.tag)) return;

        StartRescueSequence(other.gameObject);
    }

    private void StartRescueSequence(GameObject objectToRescue)
    {
        isRescuing = true;
        currentRescueObject = objectToRescue;

        //  CAMBIAR EL TRIGGER A COLLIDER NORMAL para evitar más rescates
        Collider rescueCollider = GetComponent<Collider>();
        if (rescueCollider != null)
        {
            rescueCollider.isTrigger = false;
            Debug.Log(" ObjectRescueTrigger cambiado a collider normal");
        }

        // Forzar al jugador a soltar el objeto si lo está agarrando
        ForcePlayerToReleaseObject();

        // QUITAR COLLIDERS del objeto rescatado
        RemoveObjectColliders(objectToRescue);

        // Deshabilitar temporalmente la capacidad de agarrar el objeto
        DisableObjectPickup(objectToRescue);

        // Desactivar física temporalmente
        Rigidbody objectRb = currentRescueObject.GetComponent<Rigidbody>();
        if (objectRb != null)
        {
            objectRb.isKinematic = true;
            objectRb.velocity = Vector3.zero;
            objectRb.angularVelocity = Vector3.zero;
        }

        DialogueData.DialogueEntry rescueDialogue = GetRandomRescueDialogue();

        if (!AssistantManager.Instance.IsAssistantActive())
        {
            AssistantManager.Instance.ShowExternalDialogueWithCallback(
                rescueDialogue.dialogueText,
                rescueDialogue.displayDuration,
                rescueDialogue.voiceClip,
                () => StartCoroutine(RescueObjectCoroutine())
            );
        }
        else
        {
            AssistantManager.Instance.ShowExternalDialogueWithCallback(
                rescueDialogue.dialogueText,
                rescueDialogue.displayDuration,
                rescueDialogue.voiceClip,
                () => StartCoroutine(RescueObjectCoroutine())
            );
        }
    }

    private void ForcePlayerToReleaseObject()
    {
        if (interactionManager != null)
        {
            interactionManager.ForceReleaseObject();
        }
        else
        {
            Debug.LogWarning("InteractionManager no encontrado en el jugador");
        }
    }

    private void DisableObjectPickup(GameObject obj)
    {
        // Buscar y deshabilitar todos los componentes que implementen IPickable
        MonoBehaviour[] allComponents = obj.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in allComponents)
        {
            if (component is IPickable)
            {
                if (!originalObjectStates.ContainsKey(obj))
                {
                    originalObjectStates[obj] = (component, null);
                }
                else
                {
                    var (_, originalColliders) = originalObjectStates[obj];
                    originalObjectStates[obj] = (component, originalColliders);
                }

                component.enabled = false;
                Debug.Log($" IPickable deshabilitado en {obj.name}");
                return;
            }
        }

        Debug.LogWarning($" No se encontró componente IPickable en {obj.name}");
    }

    private void RemoveObjectColliders(GameObject obj)
    {
        // Obtener todos los colliders del objeto y sus hijos
        Collider[] allColliders = obj.GetComponentsInChildren<Collider>();

        if (allColliders.Length > 0)
        {
            // Guardar referencia a los colliders originales
            if (!originalObjectStates.ContainsKey(obj))
            {
                originalObjectStates[obj] = (null, allColliders);
            }
            else
            {
                var (pickableComponent, _) = originalObjectStates[obj];
                originalObjectStates[obj] = (pickableComponent, allColliders);
            }

            // DESHABILITAR TODOS LOS COLLIDERS
            foreach (Collider collider in allColliders)
            {
                collider.enabled = false;
            }

            Debug.Log($" Removidos {allColliders.Length} colliders de {obj.name}");
        }
        else
        {
            Debug.LogWarning($"No se encontraron colliders en {obj.name}");
        }
    }

    private void RestoreObjectState(GameObject obj)
    {
        if (originalObjectStates.ContainsKey(obj))
        {
            var (pickableComponent, originalColliders) = originalObjectStates[obj];

            // Restaurar componente IPickable
            if (pickableComponent != null)
            {
                pickableComponent.enabled = true;
                Debug.Log($" IPickable restaurado en {obj.name}");
            }

            // Restaurar colliders
            if (originalColliders != null)
            {
                foreach (Collider collider in originalColliders)
                {
                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                }
                Debug.Log($" Restaurados {originalColliders.Length} colliders en {obj.name}");
            }

            originalObjectStates.Remove(obj);
        }

        // RESTAURAR EL TRIGGER DEL OBJECTRESCUETRIGGER
        Collider rescueCollider = GetComponent<Collider>();
        if (rescueCollider != null)
        {
            rescueCollider.isTrigger = wasRescueTriggerOriginally;
            Debug.Log(" ObjectRescueTrigger restaurado a estado trigger original");
        }
    }

    private DialogueData.DialogueEntry GetRandomRescueDialogue()
    {
        if (rescueDialogues != null && rescueDialogues.dialogueEntries.Count > 0)
        {
            return rescueDialogues.GetRandomDialogue();
        }

        return new DialogueData.DialogueEntry
        {
            dialogueText = "¡What are you doing!",
            displayDuration = 3f
        };
    }

    private IEnumerator RescueObjectCoroutine()
    {
        GameObject assistant = AssistantManager.Instance.GetCurrentAssistantObject();
        if (assistant == null || currentRescueObject == null) yield break;

        // Fase 1: Asistente va al objeto
        yield return StartCoroutine(MoveAssistantToPosition(assistant, currentRescueObject.transform.position, rescueSpeed));

        if (rescueParticles != null)
        {
            Instantiate(rescueParticles, currentRescueObject.transform.position, Quaternion.identity);
        }

        // Fase 2: Asistente lleva el objeto al rescuePoint
        yield return StartCoroutine(MoveToRescuePoint(assistant, currentRescueObject));

        // Fase 3: Asistente regresa al jugador con el objeto
        yield return StartCoroutine(ReturnToPlayer(assistant, currentRescueObject));

        CompleteRescue();
    }

    private IEnumerator MoveAssistantToPosition(GameObject assistant, Vector3 targetPosition, float speed)
    {
        Vector3 startPosition = assistant.transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float duration = distance / speed;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (assistant == null) yield break;

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            assistant.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        if (assistant != null)
        {
            assistant.transform.position = targetPosition;
        }
    }

    private IEnumerator MoveToRescuePoint(GameObject assistant, GameObject objectToRescue)
    {
        if (rescuePoint == null || assistant == null || objectToRescue == null) yield break;

        Vector3 assistantStartPos = assistant.transform.position;
        Vector3 objectStartPos = objectToRescue.transform.position;

        float distance = Vector3.Distance(assistantStartPos, rescuePoint.position);
        float duration = distance / rescueSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (assistant == null || objectToRescue == null) yield break;

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            assistant.transform.position = Vector3.Lerp(assistantStartPos, rescuePoint.position, t);
            objectToRescue.transform.position = Vector3.Lerp(objectStartPos, rescuePoint.position, t);

            yield return null;
        }

        if (assistant != null) assistant.transform.position = rescuePoint.position;
        if (objectToRescue != null) objectToRescue.transform.position = rescuePoint.position;

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ReturnToPlayer(GameObject assistant, GameObject objectToRescue)
    {
        if (playerTransform == null || assistant == null || objectToRescue == null) yield break;

        Vector3 finalPosition = GetFinalPosition();
        Vector3 assistantStartPos = assistant.transform.position;
        Vector3 objectStartPos = objectToRescue.transform.position;

        float distance = Vector3.Distance(assistantStartPos, finalPosition);
        float duration = distance / returnSpeed;
        float elapsedTime = 0f;

        if (returnParticles != null)
        {
            ParticleSystem particles = Instantiate(returnParticles, objectStartPos, Quaternion.identity);
            particles.transform.SetParent(objectToRescue.transform);
        }

        while (elapsedTime < duration)
        {
            if (assistant == null || objectToRescue == null) yield break;

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            assistant.transform.position = Vector3.Lerp(assistantStartPos, finalPosition, t);
            objectToRescue.transform.position = Vector3.Lerp(objectStartPos, finalPosition, t);

            yield return null;
        }

        if (assistant != null) assistant.transform.position = finalPosition;
        if (objectToRescue != null) objectToRescue.transform.position = finalPosition;
    }

    private Vector3 GetFinalPosition()
    {
        if (playerTransform == null)
            return Vector3.zero;

        return playerTransform.position +
               playerTransform.forward * finalOffset.z +
               playerTransform.up * finalOffset.y +
               playerTransform.right * finalOffset.x;
    }

    private void CompleteRescue()
    {
        if (currentRescueObject != null)
        {
            Rigidbody objectRb = currentRescueObject.GetComponent<Rigidbody>();
            if (objectRb != null)
            {
                objectRb.isKinematic = false;
                objectRb.AddForce(playerTransform.forward * 2f, ForceMode.Impulse);
            }

            //  RESTAURAR EL OBJETO COMPLETAMENTE
            RestoreObjectState(currentRescueObject);
        }

        currentRescueObject = null;
        isRescuing = false;

        Debug.Log(" Rescate completado! Objeto desbloqueado y listo para usar.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, GetComponent<Collider>().bounds.size);

        if (rescuePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(rescuePoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, rescuePoint.position);
        }

        if (playerTransform != null)
        {
            Gizmos.color = Color.green;
            Vector3 finalPos = GetFinalPosition();
            Gizmos.DrawWireSphere(finalPos, 0.3f);
            Gizmos.DrawLine(playerTransform.position, finalPos);
        }
    }
}
