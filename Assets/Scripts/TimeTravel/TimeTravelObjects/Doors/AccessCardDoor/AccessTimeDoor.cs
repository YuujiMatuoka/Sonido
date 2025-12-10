using UnityEngine;
using System.Collections;

public class AccessTimeDoor : TimeTravelObject
{
    [Header("Partes de la puerta")]
    [SerializeField] private Transform doorLeft;
    [SerializeField] private Transform doorRight;

    [Header("Deslizamiento")]
    [SerializeField] private float slideDistance = 1f;
    [SerializeField] private float slideSpeed = 1f;

    [Header("Consola requerida")]
    [SerializeField] private CardConsole linkedConsole;

    [SerializeField] private bool isOpen = false;

    private Vector3 doorLeftClosedPos;
    private Vector3 doorRightClosedPos;
    private Vector3 doorLeftOpenPos;
    private Vector3 doorRightOpenPos;

    [System.Serializable]
    public struct DoorState
    {
        public bool isOpen;
    }

    public DoorState stateL1;

    protected override void Start()
    {
        base.Start();

        doorLeftClosedPos = doorLeft.position;
        doorRightClosedPos = doorRight.position;

        doorLeftOpenPos = doorLeftClosedPos - doorLeft.right * slideDistance;
        doorRightOpenPos = doorRightClosedPos + doorRight.right * slideDistance;

        UpdateVisuals(TimeTravelManager.Instance.CurrentTimeState);
    }

    protected override void UpdateVisuals(TimeState timeState)
    {
        StopAllCoroutines();

        if (isOpen)
        {
            StartCoroutine(MoveDoors(doorLeftOpenPos, doorRightOpenPos));
        }
        else
        {
            StartCoroutine(MoveDoors(doorLeftClosedPos, doorRightClosedPos));
        }
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

    public void TryOpen()
    {
        var currentTime = TimeTravelManager.Instance.CurrentTimeState;

        bool isConsoleActive = linkedConsole != null; // linkedConsole.WasActivatedInL1();
        bool isDoorUsable = currentTime == TimeState.L1 || (currentTime == TimeState.Origin && IsProtected());

        if (isConsoleActive && isDoorUsable && !isOpen)
        {
            isOpen = true;
            UpdateVisuals(currentTime);
        }
    }

    public override void OnTimeChanged(TimeState newTime)
    {
        base.OnTimeChanged(newTime);

        if (newTime == TimeState.Origin && !IsProtected())
        {
            isOpen = stateL1.isOpen;
        }

        UpdateVisuals(newTime);
    }

    public void SaveState(TimeState time)
    {
        if (time == TimeState.L1)
        {
            stateL1.isOpen = isOpen;
        }
    }

    public void LoadState(TimeState time)
    {
        if (time == TimeState.L1)
        {
            isOpen = stateL1.isOpen;
        }
    }
}
