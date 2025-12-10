using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecundaryCardConsole : TimeTravelObject, IInteractable
{

    public CardConsole PrimeConsole;

    public void Interact()
    {
        PrimeConsole.BInteract();
    }
}
