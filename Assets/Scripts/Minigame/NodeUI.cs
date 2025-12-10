using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NodeUI : MonoBehaviour
{
    [Header("Node Settings")]
    public bool isCorrupted = false;
    public List<NodeUI> connections = new List<NodeUI>();

    private Image image;
    private Button button;

    private void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void Start()
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        image.color = isCorrupted ? Color.red : Color.green;
    }

    private void OnClick()
    {
        if (isCorrupted)
        {
            RemoveNode();
            HackingManager.Instance.CheckVictory();
        }
    }

    private void RemoveNode()
    {
        // Desconectar de otros nodos
        foreach (var conn in connections)
        {
            conn.connections.Remove(this);
        }

        connections.Clear();

        Destroy(gameObject);
    }
}

