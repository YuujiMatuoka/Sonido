using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public class SceneManagerCustom : MonoBehaviour
{
    public static SceneManagerCustom Instance { get; private set; }

    [Header("Configuración inicial")]
    public string firstRoomScene = "Scene1_A";
    public string playerScene = "PlayerAlone";

    [Header("Loading Screen")]
    public Canvas loadingCanvas;
    public string showAnimationName = "ShowLoading";
    public string hideAnimationName = "HideLoading";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(LoadInitialScenes());
    }

    IEnumerator LoadInitialScenes()
    {
        yield return SceneManager.LoadSceneAsync(playerScene, LoadSceneMode.Additive);

        var asyncOp = SceneManager.LoadSceneAsync(firstRoomScene, LoadSceneMode.Additive);
        yield return asyncOp;

        yield return null;

        Scene firstScene = SceneManager.GetSceneByName(firstRoomScene);
        if (firstScene.isLoaded)
        {
            if (DoorSpawnerManager.Instance != null)
            {
                DoorSpawnerManager.Instance.ProcessSceneForDoorAnchors(firstScene);
            }
            else
            {
                Debug.LogWarning("DoorSpawnerManager.Instance sigue siendo nulo después de cargar la escena y esperar un frame.");
            }
        }
        else
        {
            Debug.LogError($"¡FALLO AL CARGAR! La escena '{firstRoomScene}' no se encontró o no está cargada después de la operación.");
        }

        yield return StartCoroutine(HideLoadingScreen());
    }

    public void LoadScene(string sceneName, System.Action onSceneLoaded = null)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            Debug.LogWarning($"SceneManager: Se intentó cargar la escena '{sceneName}', pero ya estaba cargada.");
            onSceneLoaded?.Invoke();
            return;
        }

        StartCoroutine(LoadSceneCoroutine(sceneName, onSceneLoaded));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, System.Action onSceneLoaded)
    {
        var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return asyncOp;

        yield return null;

        Scene newScene = SceneManager.GetSceneByName(sceneName);

        if (!newScene.isLoaded)
        {
            Debug.LogError($"¡FALLO AL CARGAR! La escena '{sceneName}' no se cargó.");
            yield break;
        }

        if (DoorSpawnerManager.Instance != null)
        {
            DoorSpawnerManager.Instance.ProcessSceneForDoorAnchors(newScene);
        }
        else
        {
            Debug.LogWarning("DoorSpawnerManager.Instance es nulo al cargar una escena subsecuente.");
        }

        onSceneLoaded?.Invoke();

        yield return StartCoroutine(HideLoadingScreen());
    }

    private IEnumerator HideLoadingScreen()
    {
        loadingCanvas.gameObject.SetActive(false);

        yield return null;
    }

    public void UnloadScene(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }
        else
        {
            Debug.LogWarning($"SceneManager: Se intentó descargar la escena '{sceneName}', pero no estaba cargada.");
        }
    }
}