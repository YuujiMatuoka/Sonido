using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MemoryCard : PickableTimeTravelObject, IInteractable
{
    [Header("Card Settings")]
    [SerializeField] private string cardID;
    [TextArea]
    [SerializeField] private string learningInteractionText = "Reading card.......... learned codes.";
    [SerializeField] private string learnedInteractionText = "I know these codes already.";
    [TextArea]
    [SerializeField] private string notFunctionalInteractionText = "Reading card..........It doesn't contain any data.";

    public string CardID => cardID;

    private bool isFunctional => TimeTravelManager.Instance.CurrentTimeState == TimeState.L1 && !IsBroken();
    private bool hasTheCodes = false;

    public void Interact()
    {
        if (isFunctional)
        {
            var playerModel = GameManager.Instance.PlayerManager.Model;
            playerModel.LearnCard(cardID);
            Debug.Log($"Player learned card ID: {cardID}");
            if (!hasTheCodes)
            {
                // Llamar a la función en el UIManager para mostrar el texto de éxito
                UIManager.Instance.ShowInteractionText(learningInteractionText);
                hasTheCodes = true;
            }
          
            else
                // Llamar a la función en el UIManager para mostrar el texto de éxito
                UIManager.Instance.ShowInteractionText(learnedInteractionText);
        }
        else
        {
            Debug.Log("Card is not functional in this timeline.");
            // Llamar a la función en el UIManager para mostrar el texto de no funcional
            UIManager.Instance.ShowInteractionText(notFunctionalInteractionText);
        }
    }

    protected override string GetAdditionalDescriptionInfo()
    {
        var player = GameManager.Instance.PlayerManager?.Model;
        if (player != null && player.KnowsCard(cardID))
            return $"I know these codes already..";
        return $"A card with the code: {cardID}.";
    }
}


