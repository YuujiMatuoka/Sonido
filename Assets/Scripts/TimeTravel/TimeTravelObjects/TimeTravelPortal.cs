using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTravelPortal : MonoBehaviour
{
    [SerializeField] private AudioSource myAudioSource;
    [SerializeField] private AudioClip portalActivated;
    [SerializeField] private AudioClip goToTravel;

    [Header("Player Rotation Settings")]
    [SerializeField] private bool rotatePlayer180Degrees = true;

    [Header("Exit Direction Settings")]
    [SerializeField] private bool useExitPoint = true; 
    [SerializeField] private Transform exitPoint; 

    private bool isActive = false;
    private bool wasPlayingBeforePause = false;

    public void Start()
    {
        if (myAudioSource != null)
            AudioManager.Instance?.RegisterSource(myAudioSource);

        GameManager.OnGameStateChanged += OnGameStateChanged;

        //if (useExitPoint && exitPoint == null)
        //{
        //    Debug.LogWarning("TimeTravelPortal: useExitPoint is enabled but no exitPoint is assigned!", this);
        //}
    }

    private void OnGameStateChanged(GameState newState)
    {
        if (myAudioSource == null) return;

        switch (newState)
        {
            case GameState.Paused:
                if (myAudioSource.isPlaying)
                {
                    wasPlayingBeforePause = true;
                    myAudioSource.Pause();
                }
                break;

            case GameState.Playing:
                if (wasPlayingBeforePause)
                {
                    myAudioSource.UnPause();
                    wasPlayingBeforePause = false;
                }
                break;
        }
    }

    public void ActivatePortal()
    {
        isActive = true;
        gameObject.SetActive(true);
        myAudioSource.clip = portalActivated;
        myAudioSource.loop = true;
        myAudioSource.Play();
        wasPlayingBeforePause = false; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            if (!TimeTravelManager.Instance.WaitTimeTravel)
            {
                PlayerManager playerManager = other.GetComponent<PlayerManager>();
                if (playerManager != null)
                {
                    if (useExitPoint && exitPoint != null)
                    {
                        playerManager.RotatePlayerTowardsPoint(exitPoint.position);
                    }
                    else if (rotatePlayer180Degrees)
                    {
                        playerManager.RotatePlayer180Degrees();
                    }
                }

                Desactivate();
                UIManager.Instance.OnTimeTravelEffect();
                myAudioSource.clip = goToTravel;
                myAudioSource.Play();
                TimeTravelManager.Instance.ToggleTime();
            }
        }
        else if (other.CompareTag("Object"))
        {
            TimeTwinLink twinLink = other.GetComponent<TimeTwinLink>();
            PickableTimeTravelObject pickable = other.GetComponent<PickableTimeTravelObject>();
            if (twinLink != null && TimeTravelManager.Instance != null && !pickable.IsHeld)
            {
                TimeState actual = TimeTravelManager.Instance.CurrentTimeState;
                TimeState destination;
                if (actual == TimeState.Origin)
                {
                    destination = TimeState.L1;
                }
                else
                {
                    destination = TimeState.Origin;
                }

                TimeTravelManager.Instance.RequestObjectTimeTravel(twinLink, destination);
            }
        }
    }

    public void Desactivate()
    {
        myAudioSource.loop = false;
        myAudioSource.Stop();
        isActive = false;
        gameObject.SetActive(false);
        wasPlayingBeforePause = false; 
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
}
