using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FollowState<T> : State<T>
{
    private readonly BunnyModel model;
    private readonly IRobotView view;
    private readonly ISteering steering;
    private readonly ObstacleAvoidanceV2 obstacleAvoidance;

    private float stopDistance = 2.5f;   // distancia mínima para frenar
    private float followDistance = 5f;   // distancia máxima para volver a seguir

    private Rigidbody _target;

    public FollowState(BunnyModel model, IRobotView view, ISteering steering, ObstacleAvoidanceV2 obstacleAvoidance, Rigidbody target)
    {
        this.model = model;
        this.view = view;
        this.steering = steering;
        this.obstacleAvoidance = obstacleAvoidance;
        _target = target;
    }

    public override void Init()
    {
        view.PlayFollowAnimation();
        model.Rb.freezeRotation = false;
    }

    public override void Execute()
    {
        Vector3 targetPos = _target.position;
        float distance = Vector3.Distance(model.transform.position, targetPos);

        if (distance <= stopDistance)
        {
            // Frenar: no mover
            model.Move(Vector3.zero);
            return;
        }
        else if (distance > followDistance)
        {
            // Seguir normalmente
            var desiredDir = steering.GetDir();
            var finalDir = obstacleAvoidance.GetDir(desiredDir, false);
            model.Move(finalDir.normalized);
            model.LookDir(finalDir);
        }
        else
        {
            // Distancia intermedia, por ejemplo puedes desacelerar o seguir
            var desiredDir = steering.GetDir();
            var finalDir = obstacleAvoidance.GetDir(desiredDir, false);
            model.Move(finalDir.normalized * 0.5f); // moverse más lento (ejemplo)
            model.LookDir(finalDir);
        }
    }

    public override void Sleep()
    {
        view.StopFollowAnimation(); 
    }
}

