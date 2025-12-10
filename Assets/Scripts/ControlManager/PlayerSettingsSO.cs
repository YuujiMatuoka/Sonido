using UnityEngine;
using System;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Settings/PlayerSettings")]
public class PlayerSettingsSO : ScriptableObject
{
    [Range(0.1f, 10f)]
    public float mouseSensitivity = 4f;

    // Evento para notificar cambios
    public event Action<float> OnSensitivityChanged;

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey("MouseSensitivity"))
            mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity");
    }

    // Nuevo método para cambiar sensibilidad y notificar
    public void SetSensitivity(float value)
    {
        mouseSensitivity = value;
        SaveSettings();
        OnSensitivityChanged?.Invoke(value);
    }
}
