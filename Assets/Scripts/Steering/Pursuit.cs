using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pursuit : ISteering
{
    Transform _entity;
    Rigidbody _target;
    float _timePrediction;

    public Pursuit(Transform entity, Rigidbody target, float timePrediction)
    {
        _entity = entity;
        _target = target;
        _timePrediction = timePrediction;
    }
    public Vector3 GetDir()
    {
        Vector3 point = _target.position + _target.transform.forward * _target.velocity.magnitude * _timePrediction;

        // Ignorar la altura para el cálculo de dirección
        point.y = _entity.position.y;

        Vector3 dirToPoint = (point - _entity.position).normalized;

        // Opcional: forzar dirToPoint.y a 0 para que no intente moverse verticalmente
        dirToPoint.y = 0f;

        return dirToPoint.normalized;
    }
}

