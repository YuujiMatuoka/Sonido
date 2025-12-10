using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MemoryCardConsole : TimeTravelObject, IInteractable
{
    [Header("Settings")]
    [SerializeField] private string requiredCardID;
    [SerializeField] private TimeDoor linkedDoor;

    public void Interact()
    {
        var playerModel = GameManager.Instance.PlayerManager?.Model;

        if (playerModel != null && playerModel.KnowsCard(requiredCardID) && linkedDoor.CanBeOpened())
        {
            linkedDoor.OpenDoor();
            Debug.Log("Console activated: Door opened.");
        }
        else
        {
            Debug.Log("Console: Access denied.");
        }
    }

    protected override string GetAdditionalDescriptionInfo()
    {
        var playerModel = GameManager.Instance.PlayerManager?.Model;
        if (playerModel.KnowsCard(requiredCardID))
        {
            return "I already have the codes for this door.";
        }
        return $"It requires a card with ID: {requiredCardID}.";
    }
}


