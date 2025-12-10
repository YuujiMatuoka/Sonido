using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackingManager : MonoBehaviour
{
    public static HackingManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void CheckVictory()
    {
        NodeUI[] allNodes = FindObjectsOfType<NodeUI>();

        foreach (var node in allNodes)
        {
            if (node.isCorrupted) return; // Todavía quedan
        }

        Debug.Log("Puzzle completado!");
        HackingGameController.Instance.CloseMinigame(); //cerrar al ganar
    }
}
