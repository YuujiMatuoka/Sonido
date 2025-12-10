using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOnOffColliders : TimeTravelObject
{
    [SerializeField] private Collider col;

    public override void OnTimeChanged(TimeState newTimeState)
    {
        base.OnTimeChanged(newTimeState);
        if (newTimeState == TimeState.L1)
        {
            col.enabled = true;
        }
        else if (newTimeState == TimeState.Origin && !IsProtected())
        {
            col.enabled = false;
        }
    }

}
