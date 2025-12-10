using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PikeableSystem : IUpdatable, IPickable
{
    // Referencias
    private readonly Transform _target;
    private readonly Rigidbody _rb;

    // Configuración
    private float _holdSpeed = 15f;
    private float _maxGrabDistance = 4f;

    // Estado
    private Transform _currentHand;
    public bool IsHeld { get; private set; }
    public Vector3 Position => _target.position;

    public PikeableSystem(Transform target, Rigidbody rb)
    {
        _target = target;
        _rb = rb;
    }

    // === Interfaz IPickable ===
    public void Grab(Transform handTransform, Lightning lightningEffect)
    {
        if (IsHeld) return;

        IsHeld = true;
        _currentHand = handTransform;

        // Configurar física
        _rb.useGravity = false;
        _rb.velocity = Vector3.zero;

        Debug.Log("MeAgarre");
    }

    public void OnDrop()
    {
        if (!IsHeld) return;

        IsHeld = false;
        _rb.useGravity = true;
        _currentHand = null;
    }

    // === Actualización ===
    public void OnUpdate(float deltaTime)
    {
        if (!IsHeld || _currentHand == null) return;

        // Movimiento suave
        Vector3 toTarget = _currentHand.position - _target.position;
        _rb.velocity = toTarget * _holdSpeed;

        // Auto-soltar si está lejos
        if (toTarget.magnitude > _maxGrabDistance)
            OnDrop();
    }

    public bool IsHolding()
    {
        throw new System.NotImplementedException();
    }
}