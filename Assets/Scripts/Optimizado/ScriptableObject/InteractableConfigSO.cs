using UnityEngine;

[CreateAssetMenu(menuName = "Game/Interactable Config")]
public class InteractableConfigSO : ScriptableObject
{
    public InteractableType type;
    public GameObject prefab;

    [Header("Pikeable Settings")]
    public bool hasPikeableSystem = false;
    public PikeableData pikeableData;

    [Header("Pool Settings")]
    public int poolSize = 5;
}