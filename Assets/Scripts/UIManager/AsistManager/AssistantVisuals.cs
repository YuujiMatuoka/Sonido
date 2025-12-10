using UnityEngine;
using System.Collections;

public class AssistantVisuals : MonoBehaviour
{
    private Dissolve[] dissolveScripts;

    public float dissolveDuration { get; private set; } = 1.5f;

    [SerializeField] private GameObject particlesGameObject;
    private void Awake()
    {
        dissolveScripts = GetComponentsInChildren<Dissolve>(true);

        if (dissolveScripts.Length > 0)
        {
            dissolveDuration = dissolveScripts[0].duration;
        }

        GameManager.OnGameStateChanged += OnGameStateChanged;
    }
    private void OnGameStateChanged(GameState newState)
    {
        bool isPaused = (newState == GameState.Paused);

        if (particlesGameObject != null)
        {
            particlesGameObject.SetActive(!isPaused);
        }
    }
    public void ReintegrateAll()
    {
        if (dissolveScripts == null) return;

        foreach (Dissolve dissolver in dissolveScripts)
        {
            dissolver.Reintegrate();
        }
    }

    public Coroutine DisintegrateAll()
    {
        if (dissolveScripts == null) return null;

        Coroutine lastCoroutine = null;
        foreach (Dissolve dissolver in dissolveScripts)
        {
            lastCoroutine = dissolver.Disintegrate();
        }

        return lastCoroutine;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
}
