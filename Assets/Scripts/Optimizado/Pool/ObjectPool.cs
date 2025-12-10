using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private Queue<T> _objects = new();
    private T _prefab;
    private Transform _parent;

    public ObjectPool(T prefab, int size, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < size; i++)
        {
            T obj = GameObject.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _objects.Enqueue(obj);
        }
    }

    public T Get(Vector3 pos, Quaternion rot)
    {
        if (_objects.Count == 0)
            return null;

        T obj = _objects.Dequeue();
        obj.transform.SetPositionAndRotation(pos, rot);
        obj.gameObject.SetActive(true);
        (obj as IPoolable)?.OnSpawn();
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        (obj as IPoolable)?.OnDespawn();
        _objects.Enqueue(obj);
    }
}