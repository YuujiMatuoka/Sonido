using UnityEngine;

public class TimeTwinLink : MonoBehaviour, ITimeTravel
{
    [Header("Setup")]
    public string twinId;
    public TimeState myTimeline = TimeState.Origin;
    public TimeTwinLink twin; // referencia a la otra versión (asignar en inspector)

    [Header("State (debug)")]
    [SerializeField] private bool suppressedInOwnTimeline;
    [SerializeField] private bool illegalVisibleInOtherTimeline;
    [SerializeField] private bool stabilizedByShield;
    [SerializeField] private bool isHeld;

    // NUEVO: Para guardar la posición cuando L1 se suelta en Origin
    [SerializeField] private Vector3 savedPositionInOrigin;
    [SerializeField] private Quaternion savedRotationInOrigin;
    [SerializeField] private bool hasSavedPositionInOrigin;
    private bool hasTravel = false;

    private Rigidbody rb;
    private Collider[] cols;
    private Renderer[] rends;
    private TimeTravelObject timeTravelObject;

    // cache para detectar movimiento del pasado
    private Vector3 lastPos;
    private Quaternion lastRot;

    // --- ayuda debug
    private void DBG(string msg) => Debug.Log($"[Twin:{twinId ?? name}] {msg}");

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cols = GetComponentsInChildren<Collider>(true);
        rends = GetComponentsInChildren<Renderer>(true);

