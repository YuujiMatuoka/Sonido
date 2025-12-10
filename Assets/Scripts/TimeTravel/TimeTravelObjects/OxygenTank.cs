using UnityEngine;

public class OxygenTank : PickableTimeTravelObject
{
    [Header("Oxygen Tank Settings")]
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private LayerMask breakableLayer;
    protected override void Start()
    {
        base.Start();
        visualL1.SetActive(false);
        visualOriginBroken.SetActive(false);
        visualL1Origen.SetActive(true);
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

    public override void OnTimeChanged(TimeState newTimeState)
    {
        base.OnTimeChanged(newTimeState);

        if (newTimeState == TimeState.Origin && !isProtected && !isBroken)
        {
            // Reproducir efecto de explosión
            if (explosionEffect != null)
            {
                explosionEffect.Play();
            }

            // Romperse a sí mismo
            isBroken = true;
            UpdateVisuals(newTimeState);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}






