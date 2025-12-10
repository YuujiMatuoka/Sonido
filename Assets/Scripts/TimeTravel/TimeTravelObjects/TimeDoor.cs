using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TimeDoor : TimeTravelObject
{
    [SerializeField] private bool isOpen = false;
    [SerializeField] private float slideDistance = 1f;
    [SerializeField] private float slideSpeed = 1f;

    [Header("Sonido")]
    [SerializeField] private AudioClip openSoundClip;
    [SerializeField] private AudioClip closeSoundClip;

    [Header("Origen Visuales")]
    [SerializeField] private Transform DoorOne_Origin;
    [SerializeField] private Transform DoorTwo_Origin;

    [Header("L1 Visuales")]
    [SerializeField] private Transform DoorOne_L1;
    [SerializeField] private Transform DoorTwo_L1;

    [Header("AnchorScene")]
    public Transform AnchorTransform;
    [HideInInspector] public string Scene_A;
    [HideInInspector] public string Scene_B;

    [HideInInspector] public string uniqueAnchorID;

    [Header("Lógica de Cierre")]
    [SerializeField] public Collider closingBlocker;


    [Header("Timing Config")]
    [SerializeField] private float preOpenDelay = 1f;
    [SerializeField] private float postCloseDelay = 0.5f;


    private Transform currentDoorOne;
    private Transform currentDoorTwo;

    private Vector3 doorOneClosed_Origin;
    private Vector3 doorTwoClosed_Origin;
    private Vector3 doorOneClosed_L1;
    private Vector3 doorTwoClosed_L1;

    private Vector3 doorOneOpenPosition;
    private Vector3 doorTwoOpenPosition;

    private bool isMoving = false;
    private AudioSource mySound;
    private bool lastL1State;

    private bool isLoaded = false;
    private bool isLoadingScene = false;

    [Header("Drop Object Trigger")]
    [SerializeField] private GameObject dropObjectTrigger;


    [Header("System Configuration")]
    public int systemID;
    public int phase;
    public bool IsMoving => isMoving;

    public bool IsOpen() => isOpen;


    protected override void Start()
    {
        base.Start();
        mySound = GetComponent<AudioSource>();
        lastL1State = isOpen;

        doorOneClosed_Origin = DoorOne_Origin.position;
        doorTwoClosed_Origin = DoorTwo_Origin.position;
        doorOneClosed_L1 = DoorOne_L1.position;
        doorTwoClosed_L1 = DoorTwo_L1.position;

        UpdateActiveDoors(TimeTravelManager.Instance.CurrentTimeState);

        if (closingBlocker != null)
        {
            closingBlocker.gameObject.SetActive(false);
        }

        if (mySound != null && openSoundClip != null)
        {
            mySound.clip = openSoundClip;
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

    private void UpdateActiveDoors(TimeState state)
    {
        bool isFuture = (state == TimeState.L1);
        bool useL1Visuals = isFuture || isProtected;

        DoorOne_Origin.gameObject.SetActive(!useL1Visuals);
        DoorTwo_Origin.gameObject.SetActive(!useL1Visuals);
        DoorOne_L1.gameObject.SetActive(useL1Visuals);
        DoorTwo_L1.gameObject.SetActive(useL1Visuals);

        currentDoorOne = useL1Visuals ? DoorOne_L1 : DoorOne_Origin;
        currentDoorTwo = useL1Visuals ? DoorTwo_L1 : DoorTwo_Origin;

        Vector3 closedOne = useL1Visuals ? doorOneClosed_L1 : doorOneClosed_Origin;
        Vector3 closedTwo = useL1Visuals ? doorTwoClosed_L1 : doorTwoClosed_Origin;

        doorOneOpenPosition = closedOne + (-currentDoorOne.right * slideDistance);
        doorTwoOpenPosition = closedTwo + (currentDoorTwo.right * slideDistance);

        if (!isMoving)
        {
            bool puertaAbierta = isOpen;

            currentDoorOne.position = puertaAbierta ? doorOneOpenPosition : closedOne;
            currentDoorTwo.position = puertaAbierta ? doorTwoOpenPosition : closedTwo;
        }
    }

    public void OpenDoor(bool useSceneA = true)
    {
        if (!isOpen && !isMoving && CanBeOpened())
        {
            StartCoroutine(OpenDoorWithEffects(useSceneA));
        }
    }

    private IEnumerator OpenDoorWithEffects(bool useSceneA)
    {
        if (dropObjectTrigger != null)
        {
            dropObjectTrigger.SetActive(true);
        }

        yield return StartCoroutine(PreOpenEffects(useSceneA));

        // 3. Abrir puerta: Asignar y reproducir clip de apertura
        if (mySound != null && openSoundClip != null)
        {
            mySound.clip = openSoundClip; // Aseguramos el clip de apertura
            mySound.Play();
        }
        yield return StartCoroutine(MoveDoors(doorOneOpenPosition, doorTwoOpenPosition));
        isOpen = true;

        if (TimeTravelManager.Instance.CurrentTimeState == TimeState.L1)
            lastL1State = true;

        if (closingBlocker != null)
        {
            closingBlocker.gameObject.SetActive(true);
            closingBlocker.isTrigger = true;
        }
    }

    public bool CanBeOpened()
    {
        if (TimeTravelManager.Instance.CurrentTimeState == TimeState.Origin && isProtected)
            return true;

        return !IsBroken();
    }


    public void CloseDoor(bool useSceneA = true)
    {
        if (isOpen && !isMoving)
        {
            StartCoroutine(CloseDoorAndUnloadScene(useSceneA));
        }
    }

    private IEnumerator CloseDoorAndUnloadScene(bool useSceneA)
    {
        if (dropObjectTrigger != null)
        {
            dropObjectTrigger.SetActive(false);
        }

        if (closingBlocker != null)
        {
            closingBlocker.gameObject.SetActive(true);
            closingBlocker.isTrigger = false;
        }

        Vector3 closedOne = currentDoorOne == DoorOne_L1 ? doorOneClosed_L1 : doorOneClosed_Origin;
        Vector3 closedTwo = currentDoorTwo == DoorTwo_L1 ? doorTwoClosed_L1 : doorTwoClosed_Origin;

        if (mySound != null && closeSoundClip != null)
        {
            mySound.clip = closeSoundClip;
            mySound.Play();
        }

        yield return StartCoroutine(MoveDoors(closedOne, closedTwo));
        isOpen = false;

        if (TimeTravelManager.Instance.CurrentTimeState == TimeState.L1)
            lastL1State = false;

        if (closingBlocker != null)
        {
            closingBlocker.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(postCloseDelay);

        if (isLoaded)
        {
            string sceneToUnload = useSceneA ? Scene_A : Scene_B;
            if (!string.IsNullOrEmpty(sceneToUnload))
            {
                SceneManagerCustom.Instance.UnloadScene(sceneToUnload);
            }
            isLoaded = false;
        }
    }

    private IEnumerator PreOpenEffects(bool useSceneA)
    {
        if (!isLoaded && !isLoadingScene)
        {
            string sceneToLoad = useSceneA ? Scene_A : Scene_B;

            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                isLoadingScene = true;
                SceneManagerCustom.Instance.LoadScene(sceneToLoad, OnSceneLoadComplete);
                isLoaded = true;

                while (isLoadingScene)
                {
                    yield return null;
                }
            }

        }
        yield return new WaitForSeconds(preOpenDelay);

    }

    private void OnSceneLoadComplete()
    {
        isLoadingScene = false;
    }

    private IEnumerator MoveDoors(Vector3 targetDoorOne, Vector3 targetDoorTwo)
    {
        isMoving = true;

        while (Vector3.Distance(currentDoorOne.position, targetDoorOne) > 0.01f ||
           Vector3.Distance(currentDoorTwo.position, targetDoorTwo) > 0.01f)
        {
            currentDoorOne.position = Vector3.MoveTowards(currentDoorOne.position, targetDoorOne, slideSpeed * Time.deltaTime);
            currentDoorTwo.position = Vector3.MoveTowards(currentDoorTwo.position, targetDoorTwo, slideSpeed * Time.deltaTime);
            yield return null;
        }

        currentDoorOne.position = targetDoorOne;
        currentDoorTwo.position = targetDoorTwo;
        isMoving = false;
    }

    public void ToggleDoor()
    {
        if (isOpen) CloseDoor(true);
        else OpenDoor(true);
    }


    public void SetDoorState(bool open, bool saveAsL1State = false)
    {
        isOpen = open;
        if (saveAsL1State)
            lastL1State = open;

        Vector3 closedOne = currentDoorOne == DoorOne_L1 ? doorOneClosed_L1 : doorOneClosed_Origin;
        Vector3 closedTwo = currentDoorTwo == DoorTwo_L1 ? doorTwoClosed_L1 : doorTwoClosed_Origin;

        doorOneOpenPosition = closedOne + (-currentDoorOne.right * slideDistance);
        doorTwoOpenPosition = closedTwo + (currentDoorTwo.right * slideDistance);

        if (!isMoving)
        {
            currentDoorOne.position = isOpen ? doorOneOpenPosition : closedOne;
            currentDoorTwo.position = isOpen ? doorTwoOpenPosition : closedTwo;
        }
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (DoorSpawnerManager.Instance != null)
        {
            DoorSpawnerManager.Instance.UnregisterDoor(uniqueAnchorID);
        }

        if (DoorSystemManager.Instance != null)
        {
            DoorSystemManager.Instance.UnregisterTimeDoor(this);
        }
    }
}