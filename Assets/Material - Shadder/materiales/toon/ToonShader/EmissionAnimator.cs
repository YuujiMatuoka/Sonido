using UnityEngine;

public class EmissionAnimator : MonoBehaviour
{
    [Header("Shader Property")]
    [Tooltip("Nombre de la propiedad en el shader (por defecto _EmissionStrength)")]
    public string emissionProperty = "_EmissionStrength";

    [Header("Animación")]
    public float minValue = 2f;
    public float maxValue = 8f;
    public float speed = 1f;
    public float targetFPS = 12f;

    private float timeAccumulator = 0f;
    private float frameDuration;
    private float currentEmission;
    private bool goingUp = true;
    private Material targetMaterial;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        frameDuration = 1f / targetFPS;
        currentEmission = minValue;

        // Inicializamos la referencia
        UpdateMaterialReference();
    }

    // --- NUEVO MÉTODO PÚBLICO ---
    // Este método será llamado por el otro script cuando cambie el material
    public void UpdateMaterialReference()
    {
        if (rend == null) rend = GetComponent<Renderer>();

        // Actualizamos la referencia al material que está visible AHORA
        if (rend != null)
        {
            targetMaterial = rend.material;

            // Aplicamos el valor actual inmediatamente para que no haya saltos visuales
            if (targetMaterial.HasProperty(emissionProperty))
            {
                targetMaterial.SetFloat(emissionProperty, currentEmission);
            }
        }
    }

    void Update()
    {
        // Validaciones de seguridad
        if (!targetMaterial || !targetMaterial.HasProperty(emissionProperty))
            return;

        // limitar la actualización al frame rate elegido
        timeAccumulator += Time.deltaTime;
        if (timeAccumulator < frameDuration) return;
        timeAccumulator -= frameDuration;

        // animación ping-pong
        float direction = goingUp ? 1f : -1f;
        currentEmission += direction * speed * (maxValue - minValue) * frameDuration;

        if (currentEmission >= maxValue)
        {
            currentEmission = maxValue;
            goingUp = false;
        }
        else if (currentEmission <= minValue)
        {
            currentEmission = minValue;
            goingUp = true;
        }

        targetMaterial.SetFloat(emissionProperty, currentEmission);
    }
}