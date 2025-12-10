using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshPro))]
public class CurvedTMPDownloading : MonoBehaviour
{
    [Header("Curva")]
    public float radius = 10f;

    [Header("Animación")]
    public float letterInterval = 0.05f;
    public float pauseBeforeRestart = 0.5f;
    public bool autoStart = true;
    public bool loop = true;
    public int repeatCount = 2;

    [Header("Seguimiento de Cabeza")]
    public Transform headBone; // <<-- ASIGNA ESTO POR INSPECTOR
    public Vector3 positionOffset = new Vector3(0, 0.2f, 0.1f);
    public Vector3 rotationOffset = Vector3.zero;

    public bool playOnEnable = false;
    private TextMeshPro tmp;
    private Coroutine animRoutine;
    private bool animating;

    void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
        tmp.ForceMeshUpdate();
    }

    void OnEnable()
    {
        tmp.OnPreRenderText += OnTextRebuild;
        if (Application.isPlaying && playOnEnable)
            StartAnimation();
    }

    void OnDisable()
    {
        tmp.OnPreRenderText -= OnTextRebuild;
    }

    void Start()
    {
        if (Application.isPlaying && autoStart)
            StartAnimation();
    }

    void LateUpdate()
    {
        // Seguir la cabeza en cada frame
        if (headBone != null)
        {
            // Posición: offset relativo a la rotación de la cabeza
            transform.position = headBone.TransformPoint(positionOffset);

            // Rotación: misma rotación que la cabeza + offset adicional
            transform.rotation = headBone.rotation * Quaternion.Euler(rotationOffset);
        }
    }

    private void OnTextRebuild(TMP_TextInfo textInfo)
    {
        if (tmp == null || tmp.textInfo == null) return;
        var meshInfo = tmp.textInfo.meshInfo;

        for (int m = 0; m < meshInfo.Length; m++)
        {
            var vertices = meshInfo[m].vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vert = vertices[i];
                float x = vert.x / radius;
                float y = vert.y;
                vertices[i] = new Vector3(Mathf.Sin(x) * radius, y, Mathf.Cos(x) * radius - radius);
            }
        }
    }

    public void StartAnimation()
    {
        if (animating) return;
        animRoutine = StartCoroutine(AnimateText());
    }

    IEnumerator AnimateText()
    {
        animating = true;
        int loops = loop ? int.MaxValue : repeatCount;

        for (int l = 0; l < loops; l++)
        {
            tmp.ForceMeshUpdate();
            int totalChars = tmp.textInfo.characterCount;
            tmp.maxVisibleCharacters = 0;

            // Aparece de izquierda a derecha
            for (int i = 0; i <= totalChars; i++)
            {
                tmp.maxVisibleCharacters = i;
                yield return new WaitForSeconds(letterInterval);
            }

            yield return new WaitForSeconds(pauseBeforeRestart);

            // Apagado completo
            tmp.maxVisibleCharacters = 0;

            yield return new WaitForSeconds(pauseBeforeRestart);
        }

        animating = false;
    }
}

