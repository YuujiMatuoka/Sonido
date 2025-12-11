using System.Collections;
using UnityEngine;

public class PlayerController
{
    private readonly PlayerModel model;
    private readonly Rigidbody rb;
    private readonly Transform playerCamera;
    private readonly InteractionManager interactionManager;

    private readonly PlaySound footstep;

    private bool isHoldingClick;
    private float stepTimer = 0f;
    private CoroutineRunner coroutineRunner;

    // Clase interna para corrutinas
    private class CoroutineRunner : MonoBehaviour { }

    public PlayerController(
        PlayerModel model,
        Rigidbody rb,
        Transform cameraTransform,
        InteractionManager interactionManager,
        PlaySound playSound
    )
    {
        this.model = model;
        this.rb = rb;
        this.playerCamera = cameraTransform;
        this.interactionManager = interactionManager;
        this.footstep = playSound;
        model.defaultYPos = model.playerCamera.localPosition.y;

        // Crear GameObject para corrutinas
        GameObject runnerObj = new GameObject("CoroutineRunner");
        coroutineRunner = runnerObj.AddComponent<CoroutineRunner>();
        UnityEngine.Object.DontDestroyOnLoad(runnerObj);
    }

    // Maneja inputs discretos
    public void HandleInputs(PlayerInputs inputs)
    {
        UpdateMovement(inputs.moveX, inputs.moveZ, inputs.isRuning);
        RotateCamera(inputs.mouseX, inputs.mouseY);

        if (inputs.isJumping) Jump();
        if (inputs.isInteractKey) HandleInteraction();
        if (inputs.isInspectKey) HandleInspection();
        if (inputs.isGrappleKey) HandleGrappling();

        if (model.useToggleGrab)
        {
            if (inputs.isToggleKey) HandleToggleGrab();
        }
    }

    // Maneja input por toggle
    private void HandleToggleGrab()
    {
        interactionManager.TryToggleGrab();
    }

    // En el método HandleContinuousInput, asegurar que funcione correctamente
    public void HandleContinuousInput(bool isHolding)
    {
        if (isHolding != isHoldingClick)
        {
            interactionManager.TryInteract(
                InteractionType.LeftClick,
                isHolding
            );
            isHoldingClick = isHolding;

            // Debug para verificar que el input continuo funciona
            if (isHolding)
            {
                Debug.Log("Hold START - Intentando agarrar");
            }
            else
            {
                Debug.Log("Hold END - Intentando soltar");
            }
        }
    }

    private void HandleGrappling()
    {
        if (model.grapplingGun.IsGrappling())
        {
            model.grapplingGun.StopGrapple();
        }
        else
        {
            model.grapplingGun.StartGrapple();
        }
    }
    public void Footstep()
    {
        footstep.PlayFootstepSound();
    }
    private void UpdateMovement(float moveX, float moveZ, bool wantToRun)
    {
        UpdateGroundCheck();

        float speed = wantToRun ? model.runSpeed : model.moveSpeed;
        model.isRunning = wantToRun;

        Vector3 forward = playerCamera.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = playerCamera.right;
        right.y = 0f;
        right.Normalize();

        Vector3 moveDirection = (forward * moveZ + right * moveX).normalized;

        model.currentVelocity = new Vector3(
            moveDirection.x * speed,
            rb.velocity.y,
            moveDirection.z * speed
        );

        rb.velocity = model.currentVelocity;

        HandleGrapplePull();

        HandleHeadBob(moveX, moveZ, wantToRun);

        //// Reproducir sonido de pasos si se está moviendo
        //if (model.isGrounded && moveDirection.magnitude > 0.1f)
        //{
        //    stepTimer += Time.deltaTime;
        //    if (stepTimer >= model.stepInterval)
        //    {
        //        Footstep();
        //        stepTimer = 0f;
        //    }
        //}
        //else
        //{
        //    stepTimer = model.stepInterval;
        //}

        //// Añadir un chequeo adicional
        //if (moveDirection.magnitude < 0.1f)
        //{
        //    stepTimer = model.stepInterval * 0.5f; // Reset parcial
        //}

        //if (model.isGrounded && moveDirection.magnitude > 0.1f && model.isRunning)
        //{
        //    if (!model.runLoopSource.isPlaying)
        //    {
        //        int index = Random.Range(0, model.footstepClips.Count);
        //        model.runLoopSource.clip = model.runFootstepClips[index];
        //        model.runLoopSource.Play();
        //    }
        //}
        //else
        //{
        //    if (model.runLoopSource.isPlaying)
        //    {
        //        model.runLoopSource.Stop();
        //    }
        //}
    }

