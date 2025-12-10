using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidanceV2 
{
    float _angle;
    float _radius;
    float _personalArea;
    Transform _entity;
    LayerMask _maskObs;

    public ObstacleAvoidanceV2(Transform entity, float angle, float radius, LayerMask maskObs, float personalArea = 1)
    {
        _angle = angle;
        _radius = radius;
        _entity = entity;
        _maskObs = maskObs;
        _personalArea = personalArea;
    }

    public Vector3 GetDir(Vector3 currentDir, bool calculateY)
    {
        Collider[] colls = Physics.OverlapSphere(_entity.position, _radius, _maskObs);
        Collider nearColl = null;
        Vector3 closetPoint = Vector3.zero;
        float nearCollDistance = 0;
        if (!calculateY) currentDir.y = 0;
        for (int i = 0; i < colls.Length; i++)
        {
            var currentColl = colls[i];
            closetPoint = currentColl.ClosestPoint(_entity.position);
            if (!calculateY) closetPoint.y = _entity.position.y;
            Vector3 dirToColl = closetPoint - _entity.position;
            float currentAngle = Vector3.Angle(dirToColl, currentDir);
            float distance = dirToColl.magnitude;

            if (currentAngle > _angle / 2) { continue; }

            if (nearColl == null)
            {
                nearColl = currentColl;
                nearCollDistance = distance;
                continue;
            }

            if (distance < nearCollDistance)
            {
                nearCollDistance = distance;
                nearColl = currentColl;
            }
        }
        if (nearColl == null)
        {
            return currentDir;
        }
        else
        {
            //Vector3 newDir = (currentDir + (_entity.position - closetPoint).normalized).normalized;
            Vector3 relativePos = _entity.InverseTransformPoint(closetPoint);
            Vector3 dirToClosetPoint = (closetPoint - _entity.position).normalized/60;
            Vector3 newDir;
            if (relativePos.x < 0)
            {
                newDir = Vector3.Cross(_entity.up, dirToClosetPoint);
            }
            else
            {
                newDir = -Vector3.Cross(_entity.up, dirToClosetPoint);
            }
            return Vector3.Lerp(currentDir, newDir, (_radius - Mathf.Clamp(nearCollDistance - _personalArea, 0, _radius)) / _radius);
        }
    }
}
