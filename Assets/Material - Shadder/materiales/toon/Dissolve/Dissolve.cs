using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    [Header("Shader Settings")]
    public string shaderProperty = "_StartTime"; 
    public float duration = 1.5f;                // segundos que tarda la animación
    public int frames = 12;                      // "fps" del efecto Spider-Verse

    private Renderer rend;
    private Material mat;
    private Coroutine dissolveRoutine;

    private bool isPaused = false;
    private bool isDissolved = false;            // estado actual (false = visible)

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;
        if (!rend.enabled)
        {
            // Si el renderer está APAGADO, forzamos el shader
            // a su estado "disuelto" (1f)
            mat.SetFloat(shaderProperty, 1f);
            isDissolved = true;
        }
        else
        {
            // Si el renderer está ENCENDIDO, lo forzamos
            // a su estado "visible" (-1f)
            mat.SetFloat(shaderProperty, -1f);
            isDissolved = false;
        }      // aseguramos que arranque visible
    }
    void Start()
    {
        // Suscribirse al evento de pausa
        if (GameManager.Instance != null)
        {
            GameManager.OnGameStateChanged += OnGameStateChanged;
        }
    }
    private void OnGameStateChanged(GameState newState)
    {
        isPaused = (newState == GameState.Paused);
    }

    void Update()
    {
        if (isPaused) return;

        // Tecla G  desintegrar
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (dissolveRoutine != null) StopCoroutine(dissolveRoutine);
            dissolveRoutine = StartCoroutine(AnimateDissolve(true));
        }

        // Tecla F  reintegrar
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (dissolveRoutine != null) StopCoroutine(dissolveRoutine);
            dissolveRoutine = StartCoroutine(AnimateDissolve(false));
        }
    }

    public Coroutine Disintegrate()
    {
        if (dissolveRoutine != null) StopCoroutine(dissolveRoutine);

        // Almacena la corutina en la variable
        dissolveRoutine = StartCoroutine(AnimateDissolve(true));

        // Devuelve la corutina
        return dissolveRoutine;
    }

    public Coroutine Reintegrate()
    {
        if (dissolveRoutine != null) StopCoroutine(dissolveRoutine);
        dissolveRoutine = StartCoroutine(AnimateDissolve(false));
        return dissolveRoutine; // Devuelve la corutina
    }
    private System.Collections.IEnumerator AnimateDissolve(bool toDissolve)
    {
        float start = mat.GetFloat(shaderProperty);
        float end = toDissolve ? 1f : -1f;
        float t = 0f;

        // Aseguramos que el objeto esté visible al reintegrar
        if (!toDissolve)
            rend.enabled = true;

        while (t < duration)
        {
            if (!isPaused) // Solo avanzar tiempo si no está pausado
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01(t / duration);

                // Interpolación lineal
                float value = Mathf.Lerp(start, end, normalized);

                //  efecto "Spider-Verse": saltos de 12 fps
                float frameStep = 2f / frames;
                value = Mathf.Round(value / frameStep) * frameStep;

                mat.SetFloat(shaderProperty, value);
            }
            yield return null;
        }

        // Asegurar el valor final exacto
        mat.SetFloat(shaderProperty, end);

        // Si se disolvió, ocultar el renderer
        if (toDissolve)
            rend.enabled = false;

        isDissolved = toDissolve;
    }
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.OnGameStateChanged -= OnGameStateChanged;
        }
    }
}
