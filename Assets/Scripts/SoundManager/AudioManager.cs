using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSettingsSO settings;

    private List<AudioSource> registeredSources = new List<AudioSource>();

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Cargar y aplicar configuración guardada
        settings.Load();
        ApplyVolumes();

        // Sincronizar sliders si ya existen
        SyncSlidersInScene();
    }

    // Se puede llamar manualmente cuando se carga otra escena (ej: desde UIManager)
    public void SyncSlidersInScene()
    {
        var sliders = FindObjectsOfType<Slider>(true); // busca incluso desactivados
        foreach (var s in sliders)
        {
            string lowerName = s.gameObject.name.ToLower();

            if (lowerName.Contains("master"))
            {
                s.value = settings.masterVolume;
                s.onValueChanged.RemoveAllListeners();
                s.onValueChanged.AddListener(SetMasterVolume);
            }
            else if (lowerName.Contains("music"))
            {
                s.value = settings.musicVolume;
                s.onValueChanged.RemoveAllListeners();
                s.onValueChanged.AddListener(SetMusicVolume);
            }
            else if (lowerName.Contains("sfx"))
            {
                s.value = settings.sfxVolume;
                s.onValueChanged.RemoveAllListeners();
                s.onValueChanged.AddListener(SetSFXVolume);
            }
        }
    }

    public void RegisterSource(AudioSource source)
    {
        if (source != null && !registeredSources.Contains(source))
        {
            registeredSources.Add(source);
            ApplyVolumeToSource(source);
        }
    }

    public void UnregisterSource(AudioSource source)
    {
        registeredSources.Remove(source);
    }

    public void ApplyVolumes()
    {
        foreach (var src in registeredSources)
            ApplyVolumeToSource(src);
    }

    private void ApplyVolumeToSource(AudioSource source)
    {
        if (source == null) return;

        // Clasificación básica según nombre
        if (source.gameObject.name.ToLower().Contains("music"))
            source.volume = settings.musicVolume * settings.masterVolume;
        else
            source.volume = settings.sfxVolume * settings.masterVolume;
    }

    // Métodos optimizados para actualización en tiempo real
    public void SetMasterVolume(float value)
    {
        settings.masterVolume = value;
        ApplyVolumes();
        settings.Save();
    }

    public void SetMusicVolume(float value)
    {
        settings.musicVolume = value;
        ApplyVolumes();
        settings.Save();
    }

    public void SetSFXVolume(float value)
    {
        settings.sfxVolume = value;
        ApplyVolumes();
        settings.Save();
    }

    // Nuevo método para limpiar fuentes nulas (evita errores)
    private void CleanNullSources()
    {
        registeredSources.RemoveAll(source => source == null);
    }

    // Actualizar volúmenes periódicamente (opcional, para casos extremos)
    private void Update()
    {
        // Limpiar fuentes nulas cada cierto tiempo
        if (Time.frameCount % 60 == 0) // Cada 60 frames
        {
            CleanNullSources();
        }
    }
}
