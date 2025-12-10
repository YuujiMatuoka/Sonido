using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] public Camera mainCamera;
    [SerializeField] private LayerMask obstacleLayers;

    [Header("Cursor Settings")]
    [SerializeField] private RectTransform gameCursor;

    private Image cursorImage;
    private bool isPauseMode = false;
    private Vector3 originalLocalPosition;
    private Vector3 originalLocalScale;

    [Header("Inspection Settings")]
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] public float inspectionRayLength = 5f;
    [SerializeField] private LayerMask inspectableLayer;

    [Header("Interaction Feedback")]
    [SerializeField] private GameObject interactionFeedbackPanel;
    [SerializeField] private TextMeshProUGUI interactionFeedbackText;
    [SerializeField] private float interactionFeedbackDuration = 2f;

    [Header("Pause Menu")]
    [SerializeField] public GameObject pauseMenuPanel;
    [SerializeField] public GameObject SetingMenuPanel;
    [SerializeField] private TextMeshProUGUI pauseMenuText;
    [SerializeField] public Button resumeButton;

    [Header("Settings References")]
    [SerializeField] private PlayerSettingsSO playerSettings;
    [SerializeField] private SensitivitySlider sensitivitySlider;

    [SerializeField] private GameObject timeTravelEffect;

    [SerializeField] private Transform hands;
    private IInspectable currentInspectable;
    public bool isInspecting = false;

    private PlayerModel playerModel;
    private IInspectable currentInspectedObject;

    [Header("CanvasAnim")]
    [SerializeField] private Animator Canvas_Animator;

    [Header("Gamepad Navigation")]
    [SerializeField] private float gamepadCursorSpeed = 800f;
    [SerializeField] private float gamepadCursorDeadZone = 0.2f;

    private EventSystem eventSystem;

    private PlayerInputAction inputActions;
    private Vector2 gamepadCursorVelocity;
    private bool isUsingGamepad = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializeUI();
    }

    private void InitializeUI()
    {
        descriptionPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        SetingMenuPanel.SetActive(false);
        interactionFeedbackPanel.SetActive(false);

        InitializeCursor();

        playerModel = FindObjectOfType<PlayerManager>()?.Model;

        if (sensitivitySlider == null)
        {
            sensitivitySlider = FindObjectOfType<SensitivitySlider>(true);
        }

        if (playerSettings != null)
        {
            playerSettings.LoadSettings();
        }

        resumeButton.onClick.AddListener(() => {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
            {
                GameManager.Instance.SetGameState(GameState.Playing);
            }
        });

        GameManager.OnGameStateChanged += OnGameStateChanged;

        SetupGamepadInput();
        InputDeviceDetector.OnInputDeviceChanged += OnInputDeviceChanged;
    }

    private void SetupGamepadInput()
    {
        inputActions = new PlayerInputAction();

        inputActions.UI.Submit.performed += OnSubmitPerformed;
        inputActions.UI.Cancel.performed += OnCancelPerformed;

        inputActions.UI.Enable();
    }

    private void OnInputDeviceChanged(bool isGamepad)
    {
        isUsingGamepad = isGamepad;

        if (gameCursor != null)
        {
            gameCursor.gameObject.SetActive(isGamepad || isPauseMode);
        }

        Cursor.visible = !isGamepad;

        if (isGamepad && (pauseMenuPanel.activeSelf || SetingMenuPanel.activeSelf))
        {
            CenterPauseCursor();
            SelectFirstButtonInActivePanel();
        }
    }

    private void InitializeCursor()
    {
        eventSystem = EventSystem.current;

        if (gameCursor != null)
        {
            originalLocalPosition = gameCursor.localPosition;
            originalLocalScale = gameCursor.localScale;

            cursorImage = gameCursor.GetComponent<Image>();

            if (cursorImage != null)
            {
                cursorImage.raycastTarget = false;
                Debug.Log("Cursor configurado - Raycast Target: false");
            }

            gameCursor.gameObject.SetActive(true);

            Debug.Log("Cursor inicializado correctamente");
        }
        else
        {
            Debug.LogError("No se encontró ningún cursor en la escena");
        }
    }

    private void OnGameStateChanged(GameState newState)
    {
        bool isPaused = (newState == GameState.Paused);

        SetPauseCursorMode(isPaused);

        Cursor.lockState = (newState == GameState.Playing || newState == GameState.TimeTravel)
            ? CursorLockMode.Locked
            : CursorLockMode.None;

        Cursor.visible = isPaused;

        if (gameCursor != null)
        {
            gameCursor.gameObject.SetActive(true);
        }

        if (isPaused && isUsingGamepad)
        {
            CenterPauseCursor();
            SelectFirstButtonInActivePanel();
        }
    }

    private void SetPauseCursorMode(bool isPaused)
    {
        isPauseMode = isPaused;

        if (gameCursor == null) return;

        if (isPaused)
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            gameCursor.position = screenCenter;

            EnsureCursorInRootCanvas();
        }
        else
        {
            gameCursor.localPosition = originalLocalPosition;
            gameCursor.localScale = originalLocalScale;

            if (cursorImage != null)
            {
                cursorImage.color = Color.white;
            }
        }
    }

    private void EnsureCursorInRootCanvas()
    {
        if (gameCursor == null) return;

        Canvas parentCanvas = gameCursor.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ||
                parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                return;
            }
        }

        Canvas rootCanvas = FindObjectOfType<Canvas>();
        if (rootCanvas == null)
        {
            GameObject canvasObj = new GameObject("RootCanvas");
            rootCanvas = canvasObj.AddComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        gameCursor.SetParent(rootCanvas.transform, false);
        gameCursor.SetAsLastSibling();
    }

    private void Update()
    {
        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
        {
            if (!isUsingGamepad)
            {
                isUsingGamepad = true;
                OnInputDeviceChanged(true);
            }
        }
        else if ((Mouse.current != null && Mouse.current.wasUpdatedThisFrame) ||
                 (Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame))
        {
            if (isUsingGamepad)
            {
                isUsingGamepad = false;
                OnInputDeviceChanged(false);
            }
        }

        UpdateCursor();

        if (isInspecting)
        {
            if (mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            if (!Physics.Raycast(ray, inspectionRayLength, inspectableLayer))
            {
                CloseInspection();
            }
        }

        HandleInspectionRaycast();
        HandleInput();
    }

    private void UpdateCursor()
    {
        if (gameCursor == null) return;

        if (isPauseMode)
        {
            if (isUsingGamepad)
            {
                UpdateGamepadCursor();
            }
            else
            {
                UpdatePauseCursor();
            }
        }
        else
        {
            Canvas_Animator.SetBool("Inspect", false);
        }
    }

    private void UpdateGamepadCursor()
    {
        if (gameCursor == null || !isUsingGamepad) return;

        Vector2 navigateInput = inputActions.UI.Navigate.ReadValue<Vector2>();

        if (navigateInput.magnitude < gamepadCursorDeadZone)
        {
            navigateInput = Vector2.zero;
            gamepadCursorVelocity = Vector2.zero;
        }
        else
        {
            gamepadCursorVelocity = Vector2.Lerp(
                gamepadCursorVelocity,
                navigateInput * gamepadCursorSpeed,
                Time.unscaledDeltaTime * 10f
            );
        }

        Vector2 newPosition = gameCursor.anchoredPosition + gamepadCursorVelocity * Time.unscaledDeltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, 0, Screen.width);
        newPosition.y = Mathf.Clamp(newPosition.y, 0, Screen.height);

        gameCursor.anchoredPosition = newPosition;

        UpdateCursorAnimation();
    }

    private void UpdatePauseCursor()
    {
        gameCursor.position = Input.mousePosition;

        UpdateCursorAnimation();
        HandleCursorClickEffects();
    }

    private void UpdateCursorAnimation()
    {
        if (Canvas_Animator == null) return;

        bool isOverButton = IsCursorOverButton();

        Canvas_Animator.SetBool("Inspect", isOverButton);

        if (isOverButton)
        {
            gameCursor.localScale = originalLocalScale * 1.2f;
        }
        else
        {
            gameCursor.localScale = originalLocalScale;
        }
    }

    private bool IsCursorOverButton()
    {
        if (eventSystem == null) return false;

        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = gameCursor != null ? gameCursor.position : Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject == gameCursor?.gameObject) continue;

            if (result.gameObject.GetComponent<Button>() != null ||
                result.gameObject.GetComponent<Toggle>() != null ||
                result.gameObject.GetComponent<Slider>() != null ||
                result.gameObject.GetComponent<Dropdown>() != null)
            {
                return true;
            }
        }

        return false;
    }

    private void HandleCursorClickEffects()
    {
        if (gameCursor == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            gameCursor.localScale = originalLocalScale * 0.8f;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            bool isOverButton = IsCursorOverButton();
            gameCursor.localScale = originalLocalScale * (isOverButton ? 1.2f : 1f);
        }
    }

    private void OnSubmitPerformed(InputAction.CallbackContext context)
    {
        if (!isUsingGamepad || !isPauseMode) return;

        SimulateClickAtCursor();
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        if (!isUsingGamepad || !isPauseMode) return;

        if (SetingMenuPanel != null && SetingMenuPanel.activeSelf)
        {
            SettingDesactive();
            SelectFirstButtonInActivePanel();
        }
        else if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
            {
                GameManager.Instance.SetGameState(GameState.Playing);
            }
        }
    }

    private void SimulateClickAtCursor()
    {
        if (gameCursor == null || eventSystem == null) return;

        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = gameCursor.position
        };

        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject == gameCursor.gameObject) continue;

            var button = result.gameObject.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.Invoke();

                StartCoroutine(GamepadClickEffect());
                break;
            }
        }
    }

    private IEnumerator GamepadClickEffect()
    {
        if (gameCursor == null) yield break;

        Vector3 originalScale = gameCursor.localScale;
        gameCursor.localScale = originalScale * 0.8f;

        yield return new WaitForSecondsRealtime(0.1f);

        bool isOverButton = IsCursorOverButton();
        gameCursor.localScale = originalScale * (isOverButton ? 1.2f : 1f);
    }

    private void CenterPauseCursor()
    {
        if (gameCursor != null)
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            gameCursor.position = screenCenter;
        }
    }

    private void SelectFirstButtonInActivePanel()
    {
        if (eventSystem == null) return;

        GameObject activePanel = null;
        if (SetingMenuPanel != null && SetingMenuPanel.activeSelf)
            activePanel = SetingMenuPanel;
        else if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
            activePanel = pauseMenuPanel;

        if (activePanel != null)
        {
            var buttons = activePanel.GetComponentsInChildren<Button>(true);
            if (buttons.Length > 0)
            {
                eventSystem.SetSelectedGameObject(buttons[0].gameObject);
            }
        }
    }

    #region Input Handling
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (currentInspectable != null && !isInspecting)
            {
                ShowInspection(currentInspectable);
            }
            else if (isInspecting)
            {
                CloseInspection();
            }
        }
    }
    #endregion

    #region Inspection System (mantener igual)
    private void HandleInspectionRaycast()
    {
        if (isInspecting || mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if (Physics.Raycast(ray, out RaycastHit wallHit, inspectionRayLength, obstacleLayers))
        {
            float wallDistance = Vector3.Distance(mainCamera.transform.position, wallHit.point);

            if (Physics.Raycast(ray, out RaycastHit inspectHit, wallDistance, inspectableLayer))
            {
                ProcessInspectable(inspectHit);
            }
            else
            {
                HideCurrentInspectable();
            }
        }
        else
        {
            if (Physics.Raycast(ray, out RaycastHit inspectHit, inspectionRayLength, inspectableLayer))
            {
                ProcessInspectable(inspectHit);
            }
            else
            {
                HideCurrentInspectable();
            }
        }
    }

    private void ProcessInspectable(RaycastHit hit)
    {
        var inspectable = hit.collider.GetComponent<IInspectable>();
        if (inspectable != null && inspectable is TimeTravelObject timeObj && timeObj.CanBeInspected())
        {
            float distance = Vector3.Distance(mainCamera.transform.position, hit.transform.position);
            float inspectRange = inspectionRayLength;
            float interactRange = playerModel.interactRange;

            bool inInspectRange = distance <= inspectRange;
            bool inInteractRange = distance <= interactRange;
            bool shouldActivate = inInspectRange || inInteractRange;

            if (shouldActivate)
            {
                if (currentInspectable != null && currentInspectable != inspectable)
                {
                    currentInspectable.UnPopUp();
                    currentInspectable.OnUnfocus();
                }

                if (inInspectRange)
                {
                    inspectable.OnFocus();
                }

                inspectable.OnPopUp();

                if (!Canvas_Animator.GetBool("Inspect") && inInspectRange)
                {
                    Canvas_Animator.SetBool("Inspect", true);
                }

                currentInspectable = inspectable;
            }
            else
            {
                HideCurrentInspectable();
            }
        }
    }

    private void HideCurrentInspectable()
    {
        if (currentInspectable != null)
        {
            currentInspectable.UnPopUp();
            currentInspectable.OnUnfocus();
            currentInspectable = null;
            Canvas_Animator.SetBool("Inspect", false);
        }
    }

    public Material GetHighlightMaterial() => highlightMaterial;

    private void ShowInspection(IInspectable inspectable)
    {
        isInspecting = true;
        currentInspectedObject = inspectable;

        string description = inspectable.GetDescription();
        descriptionText.text = description;
        descriptionPanel.SetActive(true);
        inspectable.UnPopUp();

        ShowAssistantForInspection(description, inspectable);
    }

    private void ShowAssistantForInspection(string description, IInspectable inspectedObject)
    {
        if (AssistantManager.Instance != null)
        {
            AudioClip inspectionClip = inspectedObject.GetInspectionAudioClip();
            float duration = inspectedObject.GetInspectionDisplayDuration();

            AssistantManager.Instance.ShowExternalDialogueWithCallback(
                description,
                duration,
                inspectionClip,
                OnInspectionDialogueEnd
            );
        }
    }

    private void OnInspectionDialogueEnd()
    {
        if (isInspecting)
        {
            CloseInspection();
        }
    }

    public void CloseInspection()
    {
        isInspecting = false;
        descriptionPanel.SetActive(false);

        if (AssistantManager.Instance != null && AssistantManager.Instance.IsAssistantActive())
        {
            AssistantManager.Instance.ForceDismissAssistant();
        }

        currentInspectedObject = null;
    }
    #endregion

    #region Time Travel Effects
    public void OnTimeTravelEffect()
    {
        if (timeTravelEffect != null)
        {
            timeTravelEffect.SetActive(true);
            StartCoroutine(DeactivateAfter(1.5f));
        }
    }

    private IEnumerator DeactivateAfter(float time)
    {
        yield return new WaitForSeconds(time);
        if (timeTravelEffect != null)
            timeTravelEffect.SetActive(false);
    }
    #endregion

    #region Interaction Feedback
    public void ShowInteractionText(string text)
    {
        if (interactionFeedbackPanel != null && interactionFeedbackText != null)
        {
            interactionFeedbackText.text = text;
            interactionFeedbackPanel.SetActive(true);
            StopCoroutine(nameof(HideInteractionTextDelayed));
            StartCoroutine(nameof(HideInteractionTextDelayed));
        }
    }

    private IEnumerator HideInteractionTextDelayed()
    {
        yield return new WaitForSeconds(interactionFeedbackDuration);
        if (interactionFeedbackPanel != null)
        {
            interactionFeedbackPanel.SetActive(false);
            interactionFeedbackText.text = "";
        }
    }
    #endregion

    #region Pause Menu
    public void SettingActive()
    {
        if (SetingMenuPanel != null) SetingMenuPanel.SetActive(true);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        AudioManager.Instance?.SyncSlidersInScene();

        if (isUsingGamepad)
        {
            SelectFirstButtonInActivePanel();
        }
    }

    public void SettingDesactive()
    {
        if (SetingMenuPanel != null) SetingMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);

        if (isUsingGamepad)
        {
            SelectFirstButtonInActivePanel();
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1;
        GameObject sceneManager = GameObject.Find("SceneManagers");
        if (sceneManager != null)
        {
            Destroy(sceneManager);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(0);
    }
    #endregion

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
        InputDeviceDetector.OnInputDeviceChanged -= OnInputDeviceChanged;

        if (inputActions != null)
        {
            inputActions.UI.Submit.performed -= OnSubmitPerformed;
            inputActions.UI.Cancel.performed -= OnCancelPerformed;
            inputActions.UI.Disable();
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    #region Shader Fix for Build
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureShaderInBuild()
    {
        var shader = Shader.Find("Custom/URP_ShockWave");
        if (shader != null)
        {
            var tempMaterial = new Material(shader);
            tempMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
    }
    #endregion
}