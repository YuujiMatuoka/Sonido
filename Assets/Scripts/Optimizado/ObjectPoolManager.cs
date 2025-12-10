using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ObjectPoolManager
{
    private Dictionary<InteractableType, object> _pools = new();

    public void Initialize(List<ObjectPoolConfig> configs)
    {
        foreach (var config in configs)
        {
            var poolType = config.prefab.GetComponent<Component>().GetType();
            var method = typeof(ObjectPoolManager).GetMethod(nameof(CreateGenericPool), BindingFlags.NonPublic | BindingFlags.Instance);
            var generic = method.MakeGenericMethod(poolType);
            generic.Invoke(this, new object[] { config });
        }
    }

    private void CreateGenericPool<T>(ObjectPoolConfig config) where T : Component
    {
        var typedPrefab = config.prefab.GetComponent<T>();
        var pool = new ObjectPool<T>(typedPrefab, config.size);
        _pools[config.type] = pool;
    }

    public T Get<T>(InteractableType type, Vector3 pos, Quaternion rot) where T : Component
    {
        if (_pools.TryGetValue(type, out var pool))
            return (pool as ObjectPool<T>)?.Get(pos, rot);

        return null;
    }

    public void Return<T>(InteractableType type, T obj) where T : Component
    {
        if (_pools.TryGetValue(type, out var pool))
            (pool as ObjectPool<T>)?.Return(obj);
    }

    public bool HasPool(InteractableType type)
    {
        return _pools.ContainsKey(type);
    }

    public void CreatePool(InteractableType type, GameObject prefab, int size)
    {
        if (!_pools.ContainsKey(type))
        {
            var poolType = prefab.GetComponent<Component>().GetType();
            var method = typeof(ObjectPoolManager).GetMethod(nameof(CreateGenericPool), BindingFlags.NonPublic | BindingFlags.Instance);
            var generic = method.MakeGenericMethod(poolType);
            generic.Invoke(this, new object[] { new ObjectPoolConfig { type = type, prefab = prefab, size = size } });
        }
    }
}