    private void HandleGrapplePull()
    {
        if (!model.grapplingGun.IsGrappling()) return;

        Vector3 grapplePoint = model.grapplingGun.GetGrapplePoint();
        Vector3 direction = (grapplePoint - rb.position);
        float distance = direction.magnitude;

        if (distance > model.stopDistance)
        {
            // Fuerza proporcional a la distancia (más suave cerca del punto)
            float normalizedDistance = Mathf.Clamp01((distance - model.stopDistance) /
                                                   (model.maxGrappleDistance - model.stopDistance));
            float pullStrength = model.pullSpeed * normalizedDistance;

            // Agregar un mínimo de fuerza para evitar que se quede atascado
            float minPullStrength = model.pullSpeed * 0.3f;
            pullStrength = Mathf.Max(pullStrength, minPullStrength);

            Vector3 pullForce = direction.normalized * pullStrength;
            rb.AddForce(pullForce, ForceMode.Acceleration);

        }
        else
        {
            model.grapplingGun.StopGrapple();
        }
    }

    private void PlayFootstep()
    {
        if (model.footstepSource != null && model.footstepClips.Count > 0)
        {
            int index = Random.Range(0, model.footstepClips.Count);
            model.footstepSource.clip = model.footstepClips[index];
            model.footstepSource.Play();
        }
    }

    private void HandleInteraction()
    {
        interactionManager.TryInteract(InteractionType.KeyPressE, true);
    }

    private void HandleInspection()
    {
        interactionManager.TryInteract(InteractionType.RightClick, true);
    }

    public void Jump()
    {
        // Verificar cooldowns y si está en el suelo
        bool isInReleaseCooldown = Time.time - model.lastReleaseTime < model.jumpCooldownAfterRelease;

        // Chequear más condiciones para evitar saltos falsos
        bool canJump = model.isGrounded &&
                       Time.time - model.lastJumpTime >= model.jumpCooldown &&
                       !isInReleaseCooldown &&
                       Mathf.Abs(rb.velocity.y) < 0.1f; // Más estricto

        if (!canJump) return;

        rb.AddForce(Vector3.up * model.jumpForce, ForceMode.Impulse);
        model.lastJumpTime = Time.time;
        model.isGrounded = false;

        // Forzar estado no grounded por un tiempo breve
        coroutineRunner.StartCoroutine(ForceUngrounded(0.2f));
    }

    private System.Collections.IEnumerator ForceUngrounded(float duration)
    {
        model.isGrounded = false;
        yield return new WaitForSeconds(duration);
        // Se recalculará en el próximo UpdateGroundCheck
    }

    public void RotateCamera(float mouseX, float mouseY)
    {
        rb.rotation = Quaternion.Euler(0f, rb.rotation.eulerAngles.y + mouseX * model.mouseSensitivity, 0f);

        model.xRotation -= mouseY * model.mouseSensitivity;
        model.xRotation = Mathf.Clamp(model.xRotation, -65f, 75f);
        playerCamera.localRotation = Quaternion.Euler(model.xRotation, 0f, 0f);
    }

