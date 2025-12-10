using UnityEngine;

public class Console : TimeTravelObject, IInteractable
{
    [SerializeField] private InteractiveTimeDoor linkedDoor;
    public AudioSource mySound;
    [SerializeField] MeshRenderer screen;
    [SerializeField] Material mat1;
    [SerializeField] Material mat2;

    protected override void Awake()
    {
        base.Awake();
        //screen.material = mat1;
    }

    public void Interact()
    {
        //if (TimeTravelManager.Instance.CurrentTimeState == TimeState.L1)
        //{
        //    if (mySound != null) mySound.Play();
        //    linkedDoor.ToggleDoor();
        //    if (linkedDoor.IsOpen)
        //    {
        //        screen.material = mat2;
        //    }
        //    else screen.material = mat1;
        //}

        if (!isBroken)
        {
            if (mySound != null) mySound.Play();
            linkedDoor.ToggleDoor();
            //if (linkedDoor.IsOpen)
            //{
            //    screen.material = mat2;
            //}
            //else screen.material = mat1;
        }
    }

    public string GetInteractionText()
    {
        return "Activar consola";
    }
}
