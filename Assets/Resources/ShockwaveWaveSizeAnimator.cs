using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ShockwaveAnimatorFade : MonoBehaviour
{
    [Header("Configuración")]
    public float maxWaveSize = 5f;
    public float speed = 2f;            // velocidad de expansión
    public float fadeDuration = 0.5f;   // tiempo que tarda en desaparecer al final

    private Material mat;
    private float currentWaveSize = 0f;
    private bool animating = false;
    private bool fadingOut = false;

    private void Awake()
    {
        var renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
            mat = renderer.material;
    }

    private void OnEnable()
    {
        if (mat == null) return;

        mat.SetFloat("_WaveSize", 0f);
        mat.SetFloat("_Fade", 1f);
        currentWaveSize = 0f;
        animating = true;
        fadingOut = false;
    }

    private void Update()
    {
        if (!animating || mat == null) return;

        // expandir la onda
        if (!fadingOut)
        {
            currentWaveSize = Mathf.MoveTowards(currentWaveSize, maxWaveSize, speed * Time.deltaTime);
            mat.SetFloat("_WaveSize", currentWaveSize);

            if (Mathf.Approximately(currentWaveSize, maxWaveSize))
            {
                fadingOut = true;
                StartCoroutine(FadeOutCoroutine());
            }
        }
    }

    private System.Collections.IEnumerator FadeOutCoroutine()
    {
        float elapsed = 0f;
        float startFade = 1f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float fadeValue = Mathf.Lerp(startFade, 0f, elapsed / fadeDuration);
            mat.SetFloat("_Fade", fadeValue);
            yield return null;
        }

        mat.SetFloat("_Fade", 0f);
        animating = false;
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (mat != null)
        {
            mat.SetFloat("_WaveSize", 0f);
            mat.SetFloat("_Fade", 1f);
        }
        currentWaveSize = 0f;
        animating = false;
        fadingOut = false;
    }
}