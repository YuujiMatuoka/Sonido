using UnityEngine;

[CreateAssetMenu(menuName = "Game/Object Pool Item")]
public class PoolItemSO : ScriptableObject
{
    public InteractableType type;
    public GameObject prefab;
    public int size = 5;
}
