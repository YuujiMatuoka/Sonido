using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour 
{
    public static event System.Action<GameState> OnGameStateChanged;

    private TimeTravelManager timeTravelManager;

    public static GameManager Instance { get; private set; }

    public PlayerManager PlayerManager { get; private set; }

    public PlayerModel PlayerModel { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Playing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
    }
    // Configuración inicial
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
        timeTravelManager = FindObjectOfType<TimeTravelManager>();
        PlayerManager = FindObjectOfType<PlayerManager>(); 
    }

    // Cambiar estado global
    public void SetGameState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                UIManager.Instance.pauseMenuPanel.SetActive(false);
                UIManager.Instance.SetingMenuPanel.SetActive(false);
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                UIManager.Instance.pauseMenuPanel.SetActive(true); 
                break;

            case GameState.TimeTravel:
                timeTravelManager.ToggleTime();
                break;
        }
    }

    // Métodos rápidos
    public void TogglePause() => SetGameState(CurrentState == GameState.Paused ? GameState.Playing : GameState.Paused);
}
