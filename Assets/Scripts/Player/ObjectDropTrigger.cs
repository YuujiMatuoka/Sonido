using UnityEngine;
[RequireComponent(typeof(Collider))]
public class ObjectDropTrigger : MonoBehaviour
{
    [Tooltip("Si se debe destruir el trigger después de soltar el objeto (opcional)")]
    public bool destroyAfterUse = false;

    [Tooltip("Tag del jugador para buscar automáticamente el InteractionManager")]
    public string playerTag = "Player";

    [SerializeField] private DialogueData dialogueData;

    private PlayerManager playerManager;
    private bool isPlayerInside = false;
    private GameObject currentPlayer;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerManager = playerObj.GetComponent<PlayerManager>();
        }

        if (playerManager == null)
        {
            Debug.LogWarning($"ObjectDropTrigger: No se encontró PlayerManager con tag {playerTag}", this);
        }
    }

    private void Update()
    {
        // Si el jugador está dentro del trigger, verificar constantemente si agarra un objeto
        if (isPlayerInside && playerManager != null && playerManager.interactionManager != null)
        {
            var interactionManager = playerManager.interactionManager;

            // Si está sosteniendo un objeto, forzar soltarlo
            if (interactionManager.CurrentHedlObject != null)
            {
                TriggerDrop(interactionManager);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = true;
            currentPlayer = other.gameObject;

            // Verificar inmediatamente si ya tiene un objeto agarrado
            if (playerManager != null && playerManager.interactionManager != null)
            {
                var interactionManager = playerManager.interactionManager;
                if (interactionManager.CurrentHedlObject != null)
                {
                    TriggerDrop(interactionManager);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = false;
            currentPlayer = null;
        }
    }

    private void TriggerDrop(InteractionManager interactionManager)
    {
        string objectName = GetObjectName(interactionManager.CurrentHedlObject);

        // Forzar soltar el objeto
        interactionManager.ForceReleaseObject();

        // Mostrar diálogo si está configurado (solo la primera vez o cada vez, según prefieras)
        if (dialogueData != null && AssistantManager.Instance != null)
        {
            ShowDynamicDialogue(objectName);
        }

        if (destroyAfterUse)
        {
            Destroy(gameObject);
        }
    }

    private string GetObjectName(IPickable heldObject)
    {
        if (heldObject == null)
            return "objeto";

        if (heldObject is MonoBehaviour monoBehaviour)
        {
            return monoBehaviour.gameObject.name;
        }

        return "objeto";
    }

    private void ShowDynamicDialogue(string objectName)
    {
        var randomEntry = dialogueData.GetRandomDialogue();
        if (randomEntry != null)
        {
            string originalText = randomEntry.dialogueText;
            string dynamicText = originalText.Replace("{objectName}", objectName);
            float duration = randomEntry.displayDuration > 0 ? randomEntry.displayDuration : 5f;

            AudioClip clip = randomEntry.voiceClip;

            AssistantManager.Instance.ShowExternalDialogue(dynamicText, duration, clip);
        }
    }

}