using UnityEngine;

public class MapManager : MonoBehaviour, ITimeTravel
{
    [Header("Configuración de Cámara")]
    [SerializeField] private Camera playerCamera;

    // Definir los números de layer como constantes
    private const int LAYER_ORIGIN = 20;
    private const int LAYER_L1 = 21;

    private void Start()
    {
        // Si no se asignó la cámara en el inspector, intentar encontrarla
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("No se encontró la cámara principal. Asigna una cámara manualmente.");
            }
        }

        // Configurar el estado inicial
        OnTimeChanged(TimeTravelManager.Instance.CurrentTimeState);

        // Registrarse como observador del TimeTravelManager
        TimeTravelManager.Instance.RegisterObserver(this);
    }

    // Implementación de ITimeTravel
    public void OnTimeChanged(TimeState newTimeState)
    {
        switch (newTimeState)
        {
            case TimeState.Origin:
                // Activar layer Origen (20) y desactivar L1 (21)
                SetLayerState(LAYER_ORIGIN, true);
                SetLayerState(LAYER_L1, false);
                break;

            case TimeState.L1:
                // Activar layer L1 (21) y desactivar Origen (20)
                SetLayerState(LAYER_ORIGIN, false);
                SetLayerState(LAYER_L1, true);
                break;
        }
    }

    // Método para cambiar el estado de un layer específico
    private void SetLayerState(int layer, bool state)
    {
        if (playerCamera == null) return;

        if (state)
        {
            // Activar el layer (añadirlo a la máscara)
            playerCamera.cullingMask |= (1 << layer);
        }
        else
        {
            // Desactivar el layer (removerlo de la máscara)
            playerCamera.cullingMask &= ~(1 << layer);
        }
    }

    // Método opcional para verificar el estado actual de los layers
    private void LogCurrentLayerStates()
    {
        if (playerCamera == null) return;

        bool originEnabled = (playerCamera.cullingMask & (1 << LAYER_ORIGIN)) != 0;
        bool l1Enabled = (playerCamera.cullingMask & (1 << LAYER_L1)) != 0;

    }

    public void PreTimeChange(TimeState newTimeState)
    {
        // No necesitamos hacer nada antes del cambio de tiempo
        // Pero podríamos añadir efectos de transición aquí si es necesario
    }

    private void OnDestroy()
    {
        // Desregistrarse del TimeTravelManager al destruirse
        if (TimeTravelManager.Instance != null)
        {
            TimeTravelManager.Instance.UnregisterObserver(this);
        }
    }
}