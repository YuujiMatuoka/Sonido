using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackingGameController : MonoBehaviour
{
    public static HackingGameController Instance { get; private set; }
    public GameObject hackingGameCanvas;
    public GameObject player;

    private PlayerManager pM; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        pM= player.GetComponent<PlayerManager>();
        hackingGameCanvas.SetActive(false);
    }

    public void OpenMinigame()
    {
        hackingGameCanvas.SetActive(true);
        LockPlayer(true);
    }

    public void CloseMinigame()
    {
        hackingGameCanvas.SetActive(false);
        LockPlayer(false);
    }

    private void LockPlayer(bool locked)
    {
        if (pM != null)
        {
            pM.enabled = !locked; // Bloquea movimiento
        }

        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = locked;
    }
}
