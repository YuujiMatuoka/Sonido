using UnityEngine;
using UnityEngine.UI;

public class SensitivitySlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private PlayerSettingsSO settings;
    private PlayerManager playerManager;

    private void Start()
    {
        slider.value = settings.mouseSensitivity;
        slider.onValueChanged.AddListener(OnSensitivityChanged);
        playerManager = FindObjectOfType<PlayerManager>();

        // Suscribirse para actualizar el slider si la sensibilidad cambia desde otro lugar
        settings.OnSensitivityChanged += OnSettingsSensitivityChanged;
    }

    public void OnSensitivityChanged(float value)
    {
        // Usar el método del ScriptableObject que notifica a todos los suscriptores
        settings.SetSensitivity(value);
    }

    // Método para cuando la sensibilidad cambia desde los settings
    private void OnSettingsSensitivityChanged(float newValue)
    {
        // Actualizar el slider visualmente sin disparar el evento onValueChanged
        slider.SetValueWithoutNotify(newValue);
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento
        if (settings != null)
        {
            settings.OnSensitivityChanged -= OnSettingsSensitivityChanged;
        }
    }
}
