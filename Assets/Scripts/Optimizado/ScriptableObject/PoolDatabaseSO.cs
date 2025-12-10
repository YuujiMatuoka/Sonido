using UnityEngine;

[CreateAssetMenu(menuName = "Game/Object Pool Database")]
public class PoolDatabaseSO : ScriptableObject
{
    public PoolItemSO[] poolItems;
}
