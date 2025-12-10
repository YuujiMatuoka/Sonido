using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionRelay : MonoBehaviour
{
    public System.Action<Collision> OnCollision;

    private void OnCollisionEnter(Collision collision)
    {
        OnCollision?.Invoke(collision);
    }
}