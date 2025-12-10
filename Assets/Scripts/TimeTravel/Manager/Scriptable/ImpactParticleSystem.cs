using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ImpactParticleSystem
{
    [System.Serializable]
    public class ParticleEffect
    {
        public ParticleSystem particleSystem;
        public float minVelocityToTrigger = 1.0f;
        public float maxVelocityForMaxEmission = 5.0f;
        public Vector2 emissionRange = new Vector2(5, 20);
        public Vector2 sizeRange = new Vector2(0.5f, 1.5f);
        public float intensityMultiplier = 1f;
    }

    [Header("Material Settings")]
    [SerializeField] private MaterialType materialType;

    [Header("Particle References")]
    [SerializeField] private ParticleEffect sparkEffect;
    [SerializeField] private ParticleEffect dustEffect;
    [SerializeField] private ParticleEffect debrisEffect;

    [Header("Surface Detection")]
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField] private LayerMask wallLayer = 1;
    [SerializeField] private float raycastDistance = 0.1f;

    private Rigidbody trackedRigidbody;
    private Transform trackedTransform;
    private Vector3 lastPosition;
    private float currentSpeed;
    private bool wasGrounded;

    public void Initialize(Rigidbody rb, Transform transform, MaterialType material = null)
    {
        trackedRigidbody = rb;
        trackedTransform = transform;
        lastPosition = transform.position;

        if (material != null)
        {
            SetMaterialType(material);
        }
    }

    public void SetMaterialType(MaterialType material)
    {
        materialType = material;

        if (materialType != null)
        {
            sparkEffect.intensityMultiplier = materialType.sparkIntensityMultiplier;
            dustEffect.intensityMultiplier = materialType.dustIntensityMultiplier;
            debrisEffect.intensityMultiplier = materialType.debrisIntensityMultiplier;
        }
    }

    public void Update()
    {
        if (trackedRigidbody == null || materialType == null) return;

        currentSpeed = trackedRigidbody.velocity.magnitude;
        CheckGroundMovement();
        lastPosition = trackedTransform.position;
    }

    private void CheckGroundMovement()
    {
        bool isGrounded = CheckIfGrounded();

        if (isGrounded && currentSpeed > dustEffect.minVelocityToTrigger)
        {
            float emissionRate = CalculateEmissionRate(dustEffect, currentSpeed);
            EmitDust(emissionRate, currentSpeed);
        }

        wasGrounded = isGrounded;
    }

    private bool CheckIfGrounded()
    {
        return Physics.Raycast(
            trackedTransform.position + Vector3.up * 0.1f,
            Vector3.down,
            raycastDistance + 0.1f,
            groundLayer
        );
    }

    private float CalculateEmissionRate(ParticleEffect effect, float velocity)
    {
        return Mathf.Lerp(
            effect.emissionRange.x,
            effect.emissionRange.y,
            Mathf.Clamp01(velocity / effect.maxVelocityForMaxEmission)
        ) * effect.intensityMultiplier;
    }

    private float CalculateParticleSize(ParticleEffect effect, float velocity)
    {
        return Mathf.Lerp(
            effect.sizeRange.x,
            effect.sizeRange.y,
            Mathf.Clamp01(velocity / effect.maxVelocityForMaxEmission)
        );
    }

    public void HandleCollision(Collision collision, float impactForce)
    {
        if (materialType == null) return;

        bool isWall = ((1 << collision.gameObject.layer) & wallLayer) != 0;
        bool isGround = ((1 << collision.gameObject.layer) & groundLayer) != 0;

        if (isWall && impactForce > sparkEffect.minVelocityToTrigger)
        {
            EmitSparks(collision.contacts[0].point, collision.relativeVelocity.normalized, impactForce);
        }

        if (isGround && impactForce > dustEffect.minVelocityToTrigger)
        {
            EmitDustOnImpact(collision.contacts[0].point, impactForce);
        }

        if (impactForce > debrisEffect.minVelocityToTrigger)
        {
            EmitDebris(collision.contacts[0].point, impactForce);
        }
    }

    private void EmitSparks(Vector3 position, Vector3 direction, float impactForce)
    {
        if (sparkEffect.particleSystem == null) return;

        float emissionCount = CalculateEmissionRate(sparkEffect, impactForce);
        float particleSize = CalculateParticleSize(sparkEffect, impactForce);

        var emitParams = new ParticleSystem.EmitParams
        {
            position = position,
            velocity = direction * impactForce * 0.5f * materialType.hardness,
            startSize = particleSize,
            applyShapeToPosition = true
        };

        sparkEffect.particleSystem.Emit(emitParams, Mathf.RoundToInt(emissionCount));
    }

    private void EmitDust(float emissionRate, float speed)
    {
        if (dustEffect.particleSystem == null) return;

        float particleSize = CalculateParticleSize(dustEffect, speed);

        var emitParams = new ParticleSystem.EmitParams
        {
            position = trackedTransform.position,
            startSize = particleSize,
            applyShapeToPosition = true
        };

        dustEffect.particleSystem.Emit(emitParams, Mathf.RoundToInt(emissionRate * Time.deltaTime));
    }

    private void EmitDustOnImpact(Vector3 position, float impactForce)
    {
        if (dustEffect.particleSystem == null) return;

        float emissionCount = CalculateEmissionRate(dustEffect, impactForce);
        float particleSize = CalculateParticleSize(dustEffect, impactForce);

        var emitParams = new ParticleSystem.EmitParams
        {
            position = position,
            startSize = particleSize,
            applyShapeToPosition = true
        };

        dustEffect.particleSystem.Emit(emitParams, Mathf.RoundToInt(emissionCount));
    }

    private void EmitDebris(Vector3 position, float impactForce)
    {
        if (debrisEffect.particleSystem == null) return;

        float emissionCount = CalculateEmissionRate(debrisEffect, impactForce);
        float particleSize = CalculateParticleSize(debrisEffect, impactForce);

        var emitParams = new ParticleSystem.EmitParams
        {
            position = position,
            startSize = particleSize,
            applyShapeToPosition = true,
            velocity = Random.insideUnitSphere * impactForce * 0.3f
        };

        debrisEffect.particleSystem.Emit(emitParams, Mathf.RoundToInt(emissionCount));
    }

    public void SetupParticleSystems(ParticleSystem sparks, ParticleSystem dust, ParticleSystem debris)
    {
        if (sparks != null)
        {
            sparkEffect.particleSystem = sparks;
            ConfigureParticleSystem(sparks, Color.yellow, 0.5f);
        }

        if (dust != null)
        {
            dustEffect.particleSystem = dust;
            ConfigureParticleSystem(dust, new Color(0.8f, 0.8f, 0.8f, 0.6f), 0.1f);
        }

        if (debris != null)
        {
            debrisEffect.particleSystem = debris;
            ConfigureParticleSystem(debris, new Color(0.6f, 0.4f, 0.2f, 1f), 0.8f);
        }
    }

    private void ConfigureParticleSystem(ParticleSystem ps, Color color, float gravity)
    {
        var main = ps.main;
        main.startColor = color;
        main.gravityModifier = gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
    }
}