using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockScale : MonoBehaviour
{
    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void LateUpdate()
    {
        transform.localScale = initialScale;
    }
}
