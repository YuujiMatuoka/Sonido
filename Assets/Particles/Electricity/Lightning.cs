using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    public Transform final;
    public Transform start;
    public int cantidadDePuntos = 10;
    public float dispersion = 0.5f;
    public float frecuencia = 0.1f;

    private LineRenderer line;
    private float tiempo = 0f;

    private AudioSource mySound;
    private bool wasPlayingOnPause = false;

    private void Awake()
    {
        line = GetLine();
        mySound = GetComponent<AudioSource>();

        if (mySound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.RegisterSource(mySound);
        }

        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
            return;

        if (final == null || !line.enabled)
        {
            // Opcional: asegurarse de que la línea esté vacía si no hay objetivo
            if (line.positionCount > 0)
                line.positionCount = 0;
            return;
        }

        tiempo += Time.deltaTime;

        if (tiempo >= frecuencia)
        {
            ActualizarPuntos(line);
            tiempo = 0f;
        }
    }

    private LineRenderer GetLine()
    {
        return GetComponent<LineRenderer>();
    }

    private void ActualizarPuntos(LineRenderer line)
    {
        Vector3 startLocalPositionInLightningSpace = transform.InverseTransformPoint(start.position);

        Vector3 targetLocalPosition = transform.InverseTransformPoint(final.position);

        // Usamos la nueva posición local para calcular los puntos.
        // El 'principio' sigue siendo Vector3.zero (el pivote de este GameObject).
        List<Vector3> puntos = InterpolarPuntos(startLocalPositionInLightningSpace, targetLocalPosition, cantidadDePuntos);
        line.positionCount = puntos.Count;
        line.SetPositions(puntos.ToArray());
    }
    private void OnGameStateChanged(GameState newState)
    {
        // Manejar pausa/reanudación del sonido
        if (mySound == null) return;

        if (newState == GameState.Paused)
        {
            if (mySound.isPlaying)
            {
                mySound.Pause();
                wasPlayingOnPause = true;
            }
        }
        else if (newState == GameState.Playing && wasPlayingOnPause)
        {
            mySound.UnPause();
            wasPlayingOnPause = false;
        }
    }

    private List<Vector3> InterpolarPuntos(Vector3 principio, Vector3 final, int totalPoints)
    {
        List<Vector3> list = new List<Vector3>();

        for (int i = 0; i < totalPoints; i++)
        {
            Vector3 punto = Vector3.Lerp(principio, final, (float)i / totalPoints) + DesfaseAleatorio();
            list.Add(punto);
        }

        return list;
    }

    private Vector3 DesfaseAleatorio()
    {
        return Random.insideUnitSphere.normalized * Random.Range(0, dispersion);
    }

    public void PlaySound()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Paused)
        {
            mySound.Play();
        }
    }

    public void StopSound()
    {
        mySound.Stop();
        wasPlayingOnPause = false;
    }
    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;

        if (mySound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.UnregisterSource(mySound);
        }
    }
}
