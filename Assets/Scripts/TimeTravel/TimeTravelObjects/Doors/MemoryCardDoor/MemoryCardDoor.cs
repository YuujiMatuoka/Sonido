using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryCardDoor : TimeTravelObject
{
    [Header("Door Parts")]
    [SerializeField] private Transform doorLeft;
    [SerializeField] private Transform doorRight;

    [Header("Settings")]
    [SerializeField] private float slideDistance = 1f;
    [SerializeField] private float slideSpeed = 1f;

    private Vector3 closedLeft, closedRight;
    private Vector3 openLeft, openRight;
    private bool isOpen = false;

    protected override void Start()
    {
        base.Start();
        closedLeft = doorLeft.position;
        closedRight = doorRight.position;
        openLeft = closedLeft - doorLeft.right * slideDistance;
        openRight = closedRight + doorRight.right * slideDistance;

        UpdateVisuals(TimeTravelManager.Instance.CurrentTimeState);
    }

    protected override void UpdateVisuals(TimeState state)
    {
        StopAllCoroutines();
        if (isOpen)
            StartCoroutine(MoveDoors(openLeft, openRight));
        else
            StartCoroutine(MoveDoors(closedLeft, closedRight));
    }

    private IEnumerator MoveDoors(Vector3 leftTarget, Vector3 rightTarget)
    {
        while (Vector3.Distance(doorLeft.position, leftTarget) > 0.01f ||
               Vector3.Distance(doorRight.position, rightTarget) > 0.01f)
        {
            doorLeft.position = Vector3.MoveTowards(doorLeft.position, leftTarget, slideSpeed * Time.deltaTime);
            doorRight.position = Vector3.MoveTowards(doorRight.position, rightTarget, slideSpeed * Time.deltaTime);
            yield return null;
        }

        doorLeft.position = leftTarget;
        doorRight.position = rightTarget;
    }

    public void Open()
    {
        if (!IsBroken() && !isOpen)
        {
            isOpen = true;
            UpdateVisuals(TimeTravelManager.Instance.CurrentTimeState);
        }
    }

    public bool CanBeOpened() => !IsBroken();
}

