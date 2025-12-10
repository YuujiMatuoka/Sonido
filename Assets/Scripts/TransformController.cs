using UnityEngine;

public class TeleportToObjects : MonoBehaviour
{
    [Header("Objetos Destino")]
    public GameObject[] targetObjects; // Asigna los objetos en el Inspector

    [Header("Opciones")]
    public bool copyPosition = true;
    public bool copyRotation = true;
    public bool copyScale = false;

    void Update()
    {
        CheckNumberKeys();
    }

    void CheckNumberKeys()
    {
        for (int i = 0; i < Mathf.Min(9, targetObjects.Length); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && targetObjects[i] != null)
            {
                TeleportToTarget(i);
            }
        }
    }

    void TeleportToTarget(int index)
    {
        GameObject target = targetObjects[index];

        if (copyPosition)
            transform.position = target.transform.position;

        if (copyRotation)
            transform.rotation = target.transform.rotation;

        if (copyScale)
            transform.localScale = target.transform.localScale;

        Debug.Log($"Teletransportado a: {target.name}");
    }
}