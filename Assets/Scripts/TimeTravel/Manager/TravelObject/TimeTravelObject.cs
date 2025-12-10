using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public abstract class TimeTravelObject : MonoBehaviour, ITimeTravel, IProtected, IInspectable
{
    [Header("Time Travel Settings")]
    [SerializeField] protected bool useFocus = true;
    [SerializeField] protected bool startProtected = false;
    [SerializeField] protected bool usePositionSaving = true;
    [SerializeField] protected bool isPickable = true;

    [Header("Time Visual Settings")]
    [SerializeField] protected GameObject visualL1Origen;
    [SerializeField] protected GameObject visualOriginBroken;
    [SerializeField] protected GameObject visualL1;

    [Header("Protection Settings")]
    [SerializeField] private Material protectionMaterial; // Material que se aplica cuando está protegido
    private List<Material[]> originalMaterials = new List<Material[]>(); // Para guardar los materiales originales
    private bool isProtectedVisualApplied = false;

    [Header("Audio Settings")]
    [SerializeField] protected AudioSource baseAudioSource;

    [Header("Inspection Audio")]
    [SerializeField] private AudioClip inspectionClip;
    [SerializeField] private float customInspectionDuration = 0f;

    [Header("Inspection Settings")]
    [SerializeField] public string objectName = "Object";


    [Header("Base Descriptions - All Objects")]
    [SerializeField, TextArea(1, 3)]
    private string originDescription = "Descripcion para objeto que esta en Origen pero con visual de L1.";

    [SerializeField, TextArea(1, 3)]
    private string brokenDescription = "Descripcion para Objeto en Origen y roto. No funciona en este estado.";

    [SerializeField, TextArea(1, 3)]
    private string L1Description = "Descripcion para Objeto en L1. Funciona en este estado.";

    [Header("Additional Descriptions - Object Specific")]
    [SerializeField, TextArea(1, 3)]
    private string[] additionalDescriptions;


    [SerializeField] private Sprite objectIcon;



    [SerializeField] protected bool isProtected;
    [SerializeField] protected bool isBroken;
    protected bool isShield = false;

    [Header("World Popup Settings")]
    [SerializeField] public bool useWorldPopup = false;
    [SerializeField] private string worldPopupText = "Este objeto tiene una descripción.";
    [SerializeField] private Canvas worldPopupCanvas; // Referencia al Canvas en el objeto
    [SerializeField] private TextMeshProUGUI worldPopupTMP; // Texto del Canvas

    [Header("PopUp Timing")]
    [SerializeField] private float popupHideDelay = 1f; // Editable desde el Inspector
    private Coroutine hidePopupCoroutine;
    private bool isPopupActive = false;
    private bool lastPopupTextState = false;

    [Header("Interaction Capabilities")]
    [SerializeField] private bool isInspectable = true;
    [SerializeField] private bool isInteractable = false;
    [SerializeField] private bool isGrabbable = false;

    public bool IsInspectable => isInspectable;
    public bool IsInteractable => isInteractable;
    public bool IsGrabbable => isGrabbable;

    [Header("Popup Icons")]
    [SerializeField] private GameObject iconInspect;
    [SerializeField] private GameObject iconInteract;
    [SerializeField] private GameObject iconGrab;


    protected Rigidbody rb;
    protected Collider objectCollider;

    // Para posiciones entre líneas de tiempo
    protected Vector3 lastL1Position;
    protected Quaternion lastL1Rotation;
    protected bool hasBeenToL1;

    protected Vector3 originPositionBeforeTravel;
    protected Quaternion originRotationBeforeTravel;

    [SerializeField] protected GameObject originResiduePrefab; 
    protected GameObject currentResidue;


    private Material originalMaterial;
    private Renderer objectRenderer;
    private bool isHighlighted = false;

    TimeState startState;

    private TimeTwinLink linkedTwin;
    protected virtual void Awake()
    {
        isProtected = startProtected;
        startState = TimeTravelManager.Instance.CurrentTimeState;
        if(startState == TimeState.L1)
        {
            isBroken = false;
        }
        else
        {
            if (isProtected) isBroken = false; else isBroken = true;
        }
            

        hasBeenToL1 = true;
        
        rb = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
            originalMaterial = objectRenderer.material;

        lastL1Position = transform.position;  
        lastL1Rotation = transform.rotation;

        linkedTwin = GetComponent<TimeTwinLink>();
    }

    protected virtual void Start()
    {
        TimeTravelManager.Instance.RegisterObserver(this);

        if (baseAudioSource != null)
            AudioManager.Instance?.RegisterSource(baseAudioSource);

        TimeState currentState = TimeTravelManager.Instance.CurrentTimeState;

        // Primero desactivar todos los visuales
        if (visualL1Origen != null) visualL1Origen.SetActive(false);
        if (visualOriginBroken != null) visualOriginBroken.SetActive(false);
        if (visualL1 != null && visualL1 != visualL1Origen) visualL1.SetActive(false); // Solo si son diferentes

        // Luego activar solo el visual correspondiente al estado inicial
        if (currentState == TimeState.L1)
        {
            if (visualL1 != null)
                visualL1.SetActive(true);
            else if (visualL1Origen != null) // Fallback si visualL1 es null
                visualL1Origen.SetActive(true);
        }
        else if (currentState == TimeState.Origin)
        {
            if (isBroken && visualOriginBroken != null)
            {
                visualOriginBroken.SetActive(true);
            }
            else if (visualL1Origen != null)
            {
                visualL1Origen.SetActive(true);
            }
        }

        // Ahora aplicar protección si es necesario
        if (isProtected)
            UpdateProtectionVisual();

        if (useWorldPopup && worldPopupCanvas != null)
        {
            worldPopupCanvas.gameObject.SetActive(false);
        }
        if (worldPopupTMP != null)
        {
            worldPopupTMP.text = "";
            worldPopupTMP.gameObject.SetActive(false);
        }
    }


    protected virtual void Update()
    {
        if (!isPopupActive) return;

        UpdatePopupIcons(); // Reevaluamos constantemente
    }
    public AudioClip GetInspectionAudioClip()
    {
        return inspectionClip;
    }

    public float GetInspectionDisplayDuration()
    {
        if (customInspectionDuration > 0)
            return customInspectionDuration;

        // Auto-calcular basado en descripción
        string description = GetDescription();
        int wordCount = description.Split(' ').Length;
        return Mathf.Clamp(2f + (wordCount * 0.4f), 3f, 10f);
    }

    public virtual void OnTimeChanged(TimeState newTimeState)
    {
        switch (newTimeState)
        {
            case TimeState.L1:
                if (linkedTwin != null && linkedTwin.myTimeline == TimeState.Origin) break;
                isBroken = false;
                if (usePositionSaving)
                {
                    if (linkedTwin != null) break;
                    originPositionBeforeTravel = transform.position;
                    originRotationBeforeTravel = transform.rotation;
                    CreateBaseResidue();
                    transform.position = lastL1Position;
                    transform.rotation = lastL1Rotation;
                }
                break;

            case TimeState.Origin:
                if (!isProtected) isBroken = true;
                if (usePositionSaving)
                {
                    if (linkedTwin != null) break;
                    lastL1Position = transform.position;
                    lastL1Rotation = transform.rotation;
                    hasBeenToL1 = true;
                    ClearResidue();
                }
                break;
        }

        UpdateVisuals(newTimeState);
        ResetPhysics();
    }

    public virtual void CreateBaseResidue()
    {
        if (originResiduePrefab != null)
        {
            currentResidue = Instantiate(originResiduePrefab, originPositionBeforeTravel, originRotationBeforeTravel);
            OnResidueCreated(currentResidue, isBroken);
        }
    }

    protected virtual void ClearResidue()
    {
        if (currentResidue != null)
        {
            Destroy(currentResidue);
            currentResidue = null;
        }
    }

    protected virtual void OnResidueCreated(GameObject residueContainer, bool wasBroken) { }

    protected virtual void UpdateVisuals(TimeState currentState)
    {
        // 1. Desactivar todo (como ya lo haces)
        if (visualL1Origen != null && visualL1Origen.activeSelf)
        {
            RemoveProtectionFromRenderers(visualL1Origen.GetComponentsInChildren<Renderer>());
            visualL1Origen.SetActive(false);
        }
        if (visualOriginBroken != null && visualOriginBroken.activeSelf)
        {
            RemoveProtectionFromRenderers(visualOriginBroken.GetComponentsInChildren<Renderer>());
            visualOriginBroken.SetActive(false);
        }
        if (visualL1 != null && visualL1 != visualL1Origen && visualL1.activeSelf)
        {
            RemoveProtectionFromRenderers(visualL1.GetComponentsInChildren<Renderer>());
            visualL1.SetActive(false);
        }

        // 2. Decidir qué visual activar
        if (currentState == TimeState.L1)
        {
            // Comprobamos si este objeto es un gemelo Y si su timeline nativa es Origin
            if (linkedTwin != null && linkedTwin.myTimeline == TimeState.Origin)
            {
                if (visualOriginBroken != null)
                {
                    visualOriginBroken.SetActive(true);
                    if (isProtected) ApplyProtectionToRenderers(visualOriginBroken.GetComponentsInChildren<Renderer>());
                }
            }
            else
            {
                if (visualL1 != null)
                {
                    visualL1.SetActive(true);
                    if (isProtected) ApplyProtectionToRenderers(visualL1.GetComponentsInChildren<Renderer>());
                }
            }
        }
        else
        {

            if (linkedTwin != null && linkedTwin.myTimeline == TimeState.L1)
            {
                if (visualL1 != null)
                {
                    visualL1.SetActive(true);
                    if (isProtected) ApplyProtectionToRenderers(visualL1.GetComponentsInChildren<Renderer>());
                }
            }
            else
            {
                if (isBroken && visualOriginBroken != null && !isProtected)
                {
                    visualOriginBroken.SetActive(true);
                }
                else if (visualL1Origen != null)
                {
                    visualL1Origen.SetActive(true);
                    if (isProtected) ApplyProtectionToRenderers(visualL1Origen.GetComponentsInChildren<Renderer>());
                }
            }
        }
        
    }

    private void ApplyProtectionToRenderers(Renderer[] renderers)
    {
        if (isHighlighted) return;
        if (renderers == null) return;

        foreach (var renderer in renderers)
        {
            if (renderer is MeshRenderer meshRenderer)
            {
                var materials = meshRenderer.sharedMaterials.ToList();
                if (!materials.Contains(protectionMaterial))
                {
                    materials.Add(protectionMaterial);
                    meshRenderer.materials = materials.ToArray();
                }
            }
        }
    }

    private void RemoveProtectionFromRenderers(Renderer[] renderers)
    {
        if (renderers == null) return;

        foreach (var renderer in renderers)
        {
            if (renderer is MeshRenderer meshRenderer)
            {
                var materials = meshRenderer.sharedMaterials.ToList();
                if (materials.Remove(protectionMaterial))
                {
                    meshRenderer.materials = materials.ToArray();
                }
            }
        }
    }
    private void UpdateProtectionVisual()
    {
        if (isHighlighted) return;
        var renderers = GetActiveVisualRenderers();
        if (renderers == null) return;

        if (isProtected && !isProtectedVisualApplied && !isShield)
        {
            originalMaterials.Clear();
            foreach (var renderer in renderers)
            {
                if (renderer is MeshRenderer meshRenderer)
                {
                    // Verificar que no tenga ya el material de protección
                    bool alreadyProtected = meshRenderer.sharedMaterials.Any(m => m == protectionMaterial);
                    if (!alreadyProtected)
                    {
                        originalMaterials.Add(meshRenderer.sharedMaterials);
                        var materials = meshRenderer.sharedMaterials.ToList();
                        materials.Add(protectionMaterial);
                        meshRenderer.materials = materials.ToArray();
                    }
                }
            }
            isProtectedVisualApplied = true;
        }
        else if (!isProtected && isProtectedVisualApplied)
        {
            int index = 0;
            foreach (var renderer in renderers)
            {
                if (renderer is MeshRenderer meshRenderer && index < originalMaterials.Count)
                {
                    // Solo restaurar si el último material es el de protección
                    if (meshRenderer.sharedMaterials.LastOrDefault() == protectionMaterial)
                    {
                        meshRenderer.materials = originalMaterials[index];
                    }
                    index++;
                }
            }
            originalMaterials.Clear();
            isProtectedVisualApplied = false;
        }
    }




    public virtual void SetProtected(bool value)
    {
        if (isProtected != value)
        {
            isProtected = value;
            var activeRenderers = GetActiveVisualRenderers();

            if (isProtected)
            {
                ApplyProtectionToRenderers(activeRenderers);
            }
            else
            {
                RemoveProtectionFromRenderers(activeRenderers);
            }
        }
    }


    public bool IsProtected() => isProtected;

    public bool IsBroken() => isBroken;

    public void SetBroken(bool state) { isBroken = state; }

    protected virtual void Break()
    {
        isBroken = true;
        UpdateVisuals(TimeTravelManager.Instance.CurrentTimeState);
    }

    protected virtual void Repair()
    {
        isBroken = false;
        UpdateVisuals(TimeTravelManager.Instance.CurrentTimeState);
    }

    protected virtual void OnBreak() { }

    public virtual void ResetObject()
    {
        hasBeenToL1 = false;
        ResetPhysics();
    }

    private void ResetPhysics()
    {
        if (!usePositionSaving) return;

        if (rb == null) return;

        // Solo resetear físicas si el rigidbody es dinámico
        if (!rb.isKinematic)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    protected virtual void OnDestroy()
    {
        if (TimeTravelManager.Instance != null)
        {
            TimeTravelManager.Instance.UnregisterObserver(this);
        }

        if (baseAudioSource != null)
            AudioManager.Instance?.UnregisterSource(baseAudioSource);
    }

    // IInspectable

    private Renderer[] GetActiveVisualRenderers()
    {
        if (visualL1Origen != null && visualL1Origen.activeSelf)
            return visualL1Origen.GetComponentsInChildren<Renderer>();

        if (visualOriginBroken != null && visualOriginBroken.activeSelf)
            return visualOriginBroken.GetComponentsInChildren<Renderer>();

        if (visualL1 != null && visualL1.activeSelf)
            return visualL1.GetComponentsInChildren<Renderer>();

        return null;
    }


    public void OnPopUp()
    {
        if (!useFocus || UIManager.Instance.isInspecting) return;

        // Usar SOLO el rango de inspección para ambos (highlight y popup)
        float distance = Vector3.Distance(UIManager.Instance.mainCamera.transform.position, transform.position);
        float inspectRange = UIManager.Instance.inspectionRayLength;

        // Solo activar si está dentro del rango de inspección
        bool inRange = distance <= inspectRange;

        if (!inRange) return;

        if (useWorldPopup && worldPopupCanvas != null)
        {
            if (hidePopupCoroutine != null)
            {
                StopCoroutine(hidePopupCoroutine);
                hidePopupCoroutine = null;
            }

            isPopupActive = true;
            UpdatePopupIcons();
            worldPopupCanvas.gameObject.SetActive(true);
        }
    }

    public void UnPopUp()
    {
        if (!useFocus) return;

        if (useWorldPopup && worldPopupCanvas != null)
        {
            // Solo iniciar el contador si el popup está activo
            if (worldPopupCanvas.gameObject.activeSelf && hidePopupCoroutine == null)
            {
                hidePopupCoroutine = StartCoroutine(HidePopupAfterDelay());
            }
        }
    }
    private IEnumerator HidePopupAfterDelay()
    {
        yield return new WaitForSeconds(popupHideDelay);
        if (worldPopupCanvas != null)
        {
            worldPopupCanvas.gameObject.SetActive(false);
            isPopupActive = false; // Apagamos seguimiento
        }
    }
    private void UpdatePopupIcons()
    {
        float inspectRange = UIManager.Instance.inspectionRayLength;
        float interactRange = GameManager.Instance.PlayerManager.Model.interactRange;
        Transform camTransform = UIManager.Instance.mainCamera.transform;
        if (camTransform == null) return;

        float distance = Vector3.Distance(camTransform.position, transform.position);

        // Verificar rangos separados para cada tipo de acción
        bool inInspectRange = distance <= inspectRange;
        bool inInteractRange = distance <= interactRange;

        // Si no está en ningún rango, cerrar popup
        if (!inInspectRange && !inInteractRange)
        {
            if (worldPopupCanvas != null)
                worldPopupCanvas.gameObject.SetActive(false);
            isPopupActive = false;
            return;
        }

        // Mostrar íconos según capacidades Y rangos específicos
        if (iconInspect != null) iconInspect.SetActive(isInspectable && inInspectRange);
        if (iconInteract != null) iconInteract.SetActive(isInteractable && inInteractRange);
        if (iconGrab != null) iconGrab.SetActive(isGrabbable && inInteractRange);

        bool showOr = (isInteractable && isGrabbable) && inInteractRange;
        if (worldPopupTMP != null)
        {
            if (showOr != lastPopupTextState)
            {
                worldPopupTMP.text = showOr ? worldPopupText : "";
                lastPopupTextState = showOr; // Guarda el nuevo estado
            }
        }

        // Asegurar que el canvas esté activo si al menos un ícono se muestra
        bool shouldShowCanvas = (isInspectable && inInspectRange) ||
                               (isInteractable && inInteractRange) ||
                               (isGrabbable && inInteractRange);

        if (worldPopupCanvas != null && shouldShowCanvas)
        {
            worldPopupCanvas.gameObject.SetActive(true);
            isPopupActive = true;
        }
    }


    public void OnFocus()
    {
        
        if (!useFocus || isProtected) return;

        if (!isHighlighted)
        {
            var renderers = GetActiveVisualRenderers();
            if (renderers != null)
            {
                var highlightMat = UIManager.Instance.GetHighlightMaterial();
                foreach (var renderer in renderers)
                {
                    // Solo procesar MeshRenderers y evitar ParticleSystemRenderers
                    if (renderer is MeshRenderer meshRenderer)
                    {
                        var currentMaterials = meshRenderer.materials;
                        bool alreadyHasHighlight = false;

                        foreach (var mat in currentMaterials)
                        {
                            if (mat == highlightMat)
                            {
                                alreadyHasHighlight = true;
                                break;
                            }
                        }

                        if (!alreadyHasHighlight)
                        {
                            var newMaterials = new List<Material>(currentMaterials) { highlightMat };
                            meshRenderer.materials = newMaterials.ToArray();
                        }
                    }
                }
                isHighlighted = true;
            }
        }
    }

    public void OnUnfocus()
    {
        if (!useFocus) return;

        if (isHighlighted)
        {
            var renderers = GetActiveVisualRenderers();
            if (renderers != null)
            {
                foreach (var renderer in renderers)
                {
                    // Solo procesar MeshRenderers
                    if (renderer is MeshRenderer meshRenderer)
                    {
                        var currentMaterials = meshRenderer.materials;
                        if (currentMaterials.Length > 0)
                        {
                            var newMaterials = new Material[currentMaterials.Length - 1];
                            for (int i = 0; i < newMaterials.Length; i++)
                            {
                                newMaterials[i] = currentMaterials[i];
                            }
                            meshRenderer.materials = newMaterials;
                        }
                    }
                }
            }
            isHighlighted = false;
        }
    }

    public string GetDescription()
    {
        // Obtener el estado actual del TimeTravelManager
        TimeState currentState = TimeTravelManager.Instance.CurrentTimeState;

        string baseDescription = currentState switch
        {
            TimeState.Origin when isBroken => brokenDescription,
            TimeState.Origin => originDescription,
            TimeState.L1 => L1Description,
            _ => "Estado desconocido"
        };

        string additionalInfo = GetAdditionalDescriptionInfo();

        return $"{baseDescription}\n\n{additionalInfo}";
    }

    public bool CanBeInspected()
    {
        return isInspectable && gameObject.activeInHierarchy;
    }

    protected virtual string GetAdditionalDescriptionInfo()
    {
        // Método que las clases hijas pueden sobrescribir para añadir información específica
        return string.Join("\n", additionalDescriptions.Where(d => !string.IsNullOrEmpty(d)));
    }

    public void PreTimeChange(TimeState newTimeState)
    {
        return;
    }
}
