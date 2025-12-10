using UnityEngine;
using UnityEngine.VFX;


[RequireComponent(typeof(VisualEffect))]
public class VFXFrameController : MonoBehaviour
{
    [Header("Frame Settings")]
    [Tooltip("Cuántos cuadros por segundo tendrá el efecto (12 = Spider-Verse)")]
    public float targetFPS = 12f;

    [Tooltip("Avanza el tiempo de simulación aunque la escena esté pausada")]
    public bool unscaledTime = true;

    private VisualEffect vfx;
    private float frameInterval; 
    private float accumulator = 0f;

    private void Awake()
    {
        vfx = GetComponent<VisualEffect>();
        frameInterval = 1f / Mathf.Max(targetFPS, 0.1f);
    }

    private void OnEnable()
    {
        accumulator = 0f;
        if (vfx != null)
        {
            
            vfx.playRate = 1f;
            vfx.pause = false;
        }
    }

    private void Update()
    {
        if (vfx == null) return;

        float delta = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        accumulator += delta;

        
        if (accumulator >= frameInterval)
        {
            vfx.playRate = 1f; 
            vfx.Simulate(frameInterval, 1);
            accumulator = 0f;
        }
        else
        {
            vfx.playRate = 0f; 
        }
    }
}