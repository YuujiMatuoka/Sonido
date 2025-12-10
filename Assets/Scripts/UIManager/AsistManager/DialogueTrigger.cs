using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    private void Start()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.RegisterSource(audioSource);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
            return;

        if (hasTriggered && triggerOnce) return;

        if (other.CompareTag("Player"))
        {
            if (dialogueData != null && AssistantManager.Instance != null)
            {
                AssistantManager.Instance.ShowDialogueFromData(dialogueData);
                hasTriggered = true;
            }
        }
    }

    private void OnValidate()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }
}
