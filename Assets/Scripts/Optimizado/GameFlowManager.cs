
using UnityEngine;
using System.Collections.Generic;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("Pool Database")]
    public PoolDatabaseSO poolDatabase;

    [Header("Spawn Points")]
    public List<Transform> pikeableSpawns;

    private ObjectPoolManager _poolManager;
    private List<PikeableSystem> _pikeableSystems = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        InitializeGame();
    }

    private void InitializeGame()
    {
        // Inicializar Pool
        _poolManager = new ObjectPoolManager();
        var configs = new List<ObjectPoolConfig>();

        foreach (var item in poolDatabase.poolItems)
        {
            configs.Add(new ObjectPoolConfig
            {
                type = item.type,
                prefab = item.prefab,
                size = item.size
            });
        }
        _poolManager.Initialize(configs);

        // Spawnear Pikeables
        foreach (var spawn in pikeableSpawns)
        {
            var obj = _poolManager.Get<Transform>(InteractableType.Pikeable, spawn.position, spawn.rotation);
            if (obj != null && obj.TryGetComponent<Rigidbody>(out var rb))
            {
                var system = new PikeableSystem(obj.transform, rb);
                _pikeableSystems.Add(system);
                UpdateManager.Instance.Register(system);
            }
        }
    }

    // Para el InteractionManager
    public PikeableSystem GetNearestPikeable(Vector3 position)
    {
        PikeableSystem nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (var system in _pikeableSystems)
        {
            if (system.IsHeld) continue;

            float dist = Vector3.Distance(system.Position, position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = system;
            }
        }
        return nearest;
    }
}