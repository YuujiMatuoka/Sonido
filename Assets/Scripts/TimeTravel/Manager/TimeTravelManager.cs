using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTravelManager : MonoBehaviour
{
    public static TimeTravelManager Instance;
    public bool WaitTimeTravel;
    public float DelayTimeTravel = 3f;
    public TimeState CurrentTimeState { get; private set; } = TimeState.Origin;
    public TimeState PreviousTimeState { get; private set; } = TimeState.Origin; 

    private List<ITimeTravel> observers = new List<ITimeTravel>();
    private GameManager gameManager;

    private void Awake()
    {
        Instance = this;
        gameManager = FindObjectOfType<GameManager>();
        WaitTimeTravel = false; // Aseguramos que inicialmente esté desbloqueado
    }

    public void ToggleTime()
    {
        // Si WaitTimeTravel es true, no permitir viajar en el tiempo
        if (WaitTimeTravel) return;

        TimeState newState = (CurrentTimeState == TimeState.Origin) ? TimeState.L1 : TimeState.Origin;
        ChangeTime(newState);
    }

    public void ChangeTime(TimeState newTime)
    {
        PreviousTimeState = CurrentTimeState;
        CurrentTimeState = newTime;

        // PRIMERO: Este estado es para todos los objetos del cual dependan otros objetos como por ejemplo el escudo
        foreach (var observer in observers)
        {
            observer.PreTimeChange(CurrentTimeState);
        }

        // LUEGO: Cambio normal de estado
        foreach (var observer in observers)
        {
            observer.OnTimeChanged(CurrentTimeState);
        }


        WaitTimeTravel = true;
        StartCoroutine(DelayedStateChange());
    }

    public void RequestObjectTimeTravel(TimeTwinLink objectToTravel, TimeState targetTimeState)
    {
        if (WaitTimeTravel) return; // Respetar el cooldown global

        Debug.Log($"Portal: {objectToTravel.name} viaja a {targetTimeState}");

        objectToTravel.HandleAutomaticPortalEntry(targetTimeState);

        // Notificar a TODOS los objetos del viaje individual
        foreach (var observer in observers)
        {
            observer.PreTimeChange(targetTimeState);
        }

        // Aplicar el cambio al objeto específico
        objectToTravel.OnTimeChanged(targetTimeState);

        // Notificar post-cambio
        foreach (var observer in observers)
        {
            if (!ReferenceEquals(observer, objectToTravel)) // Evitar notificar dos veces al objeto que viajó
            {
                observer.OnTimeChanged(CurrentTimeState); // Mantener timeline global
            }
        }
    }


    private IEnumerator DelayedStateChange()
    {
        // Esperar 5 segundos
        yield return new WaitForSeconds(DelayTimeTravel);

        // Cambiar el estado después del retraso
        gameManager.SetGameState(GameState.Playing);

        // Desbloquear viajes en el tiempo
        WaitTimeTravel = false;
    
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
}