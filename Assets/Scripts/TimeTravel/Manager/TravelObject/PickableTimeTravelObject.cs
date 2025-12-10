using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PickableTimeTravelObject : TimeTravelObject, IPickable
{
    [Header("Pickable Settings")]
    [SerializeField] protected float grabDistance = 2.5f;
    protected float holdSpeed = 15f;
    protected float minSpeed = 2f;
    protected float angularSpeed = 15f;
    [SerializeField] protected float grabTolerance = 0.02f;
    [SerializeField] protected float maxGrabDistance = 4f;
    [SerializeField] private float positionThreshold = 0.1f;

    [SerializeField] protected GameObject BreakObject;
    [SerializeField] protected GameObject L1Object;

    [Header("Impact Sound Settings")]
    [SerializeField] private AudioSource impactAudioSource;
    [SerializeField] private List<AudioClip> impactClips;
    [SerializeField] private float minImpactVelocity = 1.5f;
    [SerializeField] private float minTimeBetweenSounds = 0.2f;

    [Header("Material Settings")]
    [SerializeField] private MaterialType objectMaterial;

    [Header("Impact Particles")]
    [SerializeField] private ImpactParticleSystem impactParticles;
    [SerializeField] private bool autoCreateParticles = true;

    private float lastImpactTime;

    protected Transform currentHand;
    protected bool isHeld;
    protected Lightning currentLightningEffect;
    protected InteractionManager currentInteractionManager;

    protected private int originalLayer;
    protected private int heldObjectLayer;
    protected private int playerLayer;
    private bool firstCollisionIgnored = false;
    private bool hasTraveledOnce = false;
    private TimeTwinLink twinLink;

    protected override void Awake()
    {
        base.Awake();
        heldObjectLayer = LayerMask.NameToLayer("HeldObject");
        playerLayer = LayerMask.NameToLayer("Player");
        originalLayer = gameObject.layer;
        twinLink = GetComponent<TimeTwinLink>();

        InitializeParticleSystem();
    }

    protected override void Start()
    {
        base.Start();

        if (impactAudioSource == null)
            impactAudioSource = baseAudioSource;

        if (impactAudioSource != null)
            AudioManager.Instance?.RegisterSource(impactAudioSource);
    }

    private void InitializeParticleSystem()
    {
        if (impactParticles == null)
            impactParticles = new ImpactParticleSystem();

        if (autoCreateParticles)
        {
            CreateParticleSystems();
        }

        impactParticles.Initialize(rb, transform, objectMaterial);
    }

    protected new void Update()
    {
        base.Update();

        if (!isHeld && impactParticles != null)
        {
            impactParticles.Update();
        }
    }

    private void CreateParticleSystems()
    {
        ParticleSystem sparksPrefab = objectMaterial?.sparkParticlePrefab;
        ParticleSystem dustPrefab = objectMaterial?.dustParticlePrefab;
        ParticleSystem debrisPrefab = objectMaterial?.debrisParticlePrefab;

        ParticleSystem sparks = CreateParticleSystem("SparksParticles", sparksPrefab, Color.yellow);
        ParticleSystem dust = CreateParticleSystem("DustParticles", dustPrefab, new Color(0.8f, 0.8f, 0.8f, 0.6f));
        ParticleSystem debris = CreateParticleSystem("DebrisParticles", debrisPrefab, new Color(0.6f, 0.4f, 0.2f, 1f));

        impactParticles.SetupParticleSystems(sparks, dust, debris);
    }

    private ParticleSystem CreateParticleSystem(string name, ParticleSystem prefab, Color defaultColor)
    {
        if (prefab != null)
        {
            return Instantiate(prefab, transform);
        }
        else
        {
            GameObject particleObj = new GameObject(name);
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.zero;

            ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
            var renderer = particleObj.AddComponent<ParticleSystemRenderer>();
            renderer.material = CreateParticleMaterial(defaultColor);

            return ps;
        }
    }

    private Material CreateParticleMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.color = color;
        return mat;
    }

    protected virtual void FixedUpdate()
    {
        if (isHeld && currentHand != null && rb != null)
        {
            Vector3 targetPos = currentHand.position;
            Vector3 toTarget = targetPos - transform.position;
            float distance = toTarget.magnitude;

            if (distance > maxGrabDistance)
            {
                ForceDropFromHand();
                return;
            }

            int layerMask = ~(1 << playerLayer);

            if (Physics.Linecast(transform.position, currentHand.position, out RaycastHit hit, layerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider != null && hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
                {
                    rb.velocity = Vector3.zero;
                    return;
                }
            }

            if (distance > grabTolerance)
            {
                rb.useGravity = true;
                float scaledSpeed = Mathf.Clamp(distance * holdSpeed, minSpeed, holdSpeed);
                rb.velocity = toTarget.normalized * scaledSpeed;
            }
            else
            {
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.MovePosition(targetPos);
            }

            Quaternion targetRot = currentHand.rotation;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * angularSpeed));
        }
    }

    public virtual void Grab(Transform handTransform, Lightning lightningEffect)
    {
        if (isHeld) return;

        PlayerManager playerManager = handTransform.GetComponentInParent<PlayerManager>();

        if (playerManager == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerManager = playerObj.GetComponent<PlayerManager>();
            }
        }

        if (playerManager != null)
        {
            currentInteractionManager = playerManager.interactionManager;
        }

        originalLayer = gameObject.layer;
        gameObject.layer = heldObjectLayer;
        Physics.IgnoreLayerCollision(heldObjectLayer, playerLayer, true);

        isHeld = true;
        currentHand = handTransform;

        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        transform.SetParent(null);
        if (useWorldPopup)
            UnPopUp();

        GetComponent<TimeTwinLink>()?.NotifyGrabbed();

        currentLightningEffect = lightningEffect;

        if (currentLightningEffect != null)
        {
            currentLightningEffect.final = this.transform;
            currentLightningEffect.gameObject.SetActive(true);
            currentLightningEffect.PlaySound();
        }
    }

    public void ForceDropFromHand()
    {
        if (isHeld)
        {
            if (currentInteractionManager != null)
            {
                currentInteractionManager.ForceReleaseObject();
            }
            else
            {
                OnDrop();
            }
        }
    }

    public virtual void OnDrop()
    {
        if (!isHeld) return;

        currentInteractionManager = null;

        gameObject.layer = originalLayer;
        Physics.IgnoreLayerCollision(heldObjectLayer, playerLayer, false);

        isHeld = false;

        if (currentLightningEffect != null)
        {
            currentLightningEffect.gameObject.SetActive(false);
            currentLightningEffect.StopSound();
            currentLightningEffect.final = null;
            currentLightningEffect = null;
        }

        currentHand = null;
        StartCoroutine(WaitToNotifyDrop());

        if (rb != null)
        {
            rb.useGravity = true;
        }
    }

    public override void OnTimeChanged(TimeState newTimeState)
    {
        if (isHeld)
        {
            return;
        }

        base.OnTimeChanged(newTimeState);
    }

    protected override void OnResidueCreated(GameObject residueContainer, bool wasBroken)
    {
        if (Vector3.Distance(lastL1Position, originPositionBeforeTravel) <= positionThreshold)
        {
            return;
        }

        GameObject visualPrefab = wasBroken ? BreakObject : L1Object;

        if (visualPrefab != null)
        {
            GameObject visualInstance = Instantiate(
                visualPrefab,
                residueContainer.transform.position,
                residueContainer.transform.rotation,
                residueContainer.transform
            );

            visualInstance.transform.localPosition = Vector3.zero;
            visualInstance.transform.localRotation = Quaternion.identity;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!firstCollisionIgnored)
        {
            firstCollisionIgnored = true;
            return;
        }

        if (!hasTraveledOnce)
        {
            hasTraveledOnce = true;
            return;
        }

        if (rb != null)
        {
            float impactForce = collision.relativeVelocity.magnitude;

            if (impactForce >= minImpactVelocity && Time.time - lastImpactTime > minTimeBetweenSounds)
            {
                PlayImpactSound(impactForce);
                lastImpactTime = Time.time;
            }

            if (impactParticles != null)
            {
                impactParticles.HandleCollision(collision, impactForce);
            }
        }
    }

    private void PlayImpactSound(float impactForce)
    {
        if (impactAudioSource != null && objectMaterial != null && objectMaterial.impactSounds.Length > 0)
        {
            int index = Random.Range(0, objectMaterial.impactSounds.Length);
            impactAudioSource.pitch = 1f + Random.Range(-objectMaterial.pitchVariation, objectMaterial.pitchVariation);
            impactAudioSource.PlayOneShot(objectMaterial.impactSounds[index], Mathf.Clamp01(impactForce / 10f));
        }
        else if (impactAudioSource != null && impactClips.Count > 0)
        {
            int index = Random.Range(0, impactClips.Count);
            impactAudioSource.pitch = Random.Range(0.8f, 1.2f);
            impactAudioSource.PlayOneShot(impactClips[index]);
            impactAudioSource.pitch = 1f;
        }
    }

    private IEnumerator WaitToNotifyDrop()
    {
        yield return new WaitForSeconds(0.3f);
        GetComponent<TimeTwinLink>()?.NotifyDropped();
    }

    public void SetMaterialType(MaterialType newMaterial)
    {
        objectMaterial = newMaterial;
        if (impactParticles != null)
        {
            impactParticles.SetMaterialType(newMaterial);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (impactAudioSource != null && impactAudioSource != baseAudioSource)
            AudioManager.Instance?.UnregisterSource(impactAudioSource);
    }

    public bool IsHeld => isHeld;

    public MaterialType GetCurrentMaterial()
    {
        return objectMaterial;
    }

    public bool IsHolding()
    {
        return IsHeld;
    }
}