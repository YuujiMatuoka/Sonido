using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateBunnySteering<T> : State<T>
{
    BunnyModel _bunny;
    IRobotView _view;
    ISteering _steering;
    ObstacleAvoidanceV2 _avoidance;

    public StateBunnySteering(BunnyModel bunny, IRobotView view, ISteering steering, ObstacleAvoidanceV2 avoidance)
    {
        _bunny = bunny;
        _view = view;
        _steering = steering;
        _avoidance = avoidance;
    }

    public override void Init()
    {
        base.Init();
        _view.PlayFollowAnimation();
    }

    public override void Execute()
    {
        var dir = _steering.GetDir();
        var finalDir = _avoidance.GetDir(dir, false);
        _bunny.Move(finalDir.normalized);
        _bunny.LookDir(finalDir);
    }

    public override void Sleep()
    {
        base.Sleep();
        _view.StopFollowAnimation();
    }
}

