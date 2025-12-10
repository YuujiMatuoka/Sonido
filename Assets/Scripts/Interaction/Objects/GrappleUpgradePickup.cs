using UnityEngine;

public class GrappleUpgradePickup : TimeTravelObject, IInteractable
{
    private bool pickedUp = false;
    private PlayerManager playerManager;

    protected override void Start()
    {
        base.Start();
        playerManager = FindAnyObjectByType<PlayerManager>();
    }  
    public void Interact()
    {
        if (pickedUp) return;
        // Otorga la mejora
        playerManager.Model.hasGrappleUpgrade = true;

        // Activa el visual del gancho si lo tienes instanciado
        //if (playerManager.Model.grapplingHook.hookVisualInstance != null)
        //    playerManager.Model.grapplingHook.hookVisualInstance.SetActive(true);

        // Feedback opcional (sonido, mensaje, etc.)
        Debug.Log("¡Mejora de gancho obtenida!");

        pickedUp = true;
        Destroy(gameObject); // Elimina el objeto de la escena
        // Verifica si el jugador colisiona

    }

}