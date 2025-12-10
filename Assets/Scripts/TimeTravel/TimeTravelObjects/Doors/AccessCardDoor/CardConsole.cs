using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CardConsole : TimeTravelObject, IInteractable
{
    [Header("Slot para tarjeta")]
    [SerializeField] private Transform cardSlot;

    [Header("Puerta vinculada")]
    [SerializeField] private TimeDoor linkedDoor;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip insertSound;
    [SerializeField] private AudioClip useSound;
    [SerializeField] private AudioClip deniedSound; // Opcional: Sonido de error

    [Header("Animación de inserción")]
    [SerializeField] private float insertionDuration = 1f;
    [SerializeField] private AnimationCurve insertionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("MaterialChange")]
    [SerializeField] private List<MaterialChange> materialChanges = new List<MaterialChange>();

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 2f;
    [SerializeField] private float deniedFeedbackDuration = 1f; // Cuánto dura la pantalla roja

    [Header("System Identification")]
    public int systemID;
    public int phase;

    private bool wasPlayingOnPause = false;
    private bool isActivatedL1 = false; // ¿Tiene tarjeta?
    private bool isAnimating = false;
    private bool isBInteract = false;
    private bool isOnCooldown = false;

    // Para controlar las corrutinas de feedback visual
    private Coroutine feedbackCoroutine;

    public bool IsActivated => isActivatedL1;

    protected override void Start()
    {
        base.Start();

        if (DoorSystemManager.Instance != null && systemID != 0)
        {
            DoorSystemManager.Instance.RegisterCardConsole(this, systemID);
        }

        if (audioSource != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.RegisterSource(audioSource);
        }

        GameManager.OnGameStateChanged += OnGameStateChanged;

        // Inicializar estado visual
        UpdateRestingVisuals();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<AccessCard>(out var card)) return;
        if (card.IsUsed) return;
        if (card.IsBroken()) return;
        if (isAnimating) return;

        var currentTime = TimeTravelManager.Instance.CurrentTimeState;

        // Solo aceptamos tarjeta si estamos en el tiempo correcto y la consola funciona
        if (currentTime == TimeState.L1 || (currentTime == TimeState.Origin && IsProtected()))
        {
            StartCoroutine(AnimateCardInsertion(card));
        }
    }

    private IEnumerator AnimateCardInsertion(AccessCard card)
    {
        PlayerManager playerManager = FindAnyObjectByType<PlayerManager>();
        if (playerManager != null) playerManager.interactionManager.ForceReleaseObject();

        isAnimating = true;

        // Desactivar físicas de la tarjeta
        card.GetComponent<Collider>().enabled = false;
        Rigidbody cardRb = card.GetComponent<Rigidbody>();
        if (cardRb != null)
        {
            cardRb.isKinematic = true;
            cardRb.velocity = Vector3.zero;
        }

        Transform cardTransform = card.transform;
        cardTransform.SetParent(cardSlot);

        // Animación de movimiento
        Vector3 startLocalPosition = cardTransform.localPosition;
        Quaternion startLocalRotation = cardTransform.localRotation;
        Vector3 targetLocalPosition = Vector3.zero;
        Quaternion targetLocalRotation = Quaternion.Euler(135f, 0f, 0f);

        float elapsedTime = 0f;
        while (elapsedTime < insertionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = insertionCurve.Evaluate(elapsedTime / insertionDuration);
            cardTransform.localPosition = Vector3.Lerp(startLocalPosition, targetLocalPosition, t);
            cardTransform.localRotation = Quaternion.Lerp(startLocalRotation, targetLocalRotation, t);
            yield return null;
        }

        cardTransform.localPosition = targetLocalPosition;
        cardTransform.localRotation = targetLocalRotation;

        CompleteCardInsertion(card);
        isAnimating = false;
    }

    private void CompleteCardInsertion(AccessCard card)
    {
        isActivatedL1 = true;

        // La tarjeta se insertó, cambiamos al estado "Ready"
        SetMaterialState(MaterialState.Ready);

        card.LockInPlace(cardSlot);

        if (audioSource && insertSound)
            audioSource.PlayOneShot(insertSound);
    }

    public override void OnTimeChanged(TimeState newTime)
    {
        base.OnTimeChanged(newTime);

        // Al cambiar de tiempo, cancelamos cualquier feedback momentáneo (Denied/Open)
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        isOnCooldown = false;

        // Actualizamos al estado de reposo correcto según si tiene tarjeta o no
        UpdateRestingVisuals();
    }


    public void BInteract()
    {
        isBInteract = true;
        Interact();
    }

    public void Interact()
    {
        bool useSceneA = !isBInteract;

        // 1. Si está en cooldown o animando, ignorar
        if (isOnCooldown || isAnimating)
        {
            isBInteract = false;
            return;
        }

        // 2. Si está Roto completamente, no hacer nada
        if (IsBroken())
        {
            isBInteract = false;
            return;
        }

        // 3. Verificar si está operativo en el tiempo actual
        bool isOperational = false;
        if (TimeTravelManager.Instance.CurrentTimeState == TimeState.L1) isOperational = true;
        else if (TimeTravelManager.Instance.CurrentTimeState == TimeState.Origin && IsProtected()) isOperational = true;

        if (!isOperational)
        {
            // Debug.Log("No operativo en Origen sin protección");
            isBInteract = false;
            return;
        }

        // 4. LOGICA DE FEEDBACK: ¿Tiene la tarjeta puesta?
        if (!isActivatedL1)
        {
            // CASO: DENIED (Funciona, pero falta tarjeta)
            // Mostramos Denied y salimos
            if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = StartCoroutine(ShowDeniedFeedback());

            isBInteract = false;
            return;
        }

        // 5. CASO: EXITO (Tiene tarjeta y funciona)
        if (linkedDoor != null)
        {
            StartCoroutine(StartCooldownAndFeedback()); // Inicia cooldown y visual Open

            if (audioSource && useSound)
                audioSource.PlayOneShot(useSound);

            if (linkedDoor.IsOpen())
                linkedDoor.CloseDoor(useSceneA);
            else
                linkedDoor.OpenDoor(useSceneA);
        }

        isBInteract = false;
    }


    private IEnumerator ShowDeniedFeedback()
    {
        isOnCooldown = true; // Breve cooldown para evitar spam visual

        // Mostrar visual DENIED
        SetMaterialState(MaterialState.Denied);

        if (audioSource && deniedSound) audioSource.PlayOneShot(deniedSound);

        yield return new WaitForSeconds(deniedFeedbackDuration);

        // Volver al estado de reposo (OnNeed porque no hay tarjeta)
        UpdateRestingVisuals();

        isOnCooldown = false;
        feedbackCoroutine = null;
    }

    private IEnumerator StartCooldownAndFeedback()
    {
        isOnCooldown = true;

        // Mostrar visual OPEN (Éxito)
        SetMaterialState(MaterialState.Open);

        yield return new WaitForSeconds(cooldown);

        // Volver al estado de reposo (Ready porque sí hay tarjeta)
        UpdateRestingVisuals();

        isOnCooldown = false;
    }

    // Método maestro para decidir el estado de reposo
    private void UpdateRestingVisuals()
    {
        if (IsBroken() && !IsProtected())
        {
            // Si está roto visualmente no hacemos nada o apagamos, 
            // pero tu sistema de materiales parece ser solo para pantallas encendidas.
            // Aquí podrías poner un material de "Apagado" si tuvieras.
            return;
        }

        bool isOperational = false;
        if (TimeTravelManager.Instance.CurrentTimeState == TimeState.L1) isOperational = true;
        else if (TimeTravelManager.Instance.CurrentTimeState == TimeState.Origin && IsProtected()) isOperational = true;

        if (isOperational)
        {
            if (isActivatedL1)
            {
                SetMaterialState(MaterialState.Ready); // Funciona y tiene tarjeta
            }
            else
            {
                SetMaterialState(MaterialState.OnNeed); // Funciona pero necesita tarjeta
            }
        }
    }

    // Enum interno para facilitar la lectura
    private enum MaterialState { Open, Ready, OnNeed, Denied }

    // Método helper para aplicar el cambio a todos los materiales registrados
    private void SetMaterialState(MaterialState state)
    {
        foreach (var matChange in materialChanges)
        {
            if (matChange == null) continue;

            switch (state)
            {
                case MaterialState.Open:
                    matChange.ChangeMaterialToL1Open();
                    break;
                case MaterialState.Ready:
                    matChange.ChangeMaterialL1Ready();
                    break;
                case MaterialState.OnNeed:
                    matChange.ChangeMaterialL1OnNeed();
                    break;
                case MaterialState.Denied:
                    matChange.ChangeMaterialL1Denied();
                    break;
            }
        }
    }


    private void OnGameStateChanged(GameState newState)
    {
        if (audioSource == null) return;

        if (newState == GameState.Paused)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
                wasPlayingOnPause = true;
            }
        }
        else if (newState == GameState.Playing && wasPlayingOnPause)
        {
            audioSource.UnPause();
            wasPlayingOnPause = false;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameManager.OnGameStateChanged -= OnGameStateChanged;
        if (audioSource != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.UnregisterSource(audioSource);
        }
    }

    public bool WasActivatedInL1() => isActivatedL1;
}