using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnyView : MonoBehaviour, IRobotView
{
    Animator _anim;
    [SerializeField] GameObject brokenVisual;
    [SerializeField] GameObject RepairedVisual;
    public bool IsBroken { get; private set; }
    void Awake()
    {
        //_anim = GetComponent<Animator>();
    }

    public void PlayFollowAnimation()
    {
        //_anim.SetBool("isFollowing", true);
    }

    public void StopFollowAnimation()
    {
        //_anim.SetBool("isFollowing", false);
    }

    public void PlayIdleAnimation()
    {
        //_anim.SetTrigger("Idle");
    }

    public void PlayJumpAnimation()
    {
        //_anim.SetTrigger("Jump");
    }

    public void PlayWaitAnimation()
    {
        //_anim.SetTrigger("Wait");
    }

    public void SetBroken(bool value)
    {
        IsBroken = value;
        brokenVisual.SetActive(IsBroken);
        RepairedVisual.SetActive(!IsBroken);
    }

    public void DesactiveVisuals()
    {
        if (IsBroken)
            brokenVisual.SetActive(false);
        if (!IsBroken)
            RepairedVisual.SetActive(false);
    }

    public void ActiveVisuals()
    {
        if (IsBroken)
            brokenVisual.SetActive(true);
        if (!IsBroken)
            RepairedVisual.SetActive(true);
    }
}

