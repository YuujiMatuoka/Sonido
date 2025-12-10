using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class ParticleFrameStepper : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Cuántos cuadros por segundo tendrá la simulación visual")]
    public float targetFPS = 12f;

    [Tooltip("Aplica el efecto a todos los ParticleSystems hijos del objeto")]
    public bool includeChildren = true;

    [Tooltip("Usar tiempo no escalado (ignora pausas de juego)")]
    public bool unscaledTime = true;

    private List<ParticleSystem> particleSystems = new List<ParticleSystem>();
    private float frameInterval;
    private float accumulator = 0f;

    void Awake()
    {
        RefreshList();
        frameInterval = 1f / Mathf.Max(targetFPS, 0.1f);
    }

    void OnValidate()
    {
        frameInterval = 1f / Mathf.Max(targetFPS, 0.1f);
    }

    void RefreshList()
    {
        particleSystems.Clear();
        if (includeChildren)
            particleSystems.AddRange(GetComponentsInChildren<ParticleSystem>(true));
        else
        {
            var ps = GetComponent<ParticleSystem>();
            if (ps) particleSystems.Add(ps);
        }
    }

    void Update()
    {
        float delta = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        accumulator += delta;

        if (accumulator >= frameInterval)
        {
            
            foreach (var ps in particleSystems)
            {
                if (ps == null) continue;
                ps.Simulate(accumulator, withChildren: false, restart: false);
            }
            accumulator = 0f;
        }
        else
        {
            
            foreach (var ps in particleSystems)
            {
                if (ps == null) continue;
                ps.Simulate(0f, withChildren: false, restart: false);
            }
        }
    }

    void OnEnable()
    {
        accumulator = 0f;
    }

    
    [ContextMenu("Refrescar lista de sistemas")]
    void ManualRefresh() => RefreshList();
}