using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("Movimiento vertical")]
    public float amplitude = 0.15f;
    public float frequency = 1f;

    [Header("Rotación suave")]
    public float rotationAmplitude = 3f;
    public float rotationFrequency = 0.5f;

    [Header("Spider-Verse Effect")]
    [Range(1, 60)]
    public int animationFPS = 12;
    public bool applySpiderVerseEffect = true;

    private Vector3 startPos;
    private Quaternion startRot;
    private float timeOffset;
    private float updateInterval;
    private float lastUpdateTime;

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;

        // Variaciones aleatorias para naturalidad
        float randomOffset = Random.Range(0f, Mathf.PI * 2f);
        frequency += Random.Range(-0.2f, 0.2f);
        rotationFrequency += Random.Range(-0.1f, 0.1f);
        timeOffset = randomOffset;

        updateInterval = 1f / animationFPS;
        lastUpdateTime = Time.time;
    }

    void Update()
    {
        if (applySpiderVerseEffect)
        {
            // Actualizar solo en intervalos específicos para efecto Spider-Verse
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateFloatingAnimation();
                lastUpdateTime = Time.time;
            }
        }
        else
        {
            // Comportamiento original (suave)
            UpdateFloatingAnimation();
        }
    }

    void UpdateFloatingAnimation()
    {
        float time = Time.time + timeOffset;

        // Movimiento vertical
        float newY = startPos.y + Mathf.Sin(time * frequency) * amplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);

        // Rotación
        float rotX = Mathf.Sin(time * rotationFrequency) * rotationAmplitude;
        float rotZ = Mathf.Cos(time * rotationFrequency * 0.7f) * rotationAmplitude * 0.5f;
        transform.rotation = startRot * Quaternion.Euler(rotX, 0, rotZ);
    }

    // Métodos públicos para controlar el efecto
    public void SetAnimationFPS(int newFPS)
    {
        animationFPS = Mathf.Clamp(newFPS, 1, 60);
        updateInterval = 1f / animationFPS;
    }

    public void SetSpiderVerseEffect(bool enabled)
    {
        applySpiderVerseEffect = enabled;
        if (!enabled)
        {
            // Actualizar inmediatamente al desactivar
            UpdateFloatingAnimation();
        }
    }

    // Para resetear la posición si es necesario
    public void ResetToStart()
    {
        transform.position = startPos;
        transform.rotation = startRot;
    }
}