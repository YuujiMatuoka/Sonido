using UnityEngine;

public class BillboardPopup : MonoBehaviour
{
    private Transform camTransform;

    private void Start()
    {
        camTransform = FindAnyObjectByType<Camera>().transform;
    }

    private void LateUpdate()
    {
        if (camTransform != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - camTransform.position);
        }
    }
}
