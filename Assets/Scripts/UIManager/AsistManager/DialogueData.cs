using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Game/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueEntry
    {
        [TextArea(3, 10)]
        public string dialogueText;
        public float displayDuration = 5f;
        public AudioClip voiceClip;
    }

    public List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();

    // Método para obtener una frase aleatoria
    public DialogueEntry GetRandomDialogue()
    {
        if (dialogueEntries == null || dialogueEntries.Count == 0)
            return null;

        return dialogueEntries[Random.Range(0, dialogueEntries.Count)];
    }

    // Nuevo método: obtener diálogo con reemplazo de variables
    public DialogueEntry GetRandomDialogueWithReplacement(string objectName)
    {
        var entry = GetRandomDialogue();
        if (entry != null && !string.IsNullOrEmpty(objectName))
        {
            // Crear una copia para no modificar el original
            var modifiedEntry = new DialogueEntry()
            {
                dialogueText = entry.dialogueText.Replace("{objectName}", objectName),
                displayDuration = entry.displayDuration,
                voiceClip = entry.voiceClip
            };
            return modifiedEntry;
        }
        return entry;
    }
}