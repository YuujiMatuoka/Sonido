using UnityEngine;

public class BreakableWall : TimeTravelObject, IBreakable
{
    [Header("Breakable Wall Settings")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private LayerMask oxygenTankLayer;

    private bool wasNearTankInL1 = false;
    private bool hasCheckedInThisCycle = false;

    protected override void Start()
    {
        base.Start();
        visualL1.SetActive(false);
        visualOriginBroken.SetActive(false);
        visualL1Origen.SetActive(true);
    }
    public void BreakFromExternalSource()
    {
        if (!isProtected)
        {
            isBroken = true;
            UpdateVisuals(TimeTravelManager.Instance.CurrentTimeState);
        }
    }

    public override void OnTimeChanged(TimeState newTimeState)
    {
        base.OnTimeChanged(newTimeState);

        if (newTimeState == TimeState.L1)
        {
            // Resetear la verificación al entrar a L1
            hasCheckedInThisCycle = false;
            wasNearTankInL1 = false;

            // Verificar tanques cercanos solo una vez al inicio
            if (!hasCheckedInThisCycle)
            {
                wasNearTankInL1 = CheckForNearbyTanks();
                hasCheckedInThisCycle = true;
            }
        }
        else if (newTimeState == TimeState.Origin)
        {
            // En Origin, romper si había un tanque cerca en L1
            if (wasNearTankInL1 && !isProtected)
            {
                BreakFromExternalSource();
            }
        }

        UpdateVisuals(newTimeState);
    }

    private bool CheckForNearbyTanks()
    {
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position,
            detectionRadius,
            oxygenTankLayer,
            QueryTriggerInteraction.Ignore); // Ignorar triggers

        foreach (var collider in hitColliders)
        {
            // Verificar que el tanque no esté protegido
            var tank = collider.GetComponent<OxygenTank>();
            if (tank != null && !tank.IsProtected())
            {
                return true;
            }
        }
        return false;
    }

    protected override void UpdateVisuals(TimeState currentState)
    {
        // Desactivar todos primero
        if (visualL1Origen != null) visualL1Origen.SetActive(false);
        if (visualOriginBroken != null) visualOriginBroken.SetActive(false);
        if (visualL1 != null) visualL1.SetActive(false);

        if (currentState == TimeState.L1)
        {
            if (visualL1 != null) visualL1.SetActive(true);
        }
        else if (currentState == TimeState.Origin)
        {
            if (isBroken)
            {
                if (visualOriginBroken != null) visualOriginBroken.SetActive(true);
            }
            else
            {
                if (visualL1Origen != null) visualL1Origen.SetActive(true);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = wasNearTankInL1 ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}





