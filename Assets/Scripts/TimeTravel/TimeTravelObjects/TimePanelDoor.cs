using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimePanelDoor : TimeTravelObject, IInteractable
{
    [SerializeField] private InteractiveTimeDoor linkedDoor;
    private TimeBattery insertedBattery;
    [SerializeField] private Transform batterySocket;

    [Header("Screen")]
    [SerializeField] MeshRenderer screenL1;
    [SerializeField] MeshRenderer screenL1Origen;
    [SerializeField] Material offBattery;
    [SerializeField] Material onBattery;
    [SerializeField] Material openDoor;
    [SerializeField] Material denied;

    [Header("Sonidos")]
    [SerializeField] private AudioClip press;
    [SerializeField] private AudioClip insertClip;
    [SerializeField] private AudioSource mySound;

    [Header("Panel Specific Descriptions")]
    [TextArea]
    [SerializeField] private string withBatteryDescription = "The console has a battery inserted.";
    [TextArea]
    [SerializeField] private string withoutBatteryDescription = "There is no battery inserted in the consol.";

    [Header("Cooldown")]
    [SerializeField] private float batteryCooldownTime = 1.5f;
    private bool isInCooldown = false;

    protected override void Awake()
    {
        base.Awake();
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
        if (insertedBattery != null) return;

        insertedBattery = battery;
        battery.AttachToSocket(batterySocket);
        if (insertedBattery.IsFunctional())
        {
            // Opcional: Feedback
        }
        else
        {
            screenL1.material = offBattery;
            screenL1Origen.material = offBattery;
        }

        if (mySound && insertClip)
            mySound.PlayOneShot(insertClip);
    }

    public override void OnTimeChanged(TimeState newTimeState)
    {
        base.OnTimeChanged(newTimeState);

        if (insertedBattery != null)
        {
            var savedPosition = insertedBattery.GetLastSavedPosition();
            if (Vector3.Distance(savedPosition, batterySocket.position) > 0.2f)
            {
                RemoveBattery();
            }
        }
    }

    public void RemoveBattery()
    {
        if (insertedBattery == null) return;

        insertedBattery.DetachFromSocket();
        insertedBattery = null;

        screenL1.material = offBattery;
        screenL1Origen.material = offBattery;

        isInCooldown = true;
        Invoke(nameof(ResetCooldown), batteryCooldownTime);
    }

    private void ResetCooldown()
    {
        isInCooldown = false;
    }

    public void Interact()
    {
        // 1. CONDICIÓN PRINCIPAL: Si está roto (en mal estado), NO funciona.
        // Si está en Origen pero "sano" (!isBroken), pasa esta línea.
        if (isBroken)
        {
            return;
        }

        if (insertedBattery == null) return;

        if (!insertedBattery.IsFunctional()) return;

        if (linkedDoor != null)
        {
            if (mySound && press)
                mySound.PlayOneShot(press);

            if (linkedDoor.IsOpen)
            {
                linkedDoor.ToggleDoor();

                // CORRECCIÓN: Eliminado "&& IsProtected()".
                // Si llegamos aquí es porque !isBroken, así que podemos operar en Origen.
                if (TimeTravelManager.Instance.CurrentTimeState == TimeState.Origin)
                    StartCoroutine(ChangeScreen(screenL1Origen, denied));

                if (TimeTravelManager.Instance.CurrentTimeState == TimeState.L1)
                    StartCoroutine(ChangeScreen(screenL1, denied));
            }
            else
            {
                linkedDoor.ToggleDoor();

                // CORRECCIÓN: Eliminado "&& IsProtected()".
                if (TimeTravelManager.Instance.CurrentTimeState == TimeState.Origin)
                    StartCoroutine(ChangeScreen(screenL1Origen, openDoor));

                if (TimeTravelManager.Instance.CurrentTimeState == TimeState.L1)
                    StartCoroutine(ChangeScreen(screenL1, openDoor));
            }
        }
    }

    protected override string GetAdditionalDescriptionInfo()
    {
        List<string> descriptions = new List<string>();

        if (!string.IsNullOrEmpty(base.GetAdditionalDescriptionInfo()))
            descriptions.Add(base.GetAdditionalDescriptionInfo());

        descriptions.Add(insertedBattery != null ? withBatteryDescription : withoutBatteryDescription);

        return string.Join("\n", descriptions);
    }

    IEnumerator ChangeScreen(MeshRenderer screen, Material mat)
    {
        var currentMat = screen.material;
        screen.material = mat;
        yield return new WaitForSeconds(3f);
        screen.material = currentMat;
    }

    public bool HasBattery() => insertedBattery != null;
}