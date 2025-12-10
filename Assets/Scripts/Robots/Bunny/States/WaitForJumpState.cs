using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForJumpState<T> : State<T>
{
    private readonly BunnyModel model;
    private readonly IRobotView view;

    private float maxTime = 10f;
    private float currentTime = 0f;

    public WaitForJumpState(BunnyModel model, IRobotView view)
    {
        this.model = model;
        this.view = view;
    }

    public override void Init()
    {
        view.PlayWaitAnimation();
        model.Move(Vector3.zero); // Se queda quieto
        model.Rb.freezeRotation = true;
        //model.SetWaitingForJump(true);
        currentTime = 0f;
    }

    public override void Execute()
    {
        currentTime += Time.deltaTime;
        if(currentTime >= maxTime)
        {
            model.SetWaitingForJump(false);
            model.SetPlayerWantsJump(false);
        }
    }

    public override void Sleep()
    {
        view.PlayIdleAnimation();
    }
}

