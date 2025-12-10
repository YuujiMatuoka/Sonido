using UnityEngine;
using System.Collections;

public class InteractiveTimeDoor : TimeTravelObject
{
    [Header("Modelos")]
    [SerializeField] private Transform doorLeftL1;
    [SerializeField] private Transform doorLeftOrigen;
    [SerializeField] private Transform doorRightL1;
    [SerializeField] private Transform doorRightOrigen;

    [Header("Deslizamiento")]
    [SerializeField] private float slideDistance = 1f;
    [SerializeField] private float slideSpeed = 1f;

    [SerializeField] private bool isOpen = false;
    private bool lastL1State;

    [Header("Sonidos")]
    [SerializeField] private AudioSource soundSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    // Variables internas de referencia (Estilo TimeDoor)
    private Transform currentDoorLeft;
    private Transform currentDoorRight;

    // Posiciones calculadas dinámicamente
    private Vector3 currentLeftClosedPos;
    private Vector3 currentRightClosedPos;
    private Vector3 currentLeftOpenPos;
    private Vector3 currentRightOpenPos;

    // Posiciones base guardadas al inicio
    private Vector3 leftClosedPos_Origin;
    private Vector3 rightClosedPos_Origin;
    private Vector3 leftClosedPos_L1;
    private Vector3 rightClosedPos_L1;

    private bool isMoving = false;
    private bool initialized = false;

    protected override void Start()
    {
        base.Start();

        if (!initialized)
        {
            // Guardamos las posiciones originales de AMBOS sets de puertas
            leftClosedPos_Origin = doorLeftOrigen.position;
            rightClosedPos_Origin = doorRightOrigen.position;
            leftClosedPos_L1 = doorLeftL1.position;
            rightClosedPos_L1 = doorRightL1.position;

            initialized = true;
        }

        lastL1State = isOpen;

        // Inicializamos el estado visual
        UpdateActiveDoors(TimeTravelManager.Instance.CurrentTimeState);
    }

    private void UpdateActiveDoors(TimeState state)
    {

        bool useL1Visuals = (state == TimeState.L1) || (state == TimeState.Origin && !isBroken);

        doorLeftL1.gameObject.SetActive(useL1Visuals);
        doorRightL1.gameObject.SetActive(useL1Visuals);

        doorLeftOrigen.gameObject.SetActive(!useL1Visuals);
        doorRightOrigen.gameObject.SetActive(!useL1Visuals);

        currentDoorLeft = useL1Visuals ? doorLeftL1 : doorLeftOrigen;
        currentDoorRight = useL1Visuals ? doorRightL1 : doorRightOrigen;

        currentLeftClosedPos = useL1Visuals ? leftClosedPos_L1 : leftClosedPos_Origin;
        currentRightClosedPos = useL1Visuals ? rightClosedPos_L1 : rightClosedPos_Origin;


        currentLeftOpenPos = currentLeftClosedPos - (currentDoorLeft.right * slideDistance);
        currentRightOpenPos = currentRightClosedPos + (currentDoorRight.right * slideDistance);

        if (!isMoving)
        {
            currentDoorLeft.position = isOpen ? currentLeftOpenPos : currentLeftClosedPos;
            currentDoorRight.position = isOpen ? currentRightOpenPos : currentRightClosedPos;
        }
    }

    public override void OnTimeChanged(TimeState newTimeState)
    {
        TimeState previousState = TimeTravelManager.Instance.CurrentTimeState;

        base.OnTimeChanged(newTimeState);

        if (previousState == TimeState.L1 && newTimeState == TimeState.Origin)
        {
            isOpen = lastL1State;
        }
        else if (previousState == TimeState.Origin && newTimeState == TimeState.L1)
        {
            isOpen = lastL1State;
        }

        UpdateActiveDoors(newTimeState);
    }

    public void ToggleDoor()
    {
        // Si está roto, no hace nada (ni siquiera intenta moverse)
        if (isBroken) return;

        if (isMoving) return;

        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }

    private void OpenDoor()
    {
        isOpen = true;
        if (TimeTravelManager.Instance.CurrentTimeState == TimeState.L1) lastL1State = true;

        StartCoroutine(MoveDoors(currentLeftOpenPos, currentRightOpenPos));
    }

    private void CloseDoor()
    {
        isOpen = false;
        if (TimeTravelManager.Instance.CurrentTimeState == TimeState.L1) lastL1State = false;

        StartCoroutine(MoveDoors(currentLeftClosedPos, currentRightClosedPos));
    }

    private IEnumerator MoveDoors(Vector3 targetLeft, Vector3 targetRight)
    {
        isMoving = true;
        PlayDoorSound();

        // Movemos las puertas "Current" (las que definió UpdateActiveDoors)
        while (Vector3.Distance(currentDoorLeft.position, targetLeft) > 0.01f ||
               Vector3.Distance(currentDoorRight.position, targetRight) > 0.01f)
        {
            currentDoorLeft.position = Vector3.MoveTowards(currentDoorLeft.position, targetLeft, slideSpeed * Time.deltaTime);
            currentDoorRight.position = Vector3.MoveTowards(currentDoorRight.position, targetRight, slideSpeed * Time.deltaTime);
            yield return null;
        }

        currentDoorLeft.position = targetLeft;
        currentDoorRight.position = targetRight;
        isMoving = false;
    }

    private void PlayDoorSound()
    {
        if (soundSource != null)
        {
            if (isOpen && openSound != null)
            {
                soundSource.clip = openSound;
                soundSource.Play();
            }
            else if (!isOpen && closeSound != null)
            {
                soundSource.clip = closeSound;
                soundSource.Play();
            }
        }
    }

    public bool IsOpen => isOpen;
}