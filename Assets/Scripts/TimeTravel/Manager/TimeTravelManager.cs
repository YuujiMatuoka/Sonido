using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if AK_WWISE_ADDRESSABLES
using AK.Wwise.Unity.WwiseAddressables;
#else
using AK.Wwise;
#endif

public class TimeTravelManager : MonoBehaviour
{
    public static TimeTravelManager Instance;

    [Header("Time Travel Settings")]
    public bool WaitTimeTravel;
    public float DelayTimeTravel = 3f;

    [Header("Wwise Integration")]
    [SerializeField] private AK.Wwise.State musicStateOrigin;
    [SerializeField] private AK.Wwise.State musicStateL1;
    //[SerializeField] private AK.Wwise.RTPC timeTravelRTPC;
    [SerializeField] private float timeTravelTransitionTime = 1.0f;
    [SerializeField] private AK.Wwise.Event timeTravelSoundEvent;

    public TimeState CurrentTimeState { get; private set; } = TimeState.Origin;
    public TimeState PreviousTimeState { get; private set; } = TimeState.Origin;

    private List<ITimeTravel> observers = new List<ITimeTravel>();
    private GameManager gameManager;
    private Coroutine timeTravelCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        gameManager = FindObjectOfType<GameManager>();
        WaitTimeTravel = false;

        // Inicializar música según el estado inicial
        InitializeWwiseMusic();
    }

    private void InitializeWwiseMusic()
    {
        // Configurar la música inicial según el estado actual
        switch (CurrentTimeState)
        {
            case TimeState.Origin:
                musicStateOrigin.SetValue();
                break;
            case TimeState.L1:
                musicStateL1.SetValue();
                break;
        }
    }

    public void ToggleTime()
    {
        if (WaitTimeTravel) return;

        TimeState newState = (CurrentTimeState == TimeState.Origin) ? TimeState.L1 : TimeState.Origin;
        StartTimeTravelTransition(newState);
    }

    public void ChangeTime(TimeState newTime)
    {
        if (WaitTimeTravel) return;

        StartTimeTravelTransition(newTime);
    }

    private void StartTimeTravelTransition(TimeState newState)
    {
        if (timeTravelCoroutine != null)
            StopCoroutine(timeTravelCoroutine);

        timeTravelCoroutine = StartCoroutine(TimeTravelTransitionRoutine(newState));
    }

    private IEnumerator TimeTravelTransitionRoutine(TimeState newState)
    {
        WaitTimeTravel = true;

        // 1. Reproducir sonido de viaje en el tiempo
        if (timeTravelSoundEvent != null && timeTravelSoundEvent.IsValid())
        {
            timeTravelSoundEvent.Post(gameObject);
        }

        // 2. Iniciar transición de RTPC (opcional, para efectos de transición)
        //if (timeTravelRTPC != null && timeTravelRTPC.IsValid())
        //{
        //    timeTravelRTPC.SetGlobalValue(1.0f); // Iniciar transición
        //}

        // 3. Pequeña pausa antes del cambio de estado
        yield return new WaitForSeconds(0.5f);

        // 4. Cambiar estado de música en Wwise
        ChangeWwiseMusicState(newState);

        // 5. Cambiar estado del juego
        PreviousTimeState = CurrentTimeState;
        CurrentTimeState = newState;

        // 6. Notificar a los objetos (manteniendo tu lógica original)
        foreach (var observer in observers)
        {
            observer.PreTimeChange(CurrentTimeState);
        }

        foreach (var observer in observers)
        {
            observer.OnTimeChanged(CurrentTimeState);
        }

        // 7. Esperar tiempo de transición completo
        yield return new WaitForSeconds(DelayTimeTravel - 0.5f);

        //// 8. Finalizar transición RTPC
        //if (timeTravelRTPC != null && timeTravelRTPC.IsValid())
        //{
        //    timeTravelRTPC.SetGlobalValue(0.0f); // Finalizar transición
        //}

        // 9. Cambiar estado del juego
        if (gameManager != null)
        {
            gameManager.SetGameState(GameState.Playing);
        }

        // 10. Desbloquear viajes en el tiempo
        WaitTimeTravel = false;
        timeTravelCoroutine = null;
    }

    private void ChangeWwiseMusicState(TimeState newState)
    {
        switch (newState)
        {
            case TimeState.Origin:
                if (musicStateOrigin != null && musicStateOrigin.IsValid())
                {
                    musicStateOrigin.SetValue();
                    Debug.Log("Wwise: Cambiado a estado de música - Origen (Presente)");
                }
                break;

            case TimeState.L1:
                if (musicStateL1 != null && musicStateL1.IsValid())
                {
                    musicStateL1.SetValue();
                    Debug.Log("Wwise: Cambiado a estado de música - L1 (Pasado)");
                }
                break;
        }
    }

    public void RequestObjectTimeTravel(TimeTwinLink objectToTravel, TimeState targetTimeState)
    {
        if (WaitTimeTravel) return;

        Debug.Log($"Portal: {objectToTravel.name} viaja a {targetTimeState}");

        objectToTravel.HandleAutomaticPortalEntry(targetTimeState);

        foreach (var observer in observers)
        {
            observer.PreTimeChange(targetTimeState);
        }

        objectToTravel.OnTimeChanged(targetTimeState);

        foreach (var observer in observers)
        {
            if (!ReferenceEquals(observer, objectToTravel))
            {
                observer.OnTimeChanged(CurrentTimeState);
            }
        }

        // Opcional: Puedes agregar un sonido específico para viajes de objetos individuales
        // PlayObjectTimeTravelSound();
    }

    // Método para manejar cambios de escena
    public void OnSceneLoaded()
    {
        // Re-aplicar el estado de música actual al cargar una nueva escena
        ChangeWwiseMusicState(CurrentTimeState);
    }

    // Método para forzar el estado de música (útil para debugging)
    public void DebugSetMusicState(TimeState state)
    {
        ChangeWwiseMusicState(state);
    }

    public void RegisterObserver(ITimeTravel observer)
    {
        if (!observers.Contains(observer))
            observers.Add(observer);
    }

    public void UnregisterObserver(ITimeTravel observer)
    {
        if (observers.Contains(observer))
            observers.Remove(observer);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}