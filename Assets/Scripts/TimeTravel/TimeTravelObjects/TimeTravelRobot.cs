using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TimeTravelRobot : MonoBehaviour, ITimeTravel, IProtected, IInspectable
{
    [Header("Time Travel Settings")]
    [SerializeField] protected bool useFocus = true;
    [SerializeField] protected bool startProtected = false;
    [SerializeField] protected bool usePositionSaving = true;

    [Header("Time Visual Settings")]
    [SerializeField] protected GameObject visualL1Origen;
    [SerializeField] protected GameObject visualOriginBroken;
    [SerializeField] protected GameObject visualL1;

    [Header("Inspection Audio")]
    [SerializeField] private AudioClip inspectionClip;
    [SerializeField] private float customInspectionDuration = 0f;

    [Header("Inspection Settings")]


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

    [Header("World Popup Settings")]
    [SerializeField] public bool useWorldPopup = false;
    [SerializeField] private string worldPopupText = "Este objeto tiene una descripción.";
    [SerializeField] private Canvas worldPopupCanvas; // Referencia al Canvas en el objeto
    [SerializeField] private TextMeshProUGUI worldPopupTMP; // Texto del Canvas

    [Header("PopUp Timing")]
    [SerializeField] private float popupHideDelay = 1f; // Editable desde el Inspector
    private Coroutine hidePopupCoroutine;
    private bool isPopupActive = false;

    [Header("Interaction Capabilities")]
    [SerializeField] private bool isInspectable = true;
    [SerializeField] private bool isInteractable = false;
    [SerializeField] private bool isGrabbable = false;

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
    private BunnyModel model;

    protected virtual void Awake()
    {
        hasBeenToL1 = true;

        isProtected = startProtected;
        rb = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();
        objectRenderer = GetComponent<Renderer>();
        model = GetComponent<BunnyModel>();

        if (objectRenderer != null)
            originalMaterial = objectRenderer.material;

        lastL1Position = transform.position;
        lastL1Rotation = transform.rotation;


    }

    protected virtual void Start()
    {
        TimeTravelManager.Instance.RegisterObserver(this);

        // Al iniciar, todos rotos salvo protegidos
        isBroken = !isProtected;

        UpdateVisuals(TimeTravelManager.Instance.CurrentTimeState);

        if (useWorldPopup && worldPopupCanvas != null)
        {
            worldPopupCanvas.gameObject.SetActive(false);
        }
        if (worldPopupTMP != null)
        {
            worldPopupTMP.text = ""; // Lo actualiza UpdatePopupIcons()
            worldPopupTMP.gameObject.SetActive(false);
        }
    }
    protected virtual void Update()
    {
        if (!isPopupActive) return;

        UpdatePopupIcons();
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
                worldPopupCanvas.gameObject.SetActive(false);

                break;

            case TimeState.Origin:
                worldPopupCanvas.gameObject.SetActive(true);
                break;
        }

        UpdateVisuals(newTimeState);
        ResetPhysics();
    }

    protected virtual void CreateBaseResidue()
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
        if (visualL1Origen != null) visualL1Origen.SetActive(false);
        if (visualOriginBroken != null) visualOriginBroken.SetActive(false);
        if (visualL1 != null) visualL1.SetActive(false);

        if (currentState == TimeState.L1)
        {
            if (visualL1 != null) visualL1.SetActive(true);
        }
        else if (currentState == TimeState.Origin)
        {
            if (isBroken)
            {
                if (visualOriginBroken != null) visualOriginBroken.SetActive(true);
            }
            else
            {
                if (visualL1Origen != null) visualL1Origen.SetActive(true);
            }
        }
    }

    public void SetProtected(bool value) => isProtected = value;
    public bool IsProtected() => isProtected;

    public bool IsBroken() => isBroken;

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

        if (rb != null)
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
    }

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

        if (useWorldPopup && worldPopupCanvas != null)
        {
            if (model.HasFinishedRepair())
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
            else return;

        }
    }

    public void UnPopUp()
    {
        if (!useFocus) return;

        if (useWorldPopup && worldPopupCanvas != null)
        {
            if (model.HasFinishedRepair())
            {
                if (hidePopupCoroutine != null)
                    StopCoroutine(hidePopupCoroutine);

                if (currentResidue != null)
                    hidePopupCoroutine = StartCoroutine(HidePopupAfterDelay());
            }
            else return;
        }
    }
    private IEnumerator HidePopupAfterDelay()
    {
        yield return new WaitForSeconds(popupHideDelay);
        if (worldPopupCanvas != null)
        {
            worldPopupCanvas.gameObject.SetActive(false);
            isPopupActive = false; 
        }
    }
    private void UpdatePopupIcons()
    {
        float inspectRange = UIManager.Instance.inspectionRayLength;
        float interactRange = GameManager.Instance.PlayerManager.Model.interactRange + 0.6f;

        Transform camTransform = UIManager.Instance.mainCamera.transform;
        if (camTransform == null) return;

        float distance = Vector3.Distance(camTransform.position, transform.position);

        bool inInspectRange = distance <= inspectRange;
        bool inInteractRange = distance <= interactRange;

        bool showInspect = isInspectable && inInspectRange && !inInteractRange;
        bool showInteract = isInteractable && inInteractRange;
        bool showGrab = isGrabbable && inInteractRange;
        bool showOr = showInteract && showGrab;
        bool showTextOnly = !showInspect && !showInteract && !showGrab;

        // Si no está en ningún rango, cerramos el popup
        if (showTextOnly)
        {
            if (worldPopupCanvas != null)
                worldPopupCanvas.gameObject.SetActive(false);
            isPopupActive = false;
            return;
        }

        if (iconInspect != null) iconInspect.SetActive(showInspect);
        if (iconInteract != null) iconInteract.SetActive(showInteract);
        if (iconGrab != null) iconGrab.SetActive(showGrab);

        if (worldPopupTMP != null)
        {
            worldPopupTMP.gameObject.SetActive(showOr);
            worldPopupTMP.text = showOr ? worldPopupText : "";
        }
    }


    public void OnFocus()
    {
        if (!useFocus) return;

        if (!isHighlighted)
        {
            var renderers = GetActiveVisualRenderers();
            if (renderers != null)
            {
                var highlightMat = UIManager.Instance.GetHighlightMaterial();
                foreach (var renderer in renderers)
                {
                    var currentMaterials = renderer.materials;
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
                        renderer.materials = newMaterials.ToArray();
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
                    var currentMaterials = renderer.materials;
                    if (currentMaterials.Length > 0)
                    {
                        var newMaterials = new Material[currentMaterials.Length - 1];
                        for (int i = 0; i < newMaterials.Length; i++)
                        {
                            newMaterials[i] = currentMaterials[i];
                        }
                        renderer.materials = newMaterials;
                    }
                }
            }

            isHighlighted = false;
        }


    }

    public string GetDescription()
    {
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

    protected virtual string GetAdditionalDescriptionInfo()
    {
        return string.Join("\n", additionalDescriptions.Where(d => !string.IsNullOrEmpty(d)));
    }

    public void PreTimeChange(TimeState newTimeState)
    {
        return;
    }
}
