using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class AssistantManager : MonoBehaviour
{
    public static AssistantManager Instance { get; private set; }

    [Header("Assistant References")]
    [SerializeField] private GameObject assistantPrefab;
    [SerializeField] private Transform assistantSpawnPoint;
    [SerializeField] private Animator playerAnimator;

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Dialogue Settings")]
    [SerializeField] private DialogueData assistantDialogues;
    [SerializeField] private float defaultDisplayDuration = 5f;

    [Header("Animation Settings")]
    [SerializeField] private string summonAnimation = "SummonAssistant";
    [SerializeField] private string dismissAnimation = "DismissAssistant";

    [Header("Movement Settings")]
    [SerializeField] private float followMoveTime = 0.3f;
    [SerializeField] private float followRotateSpeed = 5.0f;

    private GameObject currentAssistant;
    private bool isAssistantActive = false;
    private bool isPaused = false;
    private AudioSource currentAudioSource;
    private float remainingDialogueTime = 0f;
    private string currentDialogueText = "";
    private bool isAnimating = false;

    private Coroutine currentDialogueCoroutine;
    private Coroutine assistantLifetimeCoroutine;
    private List<string> usedDialogues = new List<string>();
    private List<string> availableDialogues = new List<string>();
    private AssistantVisuals currentAssistantVisuals;
    private Vector3 assistantMoveVelocity = Vector3.zero;

    private bool isInRescueMode = false;
    private Vector3 rescueTargetPosition;
    private System.Action onRescueComplete;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        ResetAvailableDialogues();
    }

    private void OnGameStateChanged(GameState newState)
    {
        bool wasPaused = isPaused;
        isPaused = (newState == GameState.Paused);

        if (isPaused)
        {
            PauseAssistant();
        }
        else
        {
            if (!isAssistantActive)
            {
                ForceCleanup();
            }
            else
            {
                ResumeAssistant();
            }
        }
    }
    private void ForceCleanup()
    {
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
        }

        if (assistantLifetimeCoroutine != null)
        {
            StopCoroutine(assistantLifetimeCoroutine);
            assistantLifetimeCoroutine = null;
        }

        remainingDialogueTime = 0f;
        currentDialogueText = "";

        HideDialogue();
    }

    private void PauseAssistant()
    {
        if (currentAudioSource != null && currentAudioSource.isPlaying)
        {
            currentAudioSource.Pause();
        }

        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;

            if (remainingDialogueTime <= 0)
            {
                remainingDialogueTime = defaultDisplayDuration * 0.5f; 
            }
        }
    }

    private void ResumeAssistant()
    {
        if (isAssistantActive && remainingDialogueTime > 0 && !string.IsNullOrEmpty(currentDialogueText))
        {
            if (currentAudioSource != null && currentAudioSource.clip != null)
            {
                currentAudioSource.UnPause();
            }

            currentDialogueCoroutine = StartCoroutine(ResumeDialogueCoroutine(currentDialogueText, remainingDialogueTime));
        }
        else
        {
            ForceCleanup();
        }
    }

    private IEnumerator ResumeDialogueCoroutine(string text, float remainingTime)
    {
        if (dialoguePanel != null && !dialoguePanel.activeInHierarchy)
        {
            dialoguePanel.SetActive(true);
        }

        if (dialogueText != null)
        {
            dialogueText.text = text;
        }

        float timer = 0f;
        while (timer < remainingTime && !isPaused)
        {
            if (!isPaused)
            {
                timer += Time.unscaledDeltaTime;
            }
            yield return null;
        }

        if (!isPaused)
        {
            HideDialogue();
            remainingDialogueTime = 0f;
            currentDialogueText = "";
        }
    }

    private void Update()
    {
        if (currentAssistant != null)
        {
            if (isInRescueMode && isAssistantActive)
            {
                // En modo rescate, el asistente se mueve libremente
                // El movimiento se controla desde el ObjectRescueTrigger
            }
            else if (assistantSpawnPoint != null)
            {
                FollowSpawnPointSmoothly();
            }
        }
    }

    private void FollowSpawnPointSmoothly()
    {
        if (assistantSpawnPoint == null) return;
        currentAssistant.transform.position = Vector3.SmoothDamp(
            currentAssistant.transform.position,
            assistantSpawnPoint.position,
            ref assistantMoveVelocity,
            followMoveTime
        );
        currentAssistant.transform.rotation = Quaternion.Slerp(
            currentAssistant.transform.rotation,
            assistantSpawnPoint.rotation,
            Time.deltaTime * followRotateSpeed
        );
    }

    public void ToggleAssistant()
    {
        if (isAnimating) return;

        if (isAssistantActive)
        {
            StartCoroutine(DismissAssistantCoroutine());
        }
        else
        {
            StartCoroutine(SummonAssistantCoroutine(true));
        }
    }

    private IEnumerator SummonAssistantCoroutine(bool playRandomDialogue)
    {
        isAnimating = true;

        if (playerAnimator != null) playerAnimator.SetTrigger(summonAnimation);

        if (assistantPrefab != null && assistantSpawnPoint != null)
        {
            currentAssistant = Instantiate(assistantPrefab, assistantSpawnPoint.position, assistantSpawnPoint.rotation);
        }
        else
        {
            isAnimating = false;
            yield break;
        }

        isAssistantActive = true;
        currentAssistantVisuals = currentAssistant.GetComponent<AssistantVisuals>();

        if (currentAssistantVisuals != null)
        {
            currentAssistantVisuals.ReintegrateAll();
            yield return new WaitForSeconds(currentAssistantVisuals.dissolveDuration);
        }
        else
        {
            Debug.LogWarning("El Prefab del Asistente no tiene el script 'AssistantVisuals'.");
        }

        if (playRandomDialogue)
        {
            PlayRandomAssistantDialogue();
        }

        isAnimating = false;
    }

    private IEnumerator DismissAssistantCoroutine()
    {
        isAnimating = true;
        isAssistantActive = false;

        if (currentAudioSource != null && currentAudioSource.isPlaying)
        {
            currentAudioSource.Stop();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.UnregisterSource(currentAudioSource);
            }
        }

        if (currentAudioSource != null && currentAudioSource.isPlaying)
        {
            currentAudioSource.Stop();
        }

        if (assistantLifetimeCoroutine != null) StopCoroutine(assistantLifetimeCoroutine);
        if (currentDialogueCoroutine != null) StopCoroutine(currentDialogueCoroutine);
        assistantLifetimeCoroutine = null;
        currentDialogueCoroutine = null;

        if (playerAnimator != null) playerAnimator.SetTrigger(dismissAnimation);
        HideDialogue();

        if (currentAssistantVisuals != null)
        {
            yield return currentAssistantVisuals.DisintegrateAll();
        }

        DestroyAssistantImmediate();
        isAnimating = false;
    }

    private void PlayRandomAssistantDialogue()
    {
        if (assistantDialogues == null || assistantDialogues.dialogueEntries.Count == 0)
        {
            ShowDialogue("¡Hi! ¿Can I help you?", defaultDisplayDuration);
            StartAssistantLifetime(defaultDisplayDuration);
            return;
        }

        if (availableDialogues.Count == 0)
        {
            ResetAvailableDialogues();
        }

        DialogueData.DialogueEntry randomEntry = GetRandomDialogueEntryFromAvailable();

        if (randomEntry != null && !string.IsNullOrEmpty(randomEntry.dialogueText))
        {
            float duration = randomEntry.displayDuration > 0 ? randomEntry.displayDuration : defaultDisplayDuration;
            ShowDialogue(randomEntry.dialogueText, duration);
            StartAssistantLifetime(duration);

            if (randomEntry.voiceClip != null && currentAssistant != null)
            {
                PlayAudioClip(randomEntry.voiceClip);
            }
        }
    }

    private void PlayAudioClip(AudioClip clip)
    {
        if (clip != null && currentAssistant != null)
        {
            AudioSource assistantAudioSource = currentAssistant.GetComponent<AudioSource>();
            if (assistantAudioSource == null)
            {
                assistantAudioSource = currentAssistant.AddComponent<AudioSource>();
                assistantAudioSource.spatialBlend = 1f;
                assistantAudioSource.rolloffMode = AudioRolloffMode.Linear;
                assistantAudioSource.maxDistance = 15f;
                assistantAudioSource.minDistance = 1f;

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.RegisterSource(assistantAudioSource);
                }
            }

            currentAudioSource = assistantAudioSource;
            currentAudioSource.clip = clip;
            currentAudioSource.Play();
        }
    }

    private void StartAssistantLifetime(float duration)
    {
        if (assistantLifetimeCoroutine != null)
        {
            StopCoroutine(assistantLifetimeCoroutine);
        }
        assistantLifetimeCoroutine = StartCoroutine(DestroyAssistantAfterDialogue(duration));
    }

    private DialogueData.DialogueEntry GetRandomDialogueEntryFromAvailable()
    {
        if (assistantDialogues.dialogueEntries.Count == 0) return null;

        var availableEntries = new List<DialogueData.DialogueEntry>();
        foreach (var entry in assistantDialogues.dialogueEntries)
        {
            if (!usedDialogues.Contains(entry.dialogueText) && !string.IsNullOrEmpty(entry.dialogueText))
            {
                availableEntries.Add(entry);
            }
        }

        if (availableEntries.Count == 0)
        {
            ResetAvailableDialogues();
            availableEntries = new List<DialogueData.DialogueEntry>(assistantDialogues.dialogueEntries);
        }

        if (availableEntries.Count > 0)
        {
            int randomIndex = Random.Range(0, availableEntries.Count);
            DialogueData.DialogueEntry selectedEntry = availableEntries[randomIndex];
            usedDialogues.Add(selectedEntry.dialogueText);
            availableDialogues.Remove(selectedEntry.dialogueText);
            return selectedEntry;
        }

        return null;
    }

    private IEnumerator DestroyAssistantAfterDialogue(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (isAnimating) yield break;

        if (isAssistantActive)
        {
            StartCoroutine(DismissAssistantCoroutine());
        }
    }

    private void DestroyAssistantImmediate()
    {
        if (currentAssistant != null)
        {
            if (currentAudioSource != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.UnregisterSource(currentAudioSource);
            }

            Destroy(currentAssistant);
            currentAssistant = null;
            currentAudioSource = null;
        }
    }

    private void ResetAvailableDialogues()
    {
        availableDialogues.Clear();
        foreach (var entry in assistantDialogues.dialogueEntries)
        {
            if (!string.IsNullOrEmpty(entry.dialogueText))
            {
                availableDialogues.Add(entry.dialogueText);
            }
        }
        usedDialogues.Clear();
    }

    public void ShowDialogue(string text, float duration = 5f)
    {
        if (isPaused) return;

        if (currentDialogueCoroutine != null)
            StopCoroutine(currentDialogueCoroutine);

        currentDialogueText = text;
        remainingDialogueTime = duration;
        currentDialogueCoroutine = StartCoroutine(DisplayDialogueCoroutine(text, duration));
    }

    public void ShowDialogueFromData(DialogueData dialogueData)
    {
        if (dialogueData != null && dialogueData.dialogueEntries.Count > 0)
        {
            var randomEntry = dialogueData.GetRandomDialogue();
            if (randomEntry != null)
            {
                if (isAnimating || isPaused) return;

                if (!isAssistantActive)
                {
                    StartCoroutine(SummonAssistantWithSpecificDialogueCoroutine(randomEntry.dialogueText, randomEntry.displayDuration, randomEntry.voiceClip));
                }
                else
                {
                    ShowDialogue(randomEntry.dialogueText, randomEntry.displayDuration);
                    if (randomEntry.voiceClip != null && currentAssistant != null)
                    {
                        PlayAudioClip(randomEntry.voiceClip);
                    }
                }
            }
        }
    }

    private IEnumerator DisplayDialogueCoroutine(string text, float duration)
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = text;

        float timer = 0f;
        while (timer < duration)
        {
            if (!isPaused)
            {
                timer += Time.deltaTime;
            }
            yield return null;
        }

        if (!isPaused)
        {
            HideDialogue();
            remainingDialogueTime = 0f;
            currentDialogueText = "";
        }
    }

    private void HideDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";
    }

    public bool IsAssistantActive()
    {
        return isAssistantActive;
    }

    public void ResetDialogues()
    {
        ResetAvailableDialogues();
    }

    public void ForceDismissAssistant()
    {
        if (isAssistantActive && !isAnimating)
        {
            StartCoroutine(DismissAssistantCoroutine());
        }
    }
    public void ShowExternalDialogueWithCallback(string text, float duration, AudioClip clip = null, System.Action onDialogueEnd = null)
    {
        if (isAnimating || isPaused) return;

        if (isAssistantActive)
        {
            ShowDialogue(text, duration);
            if (clip != null && currentAssistant != null)
            {
                PlayAudioClip(clip);
            }

            if (onDialogueEnd != null)
            {
                StartCoroutine(TriggerCallbackAfterDialogue(duration, onDialogueEnd));
            }
        }
        else
        {
            StartCoroutine(SummonAssistantWithSpecificDialogueCoroutine(text, duration, clip, onDialogueEnd));
        }
    }
    private IEnumerator TriggerCallbackAfterDialogue(float duration, System.Action callback)
    {
        yield return new WaitForSeconds(duration);
        callback?.Invoke();
    }
    private IEnumerator SummonAssistantWithSpecificDialogueCoroutine(string text, float duration, AudioClip clip = null, System.Action onDialogueEnd = null)
    {
        isAnimating = true;

        if (playerAnimator != null) playerAnimator.SetTrigger(summonAnimation);

        if (assistantPrefab != null && assistantSpawnPoint != null)
        {
            currentAssistant = Instantiate(assistantPrefab, assistantSpawnPoint.position, assistantSpawnPoint.rotation);
        }
        else
        {
            isAnimating = false;
            yield break;
        }

        isAssistantActive = true;
        currentAssistantVisuals = currentAssistant.GetComponent<AssistantVisuals>();

        if (currentAssistantVisuals != null)
        {
            currentAssistantVisuals.ReintegrateAll();
        }

        ShowDialogue(text, duration);
        StartAssistantLifetime(duration);

        if (clip != null && currentAssistant != null)
        {
            PlayAudioClip(clip);
        }

        if (onDialogueEnd != null)
        {
            StartCoroutine(TriggerCallbackAfterDialogue(duration, onDialogueEnd));
        }

        yield return new WaitForSeconds(0.5f);
        isAnimating = false;
    }
    public void ShowExternalDialogue(string text, float duration, AudioClip clip = null)
    {
        if (isAnimating || isPaused) return;

        if (isAssistantActive)
        {
            ShowDialogue(text, duration);
            if (clip != null && currentAssistant != null)
            {
                PlayAudioClip(clip);
            }
        }
        else
        {
            StartCoroutine(SummonAssistantWithSpecificDialogueCoroutine(text, duration, clip));
        }
    }

    private IEnumerator SummonAssistantWithSpecificDialogueCoroutine(string text, float duration, AudioClip clip = null)
    {
        isAnimating = true;

        if (playerAnimator != null) playerAnimator.SetTrigger(summonAnimation);

        if (assistantPrefab != null && assistantSpawnPoint != null)
        {
            currentAssistant = Instantiate(assistantPrefab, assistantSpawnPoint.position, assistantSpawnPoint.rotation);
        }
        else
        {
            isAnimating = false;
            yield break;
        }

        isAssistantActive = true;
        currentAssistantVisuals = currentAssistant.GetComponent<AssistantVisuals>();

        if (currentAssistantVisuals != null)
        {
            currentAssistantVisuals.ReintegrateAll();
        }

        ShowDialogue(text, duration);
        StartAssistantLifetime(duration);

        if (clip != null && currentAssistant != null)
        {
            PlayAudioClip(clip);
        }

        yield return new WaitForSeconds(0.5f);
        isAnimating = false;
    }
    public GameObject GetCurrentAssistantObject()
    {
        return currentAssistant;
    }
    public void StartRescueMission(Vector3 targetPosition, System.Action onComplete = null)
    {
        if (!isAssistantActive) return;

        isInRescueMode = true;
        rescueTargetPosition = targetPosition;
        onRescueComplete = onComplete;
    }
    public void EndRescueMission()
    {
        isInRescueMode = false;
        onRescueComplete?.Invoke();
        onRescueComplete = null;
    }
    public bool IsInRescueMode()
    {
        return isInRescueMode;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            GameManager.OnGameStateChanged -= OnGameStateChanged;
        }
    }
}