    private void HandleHeadBob(float moveX, float moveZ, bool isRunning)
    {
        if (!model.enableHeadBob || !model.isGrounded) return;

        float speed = isRunning ? model.runBobSpeed : model.walkBobSpeed;
        float amount = isRunning ? model.runBobAmount : model.walkBobAmount;

        if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f)
        {
            // Calcula el movimiento de la cabeza
            model.timer += Time.deltaTime * speed;
            float waveSlice = Mathf.Sin(model.timer);
            float totalBob = waveSlice * amount;

            // Aplica el movimiento a la cámara
            Vector3 camPos = model.playerCamera.localPosition;
            camPos.y = model.defaultYPos + totalBob;
            model.playerCamera.localPosition = camPos;
        }
        else
        {
            // Resetea la posición cuando no hay movimiento
            model.timer = 0;
            Vector3 camPos = model.playerCamera.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, model.defaultYPos, Time.deltaTime * speed);
            model.playerCamera.localPosition = camPos;
        }
    }

    public void HandleHandZoom(float scrollInput)
    {
        if (scrollInput == 0) return;

        // Calcula la nueva distancia
        float newDistance = model.currentHandDistance + scrollInput * model.handZoomSpeed;

        // Aplica los límites
        newDistance = Mathf.Clamp(newDistance, model.minHandDistance, model.maxHandDistance);

        // Actualiza la distancia
        model.currentHandDistance = newDistance;

        // Mueve el characterHand en su eje local Z
        Vector3 localPos = model.characterHand.localPosition;
        localPos.z = newDistance;
        model.characterHand.localPosition = localPos;
    }

    public void UpdateGroundCheck()
    {
        bool wasGrounded = model.isGrounded;

        // Usar SphereCast para mejor consistencia en bordes
        Vector3 rayStart = rb.position + Vector3.up * 0.1f;
        float rayLength = model.groundCheckDistance + 0.1f;

        RaycastHit hit;
        bool groundHit = Physics.SphereCast(
            rayStart,
            model.groundCheckWidth * 0.5f,
            Vector3.down,
            out hit,
            rayLength,
            model.groundLayers
        );

        // Filtrado adicional: verificar ángulo de la superficie
        if (groundHit)
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            groundHit = angle <= model.maxSlopeAngle;
        }

        // Aplicar filtro temporal (debounce)
        if (groundHit != model.isGrounded)
        {
            model.groundCheckCooldown -= Time.deltaTime;
            if (model.groundCheckCooldown <= 0f)
            {
                model.isGrounded = groundHit;
                model.groundCheckCooldown = 0.1f; // 100ms de debounce
            }
        }
        else
        {
            model.groundCheckCooldown = 0f;
        }

        // Evento de aterrizaje
        //if (!wasGrounded && model.isGrounded)
        //{
        //    OnLanding();
        //}

        // Debug
        Color debugColor = model.isGrounded ? Color.green : Color.red;
        Debug.DrawRay(rayStart, Vector3.down * rayLength, debugColor);
        if (groundHit)
        {
            Debug.DrawLine(rayStart, hit.point, Color.yellow);
        }
    }

    private void OnLanding()
    {
        // Aquí puedes agregar efectos de aterrizaje si es necesario
        // Por ejemplo: sonido, partículas, etc.
        
    }

    // Método para limpiar recursos
    public void Dispose()
    {
        if (coroutineRunner != null && coroutineRunner.gameObject != null)
        {
            UnityEngine.Object.Destroy(coroutineRunner.gameObject);
        }
    }
    public void Rotate180Degrees()
    {
        // Rotar 180 grados en el eje Y
        float currentYRotation = rb.rotation.eulerAngles.y;
        float newYRotation = currentYRotation + 180f;

        // Aplicar la rotación directamente
        rb.rotation = Quaternion.Euler(0f, newYRotation, 0f);

    }
    public void RotateTowardsPoint(Vector3 targetPoint)
    {
        // Calcular dirección del jugador al punto objetivo
        Vector3 directionToTarget = targetPoint - rb.position;
        directionToTarget.y = 0f; // Ignorar diferencia en altura

        // Si está muy cerca, usar la dirección actual
        if (directionToTarget.magnitude < 0.1f)
        {
            return;
        }

        // Calcular la rotación para mirar hacia el punto
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget.normalized);

        // Aplicar solo la rotación en Y
        Vector3 eulerRotation = targetRotation.eulerAngles;
        rb.rotation = Quaternion.Euler(0f, eulerRotation.y, 0f);

    }
}