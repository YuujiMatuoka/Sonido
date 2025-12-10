using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MaterialChange : MonoBehaviour
{
    [Header("Configuración de Materiales")]
    public Material MaterialL1Open;
    public Material MaterialL1Ready;
    public Material MaterialL1OnNeed;
    public Material MaterialL1Denied;

    private Renderer objectRenderer;

    // Referencia al animador de emisión
    private EmissionAnimator emissionAnimator;

    // Propiedad pública para saber qué material está activo actualmente
    public Material CurrentActiveMaterial { get; private set; }

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        // Buscamos el componente EmissionAnimator en el mismo objeto
        emissionAnimator = GetComponent<EmissionAnimator>();

        if (objectRenderer == null)
        {
            Debug.LogError("No Renderer component found on this object!");
            return;
        }

        // Establecer el material inicial como activo
        CurrentActiveMaterial = objectRenderer.material;
    }

    public void ChangeMaterialToL1Open()
    {
        SetMaterialImmediate(MaterialL1Open);
    }

    public void ChangeMaterialL1Ready()
    {
        SetMaterialImmediate(MaterialL1Ready);
    }

    public void ChangeMaterialL1OnNeed()
    {
        SetMaterialImmediate(MaterialL1OnNeed);
    }

    public void ChangeMaterialL1Denied()
    {
        SetMaterialImmediate(MaterialL1Denied);
    }

    // Método privado para hacer el cambio inmediato
    private void SetMaterialImmediate(Material newMaterial)
    {
        // Validaciones de seguridad
        if (objectRenderer == null || newMaterial == null) return;

        // Si ya es el material actual, no hacemos nada
        if (CurrentActiveMaterial == newMaterial) return;

        // 1. Asignación directa e inmediata del nuevo material
        objectRenderer.material = newMaterial;
        CurrentActiveMaterial = newMaterial;

        // 2. IMPORTANTE: Le avisamos al animador que el material cambió
        if (emissionAnimator != null)
        {
            emissionAnimator.UpdateMaterialReference();
        }
    }

    public bool IsCurrentMaterial(Material materialToCheck)
    {
        return CurrentActiveMaterial == materialToCheck;
    }
}