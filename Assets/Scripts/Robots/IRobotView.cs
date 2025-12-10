using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRobotView 
{
    void PlayFollowAnimation();
    void PlayIdleAnimation();
    void PlayJumpAnimation();
    void PlayWaitAnimation();
    void StopFollowAnimation();
    void SetBroken(bool value);
}
