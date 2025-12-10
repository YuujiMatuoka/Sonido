using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeBattery : PickableTimeTravelObject
{
    [SerializeField] private bool isCharged = false;

    [Header("Battery Visual Settings")]
    [SerializeField] private GameObject visualCharged;

    [Header("Battery Specific Descriptions")]
    [TextArea]
    [SerializeField] private string chargedDescription = "La batería está completamente cargada y lista para usar.";
    [TextArea]
    [SerializeField] private string dischargedDescription = "La batería está descargada. Necesita recargarse.";

    private Transform currentSocket;
    private Vector3 lastFramePosition;

    private TimeTwinLink myTwin;

    protected override void Start()
    {
        base.Start();
        if (visualCharged != null) visualCharged.SetActive(false);
        lastFramePosition = transform.position;
        myTwin = GetComponent<TimeTwinLink>();
    }

    private void UpdateBatteryVisuals()
    {

        if (isCharged && visualCharged != null)
        {
            visualCharged.SetActive(true);
            visualL1.SetActive(false);
        }
        else if (!isCharged)
        {
            visualCharged.SetActive(false);
            visualL1.SetActive(true);
        }
        
    }


    public override void Grab(Transform handTransform, Lightning lightningEffect)
    {
        if (currentSocket != null)
        {
            var socketParent = currentSocket.parent;
            if (socketParent.TryGetComponent<TimePanelDoor>(out var panel))
            {
                panel.RemoveBattery();
            }
            else if (socketParent.TryGetComponent<TimeCharger>(out var charger))
            {
                charger.RemoveBattery();
            }
        }

        base.Grab(handTransform, lightningEffect);
    }

    public override void OnDrop()
    {
        base.OnDrop();
    }

    public void AttachToSocket(Transform socket)
    {
        if (isHeld)
        {
            ForceDropFromHand();
        }
        currentSocket = socket;
        transform.SetParent(socket);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        GetComponent<Rigidbody>().isKinematic = true;
        usePositionSaving = false;
    }

    public void DetachFromSocket()
    {
        currentSocket = null;
        transform.SetParent(null);
        GetComponent<Rigidbody>().isKinematic = false;
        usePositionSaving = true;
    }

    public Vector3 GetLastSavedPosition()
    {
        return lastL1Position;
    }

    public override void OnTimeChanged(TimeState newTimeState)
    {
        base.OnTimeChanged(newTimeState);

        if(myTwin!= null)
        {
            if (newTimeState == TimeState.Origin && myTwin.myTimeline == TimeState.Origin)
            {

                if (!isProtected)
                {
                    Discharge();
                }
            }
        }


        UpdateBatteryVisuals();
    }

    public void Charge()
    {
        if (!isBroken)
        {
            isCharged = true;
            UpdateBatteryVisuals();
        }
    }

    public void Discharge()
    {
        isCharged = false;
        UpdateBatteryVisuals();
    }

    protected override string GetAdditionalDescriptionInfo()
    {
        List<string> descriptions = new List<string>();

        if (!string.IsNullOrEmpty(base.GetAdditionalDescriptionInfo()))
            descriptions.Add(base.GetAdditionalDescriptionInfo());

        if (isCharged)
            descriptions.Add(chargedDescription);
        else
            descriptions.Add(dischargedDescription);

        return string.Join("\n", descriptions);
    }

    public bool IsFunctional() => isCharged && !isBroken;
    public bool IsCharged() => isCharged;

    //private void LateUpdate()
    //{
    //    if (TimeTravelManager.Instance.CurrentTimeState == TimeState.L1 && usePositionSaving)
    //    {
    //        if (Vector3.Distance(transform.position, lastFramePosition) > 0.001f)
    //        {
    //            lastL1Position = transform.position;
    //        }
    //    }

    //    lastFramePosition = transform.position;
    //}
}
