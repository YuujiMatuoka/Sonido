using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeCharger : TimeTravelObject
{
    [SerializeField] private float chargeTime = 3f;
    [SerializeField] private Transform batterySocket;

    [Header("Charger Specific Descriptions")]
    [TextArea]
    [SerializeField] private string chargingDescription = "The charger is currently charging a battery.";
    [TextArea]
    [SerializeField] private string readyDescription = "The charger is ready to receive a battery.";


    [Header("MaterialChange")]
    [SerializeField] private List<MaterialChange> materialChanges = new List<MaterialChange>();

    private TimeBattery insertedBattery;
    private float chargeTimer = 0f;
    private bool isCharging = false;

    [SerializeField] private float batteryCooldownTime = 1.5f;
    private bool isInCooldown = false;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {

        if (isCharging && insertedBattery != null)
        {
            chargeTimer += Time.deltaTime;
            if (chargeTimer >= chargeTime)
            {
                insertedBattery.Charge();
                StopCharging();
            }
        }
    }



    private void OnCollisionEnter(UnityEngine.Collision collision)
    {
        if (insertedBattery != null || isInCooldown) return;

        if (collision.gameObject.TryGetComponent<TimeBattery>(out var battery))
        {
            InsertBattery(battery);
        }
    }
    public void InsertBattery(TimeBattery battery)
    {
        if (insertedBattery != null)
        {
            return;
        }

        insertedBattery = battery;
        battery.AttachToSocket(batterySocket);
        
        if (!battery.IsBroken())
        {

            foreach (MaterialChange materialChange in materialChanges)
            {
                if (materialChange != null)
                {
                    materialChange.ChangeMaterialL1Ready();
                }
            }
            StartCharging();
        }
    }

    public void RemoveBattery()
    {
        if (insertedBattery == null) return;
        
        StopCharging();
        insertedBattery.DetachFromSocket();
        insertedBattery = null;

        foreach (MaterialChange materialChange in materialChanges)
        {
            if (materialChange != null)
            {
                materialChange.ChangeMaterialToL1Open();
            }
        }

        // Activar cooldown
        isInCooldown = true;
        Invoke(nameof(ResetCooldown), batteryCooldownTime);
    }

    private void ResetCooldown()
    {
        isInCooldown = false;
    }

    public override void OnTimeChanged(TimeState newTimeState)
    {
        base.OnTimeChanged(newTimeState);

        if (insertedBattery != null)
        {
            // Si la batería al cambiar de línea de tiempo NO está donde debería estar (en el socket)
            var savedPosition = insertedBattery.GetLastSavedPosition();
            if (Vector3.Distance(savedPosition, batterySocket.position) > 0.2f)
            {
                //Debug.Log("La batería no sigue en el socket tras viajar en el tiempo. Se elimina del panel.");
                RemoveBattery();
            }
        }

    }
        private void StartCharging()
    {
        isCharging = true;
        chargeTimer = 0f;
    }

    private void StopCharging()
    {
        isCharging = false;
    }

    protected override string GetAdditionalDescriptionInfo()
    {
        List<string> descriptions = new List<string>();

        if (!string.IsNullOrEmpty(base.GetAdditionalDescriptionInfo()))
            descriptions.Add(base.GetAdditionalDescriptionInfo());

        if (insertedBattery != null)
        {
            descriptions.Add(IsBroken() ? "It has a battery inserted but it's not charging because the charger is broken." : chargingDescription);
        }
        else
        {
            descriptions.Add(IsBroken() ? "It is useless." : readyDescription);
        }

        return string.Join("\n", descriptions);
    }


public bool HasBattery() => insertedBattery != null;
}