        lastPos = transform.position;
        lastRot = transform.rotation;
        timeTravelObject = GetComponent<TimeTravelObject>();
    }

    private void Start()
    {
        if (TimeTravelManager.Instance != null)
            TimeTravelManager.Instance.RegisterObserver(this);
        else
            DBG("WARNING: TimeTravelManager missing");

        if (twin == null)
            DBG("AVISO: twin no asignado. Asignalo en inspector para comportamiento 'twin'.");

        SetVisibleBasedOnRules();
    }

    private void OnDestroy()
    {
        if (TimeTravelManager.Instance)
            TimeTravelManager.Instance.UnregisterObserver(this);
    }

    // === Notificaciones desde PickableTimeTravelObject ===
    public void NotifyGrabbed()
    {
        isHeld = true;
        //DBG($"NotifyGrabbed (global={TimeTravelManager.Instance.CurrentTimeState}, myTimeline={myTimeline})");

        if (TimeTravelManager.Instance.CurrentTimeState != myTimeline)
        {
            if (myTimeline == TimeState.Origin)
            {
                illegalVisibleInOtherTimeline = true;
                //DBG("marcado illegalVisibleInOtherTimeline = true (sosteniendo presente en pasado)");
                SetVisible(true);
            }
            else // myTimeline == L1
            {
                if (twin != null)
                {
                    twin.suppressedInOwnTimeline = true;  // Suprimir Origin
                    twin.SetVisibleBasedOnRules();
                }

                //Si agarramos L1 que estaba dejado en Origin, resetear el flag
                if (hasSavedPositionInOrigin)
                {
                    hasSavedPositionInOrigin = false;
                    //DBG("L1 agarrado desde Origin - reset hasSavedPositionInOrigin");
                }

                //DBG("marcado broughtAcrossToOtherTimeline = true");
                SetVisible(true);
            }
        }
    }

    public void NotifyDropped()
    {
        isHeld = false;
        DBG($"NotifyDropped (global={TimeTravelManager.Instance.CurrentTimeState}, myTimeline={myTimeline})");

        var current = TimeTravelManager.Instance.CurrentTimeState;

        if (current != myTimeline)
        {
            if (myTimeline == TimeState.Origin)
            {
                bool protectedHere = GetComponent<PickableTimeTravelObject>()?.IsProtected() ?? false;
                stabilizedByShield = protectedHere;
                illegalVisibleInOtherTimeline = protectedHere;

                DBG($"Dropped presente en pasado. protectedHere={protectedHere}. stabilizedByShield={stabilizedByShield}");

                SetVisible(protectedHere);

                if (!protectedHere)
                {
                    DBG("Objeto de Origen desactivado al soltar en L1 sin protección");
                    gameObject.SetActive(false);
                    return;
                }

                if (twin != null)
                {
                    twin.suppressedInOwnTimeline = false;
                    twin.SetVisibleBasedOnRules();
                }
            }
            else // myTimeline == L1
            {
                if (hasTravel)
                {
                    // GUARDAR posición en Origin cuando se suelta L1 en Origin
                    savedPositionInOrigin = transform.position;
                    savedRotationInOrigin = transform.rotation;
                    hasSavedPositionInOrigin = true;
                    DBG($"L1 guardó posición en Origin: {savedPositionInOrigin}");

                    if (twin != null)
                    {
                        twin.suppressedInOwnTimeline = true;
                        twin.SetVisibleBasedOnRules();
                    }
                }

            }
        }
        else
        {
            DBG("Dropped en su propia timeline.");
        }
    }

    // === Reglas en la transición de tiempo (PRE y POST) ===
    public void PreTimeChange(TimeState newTimeState)
    {
        if (isHeld && newTimeState != myTimeline)
        {
            //DBG($"PreTimeChange: isHeld y voy a cruzar -> newState={newTimeState}");
            if (myTimeline == TimeState.Origin)
            {
                hasTravel = true;
                illegalVisibleInOtherTimeline = true;
            }
            else // myTimeline == L1
            {
                hasTravel = true;
                if (twin != null)
                    twin.suppressedInOwnTimeline = true;
            }
        }
        else if(isHeld && newTimeState == myTimeline)
        {
            if(myTimeline == TimeState.L1)
            {
                twin.suppressedInOwnTimeline = false;
            }
        }

        if(!isHeld && newTimeState == myTimeline)
        {
            if(myTimeline == TimeState.Origin)
            {
                if(twin.timeTravelObject.IsProtected())
                {
                    timeTravelObject.SetProtected(true);
                    timeTravelObject.SetBroken(false);
                }
                else
                {
                    timeTravelObject.SetProtected(false);
                    timeTravelObject.SetBroken(true);
                }
            }
        }
    }

    public void OnTimeChanged(TimeState newTimeState)
    {
        DBG($"OnTimeChanged newState={newTimeState} (isHeld={isHeld})");
        bool protectedHere = GetComponent<PickableTimeTravelObject>()?.IsProtected() ?? false;
        stabilizedByShield = protectedHere;
        // --- 1. LÓGICA DE 'ISBROKEN' (TU PETICIÓN) ---
        // TimeTwinLink ahora decide si el objeto se rompe
        if (!isHeld)
        {
            if (newTimeState == TimeState.Origin && myTimeline == TimeState.Origin)
            {
                // Estamos en Origin. ¿Debemos rompernos?
                // Comprobamos si NOSOTROS (Origin) estamos protegidos
                bool amIProtected = timeTravelObject.IsProtected();

                // Comprobamos si el GEMELO (L1) está protegido
                bool isTwinProtected = false;
                if (twin != null && twin.timeTravelObject != null)
                {
                    isTwinProtected = twin.timeTravelObject.IsProtected();
                }
                if (isTwinProtected)
                {
                    amIProtected = true; // Si el gemelo está protegido, NOSOTROS también lo estamos
                    timeTravelObject.SetProtected(true); // Aseguramos que NOSOTROS también estemos protegidos
                }

            }

            if(myTimeline == TimeState.Origin && newTimeState == TimeState.L1)
            {
                if(timeTravelObject.IsProtected() && !twin.timeTravelObject.IsProtected())
                {
                    Debug.Log("xd");
                    timeTravelObject.SetProtected(false);
                    timeTravelObject.SetBroken(true);
                }
            }
            // Si volvemos a Origen y este twin es de Origen, reactivar y sincronizar posición
            if (myTimeline == TimeState.Origin && newTimeState == TimeState.Origin)
            {
                if (!isHeld && twin != null)
                {
                    // Si estaba inactivo (porque lo perdimos), lo reactivamos.
                    if (!gameObject.activeSelf)
                        gameObject.SetActive(true);

                    // ¡Esta es la línea clave que arregla tu bug!
                    // Sincroniza la posición con el twin de L1
                    transform.position = twin.transform.position;
                    transform.rotation = twin.transform.rotation;
                    DBG("Objeto de Origen sincronizado con twin de L1");
                }
            }

            // SOLO restaurar posición si NO está siendo sostenido y realmente cambió de timeline física
            if (myTimeline == TimeState.L1 && newTimeState == TimeState.Origin && hasSavedPositionInOrigin && !isHeld)
            {
                // Verificar que realmente necesitamos restaurar (cuando el objeto viajó físicamente)
                TimeState currentPhysical = GetCurrentPhysicalTimeline();
                if (currentPhysical == TimeState.Origin)
                {
                    transform.position = savedPositionInOrigin;
                    transform.rotation = savedRotationInOrigin;
                    timeTravelObject.SetBroken(false); // 
                    DBG($"L1 restaurado a posición guardada en Origin: {savedPositionInOrigin}");
                }
                else
                {
                    DBG($"L1 en Origin pero físicamente en L1 - NO restaurar posición");
                }
            }

            if (myTimeline == TimeState.L1 && newTimeState == TimeState.Origin && protectedHere && !isHeld)
            {
                Debug.Log("xdddd");
                hasSavedPositionInOrigin = false;
                hasTravel = false;

            }

            // Si el pasado vuelve a su propia timeline, ya no está "traido" al presente
            if (myTimeline == TimeState.L1 && newTimeState == TimeState.L1 && !hasSavedPositionInOrigin)
            {
                if (twin != null)
                {
                    twin.suppressedInOwnTimeline = false;
                    twin.SetVisibleBasedOnRules();
                }
                transform.position = lastPos;
                transform.rotation = lastRot;
                DBG("L1 regresó físicamente a L1 -> dessuprimí objeto Origin");
            }

            // Si volvemos a la propia timeline, limpiar flags temporales que no aplican ya
            if (newTimeState == myTimeline)
            {
                if(myTimeline == TimeState.Origin)
                {
                    illegalVisibleInOtherTimeline = false;
                }
                else
                {
                    timeTravelObject.SetProtected(false);
                    stabilizedByShield = false;
                    illegalVisibleInOtherTimeline = false;
                }

            }

            SetVisibleBasedOnRules();
        }
    }

    private void FixedUpdate()
    {
        // Dominancia del Pasado: si el pasado se mueve, dicta la posición del presente
        if (myTimeline == TimeState.L1 && TimeTravelManager.Instance.CurrentTimeState != TimeState.Origin)
        {
            if ((transform.position - lastPos).sqrMagnitude > 0.0004f ||
                Quaternion.Angle(transform.rotation, lastRot) > 0.5f)
            {
                lastPos = transform.position;
                lastRot = transform.rotation;

                if (twin != null)
                {
                    twin.ApplyPastAuthoritativePose(lastPos, lastRot);
                    //DBG("Past moved -> apliqué pose al presente via ApplyPastAuthoritativePose");
                }
            }
        }

        if (myTimeline == TimeState.Origin)
        {

            if (TimeTravelManager.Instance.CurrentTimeState == TimeState.L1)
            {
                if (!timeTravelObject.IsProtected())
                {
                    SetVisibleBasedOnRules();
                }
            }
        }
    }

    // Llamado por el PASADO hacia el PRESENTE cuando el pasado se movió
    public void ApplyPastAuthoritativePose(Vector3 pos, Quaternion rot)
    {
        if (myTimeline != TimeState.Origin) return; // solo aplica al presente

        // SOLO sincroniza y oculta si NO está siendo sostenido
        if (!isHeld)
        {
            transform.position = pos;
            transform.rotation = rot;

            if (TimeTravelManager.Instance.CurrentTimeState == TimeState.L1 &&
                (illegalVisibleInOtherTimeline || stabilizedByShield))
            {
                illegalVisibleInOtherTimeline = false;
                stabilizedByShield = false;
                SetVisible(false);
                //DBG("ApplyPastAuthoritativePose: limpié illegal/stabilized y oculté presente en pasado");
            }
        }
        else
        {
            //DBG("ApplyPastAuthoritativePose: ignorado porque está siendo sostenido");
        }
    }

    public void HandleAutomaticPortalEntry(TimeState targetTimeState)
    {
        TimeState currentPhysicalTimeline = GetCurrentPhysicalTimeline();

        DBG($"Portal: físico={currentPhysicalTimeline}, destino={targetTimeState}, puedeViajar={targetTimeState != currentPhysicalTimeline}");
        DBG($"Flags: hasSavedPositionInOrigin={hasSavedPositionInOrigin}, suppressed={suppressedInOwnTimeline}, active={gameObject.activeSelf}");

        DBG($"Portal automático: {myTimeline} -> {targetTimeState}");

        if (myTimeline == TimeState.L1)
        {
            if (targetTimeState == TimeState.L1 && hasSavedPositionInOrigin)
            {
                DBG($" L1 en Origin enviando de vuelta a L1");
                // Resetear flags para volver a L1
                hasSavedPositionInOrigin = false;

                if (twin != null)
                {
                    twin.suppressedInOwnTimeline = false;
                    twin.SetVisibleBasedOnRules();
                }
            }
            else if (targetTimeState == TimeState.Origin && !hasSavedPositionInOrigin)
            {
                DBG($" L1 en L1 enviando a Origin");
                // Marcar que ahora está en Origin
                hasSavedPositionInOrigin = true;
                savedPositionInOrigin = transform.position;
                savedRotationInOrigin = transform.rotation;

                if (twin != null)
                {
                    twin.suppressedInOwnTimeline = true;
                    twin.SetVisibleBasedOnRules();
                }
            }
        }
        else if (myTimeline == TimeState.Origin)
        {
            if (targetTimeState == TimeState.L1)
            {
                DBG($" Origin en Origin enviando a L1");

                // CRÍTICO: Teletransportarse a la posición del twin de L1
                if (twin != null)
                {
                    // Moverse a la posición actual del twin de L1
                    transform.position = twin.transform.position;
                    transform.rotation = twin.transform.rotation;
                    DBG($"Origin teletransportado a posición de L1: {transform.position}");

                    // Resetear el estado de L1
                    twin.hasSavedPositionInOrigin = false;

                    // Suprimirse a sí mismo
                    suppressedInOwnTimeline = true;
                    suppressedInOwnTimeline = false;
                    SetVisible(true);

                    twin.SetVisibleBasedOnRules();
                }
                else
                {
                    DBG("ERROR: No hay twin asignado para teletransportarse");
                }
            }
            else if (targetTimeState == TimeState.Origin && suppressedInOwnTimeline)
            {
                DBG($" Origin en L1 enviando de vuelta a Origin");
                // Origin regresa a su timeline
                suppressedInOwnTimeline = false;

                if (twin != null)
                {
                    twin.hasSavedPositionInOrigin = true;
                    twin.savedPositionInOrigin = twin.transform.position;
                    twin.savedRotationInOrigin = twin.transform.rotation;
                    twin.SetVisibleBasedOnRules();
                }
            }
        }

        SetVisibleBasedOnRules();
    }

    // === Visibilidad según reglas ===
    private void SetVisibleBasedOnRules()
    {
        var current = TimeTravelManager.Instance.CurrentTimeState;
        bool visible = false;

        if (myTimeline == TimeState.L1)
        {
            // L1 debe ser visible cuando:
            // 1. Estamos en Origin Y el objeto está físicamente en Origin (tiene posición guardada)
            // 2. Estamos en L1 Y el objeto está físicamente en L1 (NO tiene posición guardada)
            bool condition1 = (current == TimeState.Origin && hasSavedPositionInOrigin);
            bool condition2 = (current == TimeState.L1 && !hasSavedPositionInOrigin);
            //bool condition3 = (current == TimeState.Origin && timeTravelObject.IsProtected());
            visible = condition1 || condition2;

            //DBG($"SetVisibleBasedOnRules L1: current={current}, saved={hasSavedPositionInOrigin}, cond1={condition1}, cond2={condition2}, visible={visible}");
            //DBG($"Estado físico actual: {GetCurrentPhysicalTimeline()}");
        }
        else // Origin
        {
            // Comprobamos si el gemelo L1 está protegido
            bool isTwinProtected = false;
            if (twin != null && twin.timeTravelObject != null)
            {
                isTwinProtected = twin.timeTravelObject.IsProtected();
            }
            // Origin debe ser visible cuando:
            // Estamos en Origin Y no está suprimido
            bool condition1 = (current == TimeState.Origin) && !suppressedInOwnTimeline;
            bool condition2 = (current == TimeState.L1) && !suppressedInOwnTimeline && timeTravelObject.IsProtected();
            visible = condition1 || condition2;
            //visible = (current == TimeState.L1) && !suppressedInOwnTimeline && timeTravelObject.IsProtected();
            //DBG($"SetVisibleBasedOnRules Origin: current={current}, suppressed={suppressedInOwnTimeline}, visible={visible}");
        }

        // Excepción: siempre visible si está sostenido
        visible = visible || isHeld;

        //DBG($"SetVisibleBasedOnRules FINAL visible={visible} (isHeld={isHeld})");
        SetVisible(visible);
    }

    private void SetVisible(bool v)
    {
        foreach (var c in cols)
            c.enabled = v;

        foreach (var r in rends)
            r.enabled = v;

        if (rb != null)
        {
            if (v)
            {
                // Al hacerlo visible volvemos a dejar que la física actúe
                rb.isKinematic = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                // Al ocultar lo congelamos y limpiamos su estado físico
                rb.isKinematic = true;
                rb.Sleep(); // en vez de tocar velocity/angularVelocity
            }
        }
    }

    private TimeState GetCurrentPhysicalTimeline()
    {
        // Si es L1 y tiene posición guardada en Origin => está físicamente en Origin
        if (myTimeline == TimeState.L1 && hasSavedPositionInOrigin)
            return TimeState.Origin;

        // Si es Origin y está suprimido => está físicamente en L1
        if (myTimeline == TimeState.Origin && suppressedInOwnTimeline)
            return TimeState.L1;

        // Por defecto, está en su timeline original
        return myTimeline;
    }
}


