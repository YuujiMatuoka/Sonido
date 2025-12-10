using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{
    public static UpdateManager Instance { get; private set; }

    private List<IUpdatable> _updatables = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;


    }

    public void Register(IUpdatable updatable)
    {
        if (!_updatables.Contains(updatable))
            _updatables.Add(updatable);
    }

    public void Unregister(IUpdatable updatable)
    {
        _updatables.Remove(updatable);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        foreach (var u in _updatables)
        {
            u.OnUpdate(dt);
        }
    }
}
