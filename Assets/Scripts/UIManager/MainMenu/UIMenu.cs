using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIMenu : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject SettingMenuPanel;
    public GameObject LoadingCanvas;

    [Header("Material Reference")]
    [SerializeField] private Material targetMaterial;

    [Header("Animation Settings")]
    [SerializeField] private float targetEmissionStrength = 3f;
    [SerializeField] private float emissionDuration = 2f;

    [Header("Settings References")]
    [SerializeField] private Animator CameraAnim;
    [SerializeField] private PlayerSettingsSO playerSettings;
    [SerializeField] private SensitivitySlider sensitivitySlider;

    [Header("Cursor Settings")]
    [SerializeField] private Animator Canvas_Animator;
    [SerializeField] private RectTransform cursorTransform;
    [SerializeField] private Image cursorImage;

    [Header("Gamepad Navigation")]
    [SerializeField] private float gamepadCursorSpeed = 800f;
    [SerializeField] private float gamepadCursorDeadZone = 0.2f;
    [SerializeField] private float gamepadSubmitCooldown = 0.3f;

    private bool isCursorActive = true;
    private Vector2 cursorPosition;
    private EventSystem eventSystem;

    // Gamepad variables
    private PlayerInputAction inputActions;
    private Vector2 gamepadCursorVelocity;
    private float lastSubmitTime;
    private bool isUsingGamepad = false;
    private float lastDeviceCheckTime;
    private InputDevice currentActiveDevice;

    private void Start()
    {
        InitializeMainMenu();
        InitializeCursor();
        SetupGamepadInput();

    }

    private void SetupGamepadInput()
    {
        inputActions = new PlayerInputAction();

        inputActions.UI.Submit.performed += OnSubmitPerformed;
        inputActions.UI.Cancel.performed += OnCancelPerformed;

        inputActions.UI.Enable();
    }

    private void UpdateActiveDevice()
    {
        if (Time.time - lastDeviceCheckTime < 0.1f) return;

        lastDeviceCheckTime = Time.time;

        InputDevice newDevice = GetActiveInputDevice();

        if (newDevice != currentActiveDevice)
        {
            currentActiveDevice = newDevice;
            bool wasUsingGamepad = isUsingGamepad;
            isUsingGamepad = (newDevice is Gamepad);

            if (isUsingGamepad != wasUsingGamepad)
            {
                OnInputDeviceChanged(isUsingGamepad);
            }
        }
    }

    private InputDevice GetActiveInputDevice()
    {
        if (Mouse.current != null &&
            (Mouse.current.delta.ReadValue().magnitude > 0.01f ||
             Mouse.current.leftButton.isPressed ||
             Mouse.current.rightButton.isPressed ||
             Mouse.current.middleButton.isPressed))
        {
            return Mouse.current;
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.isPressed)
        {
            return Keyboard.current;
        }

        if (Gamepad.current != null)
        {
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
            Vector2 dpad = Gamepad.current.dpad.ReadValue();
            float triggerInput = Mathf.Max(
                Gamepad.current.leftTrigger.ReadValue(),
                Gamepad.current.rightTrigger.ReadValue()
            );

            if (leftStick.magnitude > 0.1f ||
                rightStick.magnitude > 0.1f ||
                dpad.magnitude > 0.1f ||
                triggerInput > 0.1f ||
                Gamepad.current.aButton.isPressed ||
                Gamepad.current.bButton.isPressed ||
                Gamepad.current.xButton.isPressed ||
                Gamepad.current.yButton.isPressed)
            {
                return Gamepad.current;
            }
        }

        return currentActiveDevice ?? Keyboard.current;
    }

    private void OnInputDeviceChanged(bool isGamepad)
    {
        isUsingGamepad = isGamepad;

        Debug.Log($"Dispositivo cambiado a: {(isGamepad ? "Gamepad" : "Mouse/Teclado")}");

        if (cursorTransform != null)
        {
            cursorTransform.gameObject.SetActive(true);

            if (isGamepad)
            {
                if (cursorImage != null) cursorImage.enabled = true;
                Cursor.visible = false;
            }
            else
            {
                if (cursorImage != null) cursorImage.enabled = false;
                Cursor.visible = true;
            }
        }

        if (isGamepad)
        {
            CenterCursor();
            SelectFirstButton();
        }
        else
        {
            if (eventSystem != null)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }
    }

    private void SelectFirstButton()
    {
        if (eventSystem == null) return;

        Button firstButton = null;

        if (SettingMenuPanel != null && SettingMenuPanel.activeSelf)
        {
            Button[] buttons = SettingMenuPanel.GetComponentsInChildren<Button>(true);
            if (buttons.Length > 0) firstButton = buttons[0];
        }
        else if (MainMenuPanel != null && MainMenuPanel.activeSelf)
        {
            Button[] buttons = MainMenuPanel.GetComponentsInChildren<Button>(true);
            if (buttons.Length > 0) firstButton = buttons[0];
        }

        if (firstButton != null)
        {
            eventSystem.SetSelectedGameObject(firstButton.gameObject);
            Debug.Log($"Botón seleccionado: {firstButton.gameObject.name}");
        }
    }

    private void InitializeCursor()
    {
        Cursor.visible = false; 
        Cursor.lockState = CursorLockMode.Confined;

        if (Canvas_Animator == null)
        {
            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj != null)
            {
                Canvas_Animator = canvasObj.GetComponent<Animator>();
            }
        }

        if (cursorTransform == null)
        {
            GameObject cursorObj = GameObject.Find("Cursor");
            if (cursorObj != null)
            {
                cursorTransform = cursorObj.GetComponent<RectTransform>();
                cursorImage = cursorObj.GetComponent<Image>();
            }
            else
            {
                CreateSimpleCursor();
            }
        }
        else if (cursorImage == null)
        {
            cursorImage = cursorTransform.GetComponent<Image>();
        }

        eventSystem = EventSystem.current;

        isUsingGamepad = false;
        if (cursorImage != null) cursorImage.enabled = false;
        CenterCursor();
    }

    private void CreateSimpleCursor()
    {
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null) return;

        GameObject cursorObj = new GameObject("MenuCursor");
        cursorObj.transform.SetParent(mainCanvas.transform, false);

        cursorImage = cursorObj.AddComponent<Image>();
        cursorImage.color = new Color(1, 1, 1, 0.8f);
        cursorImage.raycastTarget = false; 

        Texture2D tex = new Texture2D(32, 32);
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                Color color = dist < 12 ? Color.white : Color.clear;
                tex.SetPixel(x, y, color);
            }
        }
        tex.Apply();

        Sprite circleSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        cursorImage.sprite = circleSprite;

        cursorTransform = cursorObj.GetComponent<RectTransform>();
        cursorTransform.sizeDelta = new Vector2(24, 24);
        cursorTransform.SetAsLastSibling();
    }

    private void InitializeMainMenu()
    {
        CameraAnim.SetBool("Settings", false);
        CameraAnim.SetBool("Play", false);

        LoadingCanvas.SetActive(false);
        SetEmissionStrength(0f);

        if (MainMenuPanel != null) MainMenuPanel.SetActive(true);
        if (SettingMenuPanel != null) SettingMenuPanel.SetActive(false);

        if (playerSettings != null)
        {
            playerSettings.LoadSettings();
            Debug.Log("Configuración cargada en menú principal");
        }

        if (sensitivitySlider != null)
        {
            Debug.Log("SensitivitySlider encontrado en menú principal");
        }
        else
        {
            sensitivitySlider = FindObjectOfType<SensitivitySlider>(true);
            if (sensitivitySlider != null)
            {
                Debug.Log("SensitivitySlider encontrado automáticamente");
            }
        }
    }

    private void Update()
    {
        UpdateActiveDevice();

        if (isUsingGamepad)
        {
            UpdateGamepadCursor();
            HandleGamepadNavigation();
        }
        else
        {
            UpdateMouseCursor();
        }
    }

    private void UpdateGamepadCursor()
    {
        if (cursorTransform == null || !isUsingGamepad || cursorImage == null || !cursorImage.enabled) return;

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

        Vector2 newPosition = cursorTransform.anchoredPosition + gamepadCursorVelocity * Time.unscaledDeltaTime;

        newPosition.x = Mathf.Clamp(newPosition.x, 0, Screen.width);
        newPosition.y = Mathf.Clamp(newPosition.y, 0, Screen.height);

        cursorTransform.anchoredPosition = newPosition;
        cursorPosition = newPosition;

        UpdateCursorAnimation();
    }

    private void UpdateMouseCursor()
    {
        if (!isCursorActive || cursorTransform == null || cursorImage == null || cursorImage.enabled) return;

        Vector2 mousePos = Input.mousePosition;
        cursorTransform.position = mousePos;
        cursorPosition = mousePos;

        UpdateCursorAnimation();

        HandleCursorInput();
    }

    private void HandleGamepadNavigation()
    {
        if (eventSystem != null && eventSystem.currentSelectedGameObject == null)
        {
            SelectFirstButton();
        }
    }

    private void UpdateCursorAnimation()
    {
        if (Canvas_Animator == null) return;

        bool isHoveringUI = IsHoveringUI();
        Canvas_Animator.SetBool("Inspect", isHoveringUI);

        if (cursorTransform != null)
        {
            if (isHoveringUI)
            {
                cursorTransform.localScale = Vector3.one * 1.1f;
            }
            else
            {
                cursorTransform.localScale = Vector3.one;
            }
        }
    }

    private bool IsHoveringUI()
    {
        if (eventSystem == null || cursorTransform == null) return false;

        var pointerData = new PointerEventData(eventSystem)
        {
            position = cursorTransform.position
        };

        var results = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject == cursorTransform.gameObject) continue;

            if (result.gameObject.GetComponent<Selectable>() != null)
            {
                return true;
            }
        }

        return false;
    }

    private void HandleCursorInput()
    {
        if (cursorTransform == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            cursorTransform.localScale = Vector3.one * 0.8f;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            cursorTransform.localScale = Vector3.one;
        }
    }

    private void OnSubmitPerformed(InputAction.CallbackContext context)
    {
        if (!isUsingGamepad || Time.unscaledTime - lastSubmitTime < gamepadSubmitCooldown)
            return;

        lastSubmitTime = Time.unscaledTime;

        if (cursorTransform != null)
        {
            StartCoroutine(ClickEffect());
        }

        if (eventSystem != null && eventSystem.currentSelectedGameObject != null)
        {
            ExecuteEvents.Execute(
                eventSystem.currentSelectedGameObject,
                new BaseEventData(eventSystem),
                ExecuteEvents.submitHandler
            );
        }
        else
        {
            SimulateClickAtCursor();
        }
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        if (!isUsingGamepad) return;

        Debug.Log("Botón B presionado en menú");

        if (SettingMenuPanel != null && SettingMenuPanel.activeSelf)
        {
            SettingDesactive();
        }
    }

    private IEnumerator ClickEffect()
    {
        if (cursorTransform == null) yield break;

        Vector3 originalScale = cursorTransform.localScale;
        cursorTransform.localScale = originalScale * 0.8f;

        yield return new WaitForSecondsRealtime(0.1f);

        bool isHovering = IsHoveringUI();
        cursorTransform.localScale = originalScale * (isHovering ? 1.1f : 1f);
    }

    private void SimulateClickAtCursor()
    {
        if (cursorTransform == null || eventSystem == null) return;

        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = cursorTransform.position
        };

        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject == cursorTransform.gameObject) continue;

            var button = result.gameObject.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.Invoke();
                Debug.Log($"Clic simulado en: {button.gameObject.name}");
                break;
            }
        }
    }

    public void CenterCursor()
    {
        if (cursorTransform != null)
        {
            cursorTransform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        }
    }

    public void SetCursorActive(bool active)
    {
        isCursorActive = active;

        if (cursorTransform != null)
        {
            cursorTransform.gameObject.SetActive(active);
        }

        Cursor.visible = !active;

        if (Canvas_Animator != null)
        {
            Canvas_Animator.SetBool("Inspect", false);
        }
    }


    public void DestroySpecificObject(GameObject targetObject)
    {
        if (targetObject != null)
        {
            Destroy(targetObject);
            Debug.Log("Objeto específico destruido: " + targetObject.name);
        }
    }

    public void SettingActive()
    {
        CameraAnim.SetBool("Settings", true);
        SettingMenuPanel.SetActive(true);
        MainMenuPanel.SetActive(false);
        AudioManager.Instance?.SyncSlidersInScene();

        if (isUsingGamepad)
        {
            SelectFirstButtonInPanel(SettingMenuPanel);
        }
    }

    public void SettingDesactive()
    {
        CameraAnim.SetBool("Settings", false);
        SettingMenuPanel.SetActive(false);
        MainMenuPanel.SetActive(true);

        if (isUsingGamepad)
        {
            SelectFirstButtonInPanel(MainMenuPanel);
        }
    }

    private void SelectFirstButtonInPanel(GameObject panel)
    {
        if (panel == null || eventSystem == null) return;

        var buttons = panel.GetComponentsInChildren<Button>(true);
        if (buttons.Length > 0)
        {
            eventSystem.SetSelectedGameObject(buttons[0].gameObject);
        }
    }

    public void LoadLevelOne()
    {
        CurvedTMPDownloading downloading = FindObjectOfType<CurvedTMPDownloading>(true);

        if (downloading != null)
        {
            downloading.gameObject.SetActive(true);
            StartCoroutine(PlayDownloadingAndLoad(downloading));
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    private IEnumerator PlayDownloadingAndLoad(CurvedTMPDownloading anim)
    {
        CameraAnim.SetBool("Play", true);

        anim.loop = false;
        anim.repeatCount = 2;
        anim.StartAnimation();

        float estimatedTime = (anim.GetComponent<TextMeshPro>().text.Length * anim.letterInterval * 2f + anim.pauseBeforeRestart * 2f) * anim.repeatCount;

        yield return new WaitForSeconds(estimatedTime);

        if (targetMaterial != null)
        {
            SetEmissionStrength(targetEmissionStrength);

            yield return new WaitForSeconds(emissionDuration);

            CameraAnim.SetBool("Play", false);
            CameraAnim.SetBool("Settings", false);
        }
        else
        {
            Debug.LogWarning("No se asignó el material objetivo para cambiar la emisión");
        }

        LoadingCanvas.SetActive(true);
        SetEmissionStrength(0f);

        Cursor.visible = true;
        SetCursorActive(false);

        SceneManager.LoadScene(1);
    }

    private void SetEmissionStrength(float strength)
    {
        if (targetMaterial != null)
        {
            targetMaterial.SetFloat("_EmissionStrength", strength);
            Debug.Log($"Emission strength cambiado a: {strength}");
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetPlayerSettings(PlayerSettingsSO settings)
    {
        playerSettings = settings;
    }

    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.UI.Submit.performed -= OnSubmitPerformed;
            inputActions.UI.Cancel.performed -= OnCancelPerformed;
            inputActions.UI.Disable();
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (Canvas_Animator != null)
        {
            Canvas_Animator.SetBool("Inspect", false);
        }
    }
}