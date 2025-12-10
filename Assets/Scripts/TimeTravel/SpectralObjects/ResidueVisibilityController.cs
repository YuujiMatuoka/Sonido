using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResidueVisibilityController : MonoBehaviour
{
    [Tooltip("Porción de la pantalla que debe estar visible (0.75 = 3/4)")]
    [SerializeField] private float screenPortion = 0.75f;

    [Tooltip("Tiempo en segundos antes de destruir el objeto")]
    [SerializeField] private float delayBeforeDestroy = 1f;

    [SerializeField] private Camera mainCamera;
    private bool isVisible = false;
    private float visibleTime = 0f;

    private void Start()
    {
        // Intentar obtener la cámara del PlayerManager primero
        PlayerManager playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager != null && playerManager.GetCamera() != null)
        {
            mainCamera = playerManager.GetCamera();
        }
        else
        {
            // Si no hay PlayerManager o no tiene cámara, usar la cámara principal
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        if (mainCamera == null) return;

        // Primera fase: detección de visibilidad
        if (!isVisible)
        {
            if (IsObjectVisible())
            {
                isVisible = true;
                visibleTime = Time.time;
            }
            return; // Salimos si aún no es visible
        }

        // Segunda fase: conteo para destrucción
        if (Time.time - visibleTime >= delayBeforeDestroy)
        {
            Destroy(gameObject);
            // gameObject.SetActive(false);
        }
    }

    private bool IsObjectVisible()
    {
        // Obtener la posición del objeto en coordenadas de pantalla
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);

        // Verificar si el objeto está dentro de la porción visible de la pantalla
        bool onScreen = screenPoint.z < 6 &&
                        screenPoint.x > (1 - screenPortion) * 0.5f &&
                        screenPoint.x < 1 - (1 - screenPortion) * 0.5f &&
                        screenPoint.y > (1 - screenPortion) * 0.5f &&
                        screenPoint.y < 1 - (1 - screenPortion) * 0.5f;

        return onScreen;
    }
}