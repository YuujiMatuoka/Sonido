using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsAnimator : MonoBehaviour
{
    [Header("Spider-Verse Animation Settings")]
    [Range(1, 60)]
    public int targetAnimationFPS = 12;
    [Range(0.1f, 3f)]
    public float animationSpeed = 1f;
    public bool affectChildren = true;

    [Header("Advanced Settings")]
    public bool enableSpeedVariation = false;
    [Range(0.1f, 0.5f)]
    public float speedVariation = 0.2f;

    private Animator[] animators;
    private float updateInterval;
    private float lastUpdateTime;
    private float[] originalSpeeds;

    void Start()
    {
        InitializeAnimators();
        updateInterval = 1f / targetAnimationFPS;
        lastUpdateTime = Time.time;

        // Guardar velocidades originales
        StoreOriginalSpeeds();

        // Configurar todos los animadores
        foreach (var animator in animators)
        {
            if (animator != null)
            {
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.updateMode = AnimatorUpdateMode.Normal;
                ApplyAnimationSpeed(animator);
            }
        }
    }

    void InitializeAnimators()
    {
        if (affectChildren)
        {
            animators = GetComponentsInChildren<Animator>();
        }
        else
        {
            animators = new Animator[] { GetComponent<Animator>() };
        }

        originalSpeeds = new float[animators.Length];
    }

    void StoreOriginalSpeeds()
    {
        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null)
            {
                originalSpeeds[i] = animators[i].speed;
            }
        }
    }

    void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateAnimations();
            lastUpdateTime = Time.time;
        }
    }

    void UpdateAnimations()
    {
        for (int i = 0; i < animators.Length; i++)
        {
            var animator = animators[i];
            if (animator != null && animator.enabled)
            {
                // Aplicar velocidad con variación si está habilitada
                float finalSpeed = animationSpeed;
                if (enableSpeedVariation)
                {
                    finalSpeed = animationSpeed * Random.Range(1f - speedVariation, 1f + speedVariation);
                }

                animator.speed = finalSpeed;
                animator.Update(updateInterval * finalSpeed);
            }
        }
    }

    void ApplyAnimationSpeed(Animator animator)
    {
        if (animator != null)
        {
            float finalSpeed = animationSpeed;
            if (enableSpeedVariation)
            {
                finalSpeed = animationSpeed * Random.Range(1f - speedVariation, 1f + speedVariation);
            }
            animator.speed = finalSpeed;
        }
    }

    // ===== MÉTODOS PÚBLICOS =====

    [ContextMenu("Set to Normal Speed")]
    public void SetNormalSpeed()
    {
        SetAnimationSpeed(1f);
    }

    [ContextMenu("Set to Slow Motion")]
    public void SetSlowMotion()
    {
        SetAnimationSpeed(0.5f);
    }

    [ContextMenu("Set to Fast Motion")]
    public void SetFastMotion()
    {
        SetAnimationSpeed(1.5f);
    }

    /// <summary>
    /// Cambia la velocidad de la animación
    /// </summary>
    /// <param name="speed">1 = velocidad normal, 0.5 = mitad de velocidad, 2 = doble velocidad</param>
    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = Mathf.Clamp(speed, 0.1f, 3f);
        ApplySpeedToAllAnimators();
    }

    /// <summary>
    /// Cambia los FPS de la animación
    /// </summary>
    /// <param name="newFPS">Frames por segundo (1-60)</param>
    public void SetAnimationFPS(int newFPS)
    {
        targetAnimationFPS = Mathf.Clamp(newFPS, 1, 60);
        updateInterval = 1f / targetAnimationFPS;
    }

    /// <summary>
    /// Restaura la velocidad original de las animaciones
    /// </summary>
    public void ResetToOriginalSpeed()
    {
        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null)
            {
                animators[i].speed = originalSpeeds[i];
            }
        }
    }

    /// <summary>
    /// Habilita/deshabilita el efecto Spider-Verse
    /// </summary>
    public void SetSpiderVerseEffect(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            // Restaurar updates normales
            foreach (var animator in animators)
            {
                if (animator != null)
                {
                    animator.speed = 1f;
                    animator.Update(0.1f); // Small update to refresh
                }
            }
        }
        else
        {
            // Re-aplicar configuración Spider-Verse
            ApplySpeedToAllAnimators();
        }
    }

    /// <summary>
    /// Habilita/deshabilita la variación de velocidad entre animadores
    /// </summary>
    public void SetSpeedVariation(bool enabled, float variationAmount = 0.2f)
    {
        enableSpeedVariation = enabled;
        speedVariation = Mathf.Clamp(variationAmount, 0.1f, 0.5f);
        ApplySpeedToAllAnimators();
    }

    private void ApplySpeedToAllAnimators()
    {
        foreach (var animator in animators)
        {
            if (animator != null)
            {
                ApplyAnimationSpeed(animator);
            }
        }
    }

    // ===== DEBUG INFO =====

    void OnGUI()
    {
#if UNITY_EDITOR
        if (Debug.isDebugBuild)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"SpiderVerse Animator: {targetAnimationFPS} FPS");
            GUI.Label(new Rect(10, 30, 300, 20), $"Animation Speed: {animationSpeed}x");
        }
#endif
    }
}
