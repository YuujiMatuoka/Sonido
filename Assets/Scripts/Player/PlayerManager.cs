using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private PlayerModel model;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private PlayerSettingsSO playerSettings;

    [Header("Sensitivity Multipliers")]
    public float gamepadSensitivityMultiplier = 200f;

    private AudioSource[] audioSources;
    private bool wasPlayingOnPause = false;

    [Header("Assistant Settings")]
    [SerializeField] private AssistantManager assistantManager;

    public PlaySound footstep;

    [Header("Toggle")]
    [SerializeField] private Toggle toggle;
    public PlayerModel Model => model;

    private PlayerController controller;
    private PlayerVisualController visual;
    public InteractionManager interactionManager;

    private Rigidbody rb;
    private Animator animator;
    public bool ToggleTograb;

    private PlayerInputAction input;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isUsingGamepad;

    void Awake()
    {
        input = new PlayerInputAction();
    }

    void OnEnable()
    {
        input.Player.Enable();

        // Movimiento
        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += _ => moveInput = Vector2.zero;

        // Mirar
        input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        input.Player.Look.canceled += _ => lookInput = Vector2.zero;

        //Asistente
        input.Player.Assistant.performed += ctx => ToggleAssistant();
    }

    void Start()
    {

        RegisterAudioSources();

        GameManager.OnGameStateChanged += OnGameStateChanged;

        rb = GetComponent<Rigidbody>();
        animator = transform.Find("RobotFinal")?.GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        model.useToggleGrab = ToggleTograb;

        interactionManager = new InteractionManager(
            _playerCamera,
            model.characterHand,
            model.interactableLayer,
            model.interactRange,
            model
        );

        controller = new PlayerController(model, rb, model.playerCamera, interactionManager,footstep);
        visual = new PlayerVisualController(model, animator, model.playerCamera.GetComponent<Camera>());

        if (toggle != null)
        {
            toggle.isOn = true;
        }

        SetInteractionMode(true);


        model.gameManager = FindObjectOfType<GameManager>();
        GameManager.OnGameStateChanged += OnGameStateChanged;

        if (model.grapplingGun == null)
            model.grapplingGun = GetComponentInChildren<GrapplingGun>();

        if (model.grapplingRope == null)
            model.grapplingRope = GetComponentInChildren<GrapplingRope>();

        playerSettings.LoadSettings();
        model.mouseSensitivity = playerSettings.mouseSensitivity;

        playerSettings.OnSensitivityChanged += OnSensitivityChanged;
    }

    void Update()
    {
        isUsingGamepad = Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame;

        if (Input.GetKeyDown(KeyCode.Escape) || (Gamepad.current?.startButton.wasPressedThisFrame == true))
        {
            if (UIManager.Instance != null && UIManager.Instance.isInspecting)
            {
                UIManager.Instance.CloseInspection();
            }
            else
            {
                model.gameManager.TogglePause();
                model.useToggleGrab = ToggleTograb;
            }
        }

        if (model.gameManager.CurrentState != GameState.Playing)
            return;

        // Zoom con scroll / gatillos
        float scrollInput = Input.mouseScrollDelta.y;
        if (scrollInput == 0 && Gamepad.current != null)
        {
            if (Gamepad.current.leftShoulder.wasPressedThisFrame) scrollInput = 1f;
            if (Gamepad.current.rightShoulder.wasPressedThisFrame) scrollInput = -1f;
        }
        if (scrollInput != 0)
            controller.HandleHandZoom(scrollInput);

        // Construcción del struct inputs
        var inputs = new PlayerInputs
        {
            moveX = moveInput.x,
            moveZ = moveInput.y,
            mouseX = lookInput.x * model.mouseSensitivity * gamepadSensitivityMultiplier,
            mouseY = lookInput.y * model.mouseSensitivity * gamepadSensitivityMultiplier,
            isRuning = input.Player.Run.IsPressed(),
            isJumping = input.Player.Jump.WasPressedThisFrame(),
            isInteractKey = input.Player.Interact.WasPressedThisFrame(),
            isToggleKey = input.Player.Grab.WasPressedThisFrame(),
            isInspectKey = input.Player.Inspect.WasPressedThisFrame(),
            isGrappleKey = input.Player.Interact.WasPressedThisFrame()
        };

        // Detectar input de agarre continuo para gamepad
        bool gamepadGrabHold = false;
        bool gamepadGrabPressed = false;
        bool gamepadGrabReleased = false;

        if (Gamepad.current != null)
        {
            // Mapear el gatillo derecho o botón B (Button East) para agarre
            gamepadGrabHold = Gamepad.current.rightTrigger.isPressed || Gamepad.current.buttonEast.isPressed;
            gamepadGrabPressed = Gamepad.current.rightTrigger.wasPressedThisFrame || Gamepad.current.buttonEast.wasPressedThisFrame;
            gamepadGrabReleased = Gamepad.current.rightTrigger.wasReleasedThisFrame || Gamepad.current.buttonEast.wasReleasedThisFrame;
        }

        // Fallback para teclado/mouse 
        if (!isUsingGamepad && Gamepad.current == null)
        {
            inputs.moveX = Input.GetAxisRaw("Horizontal");
            inputs.moveZ = Input.GetAxisRaw("Vertical");
            inputs.mouseX = Input.GetAxis("Mouse X") * model.mouseSensitivity;
            inputs.mouseY = Input.GetAxis("Mouse Y") * model.mouseSensitivity;
            inputs.isRuning = Input.GetKey(KeyCode.LeftShift);
            inputs.isJumping = Input.GetKeyDown(KeyCode.Space);
            inputs.isInteractKey |= Input.GetKeyDown(KeyCode.E);
            inputs.isToggleKey |= Input.GetMouseButtonDown(0);
            inputs.isInspectKey |= Input.GetMouseButtonDown(1);
            inputs.isGrappleKey |= Input.GetMouseButtonDown(0);
        }
        else if (Gamepad.current != null)
        {
            // Para gamepad, usar los inputs específicos
            inputs.isToggleKey |= gamepadGrabPressed;
        }

        if (inputs.isGrappleKey && model.hasGrappleUpgrade)
        {
            // Lanzamos un rayo para ver si apuntamos a un anclaje
            Ray ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

            // Usamos la distancia y la capa del modelo (que ya tienes)
            if (Physics.Raycast(ray, out RaycastHit hit, model.maxGrappleDistance, model.grappleLayer))
            {
                // Cancelamos la acción de soltar el objeto.
                inputs.isToggleKey = false;
            }
        }

        // Manejo del input continuo (sujetar) - solo para modo HOLD
        if (!model.useToggleGrab)
        {
            bool isHoldingInput = Input.GetMouseButton(0) || gamepadGrabHold;
            bool inputPressed = Input.GetMouseButtonDown(0) || gamepadGrabPressed;
            bool inputReleased = Input.GetMouseButtonUp(0) || gamepadGrabReleased;

            controller.HandleContinuousInput(isHoldingInput);

            // Manejar animaciones para modo HOLD
            if (inputPressed && !model.isHoldingObject)
            {
                visual.PlayGrabAnimation();
            }
            else if (inputReleased && model.isHoldingObject)
            {
                visual.PlayReleaseAnimation();
            }
        }
        else
        {
            // Manejo para modo TOGGLE
            if (inputs.isToggleKey)
            {
                if (!model.isHoldingObject)
                {
                    // Intentar agarrar
                    visual.PlayGrabAnimation();
                }
                else
                {
                    // Soltar
                    visual.PlayReleaseAnimation();
                }
            }
        }

        controller.HandleInputs(inputs);
        visual.UpdateVisuals();

        // Rotación directa sin smooth (como antes)
        HandleLookInput();
    }

    private void RegisterAudioSources()
    {
        audioSources = GetComponentsInChildren<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            AudioManager.Instance.RegisterSource(source);
        }
    }

    public Camera GetCamera()
    {
        return _playerCamera;
    }
    public void RotatePlayer180Degrees()
    {
        if (controller != null)
        {
            controller.Rotate180Degrees();
        }
    }
    public void RotatePlayerTowardsPoint(Vector3 targetPoint)
    {
        if (controller != null)
        {
            controller.RotateTowardsPoint(targetPoint);
        }
    }
    private void ToggleAssistant()
    {
        if (assistantManager != null)
        {
            assistantManager.ToggleAssistant();
        }
    }

    private void HandleLookInput()
    {
        // La sensibilidad ya fue aplicada en la construcción de inputs
        if (lookInput.magnitude > 0.01f)
        {
            controller.RotateCamera(lookInput.x * model.mouseSensitivity, lookInput.y * model.mouseSensitivity);
        }
    }

    private void OnSensitivityChanged(float newSensitivity)
    {
        model.mouseSensitivity = newSensitivity;

        // Actualizar también en el modelo por si acaso
        if (playerSettings != null)
        {
            playerSettings.mouseSensitivity = newSensitivity;
        }
    }

    // Modificar el método existente para usar el nuevo sistema
    public void UpdateSensitivity(float value)
    {
        // Usar el método del ScriptableObject que notifica el cambio
        playerSettings.SetSensitivity(value);
    }

    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;

        if (playerSettings != null)
        {
            playerSettings.OnSensitivityChanged -= OnSensitivityChanged;
        }

        if (audioSources != null && AudioManager.Instance != null)
        {
            foreach (AudioSource source in audioSources)
            {
                if (source != null)
                    AudioManager.Instance.UnregisterSource(source);
            }
        }
    }

    private void OnGameStateChanged(GameState newState)
    {
        Cursor.lockState = (newState == GameState.Playing || newState == GameState.TimeTravel)
        ? CursorLockMode.Locked
        : CursorLockMode.None;

        // Manejar pausa/reanudación de audio
        HandleAudioPause(newState);
    }
    private void HandleAudioPause(GameState newState)
    {
        if (audioSources == null) return;

        if (newState == GameState.Paused)
        {
            // Pausar todos los audiosources
            foreach (AudioSource source in audioSources)
            {
                if (source != null && source.isPlaying)
                {
                    source.Pause();
                    wasPlayingOnPause = true;
                }
            }
        }
        else if (newState == GameState.Playing && wasPlayingOnPause)
        {
            // Reanudar audiosources que estaban reproduciéndose antes de pausar
            foreach (AudioSource source in audioSources)
            {
                if (source != null)
                {
                    source.UnPause();
                }
            }
            wasPlayingOnPause = false;
        }
    }

    public void SetInteractionMode(bool newValue)
    {
        // 1. Obtener el valor del ToggleTograb (la lógica actual ya es correcta para esto)
        if (toggle != null)
        {
            ToggleTograb = toggle.isOn;
        }
        else
        {
            ToggleTograb = newValue;
        }

        model.useToggleGrab = ToggleTograb;

        if (!ToggleTograb)
        {
            controller.HandleContinuousInput(false);

            interactionManager.ForceReleaseObject();
        }
    }

    void OnDisable() => input.Player.Disable();
    
}


public struct PlayerInputs
{
    public float moveX, moveZ, mouseX, mouseY;
    public bool isRuning, isJumping, isPickable, isRightClick,
                isInteractKey, isInspectKey, isToggleKey, isGrappleKey;
}