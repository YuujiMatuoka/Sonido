using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AccessCard : PickableTimeTravelObject
{
    private bool isUsed = false;

    public bool IsUsed => isUsed;

    public void LockInPlace(Transform slotTransform)
    {
        isUsed = true;
        transform.position = slotTransform.position;
        transform.rotation = slotTransform.rotation;

        // desactiva física y collider para que no se saque
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        GetComponent<Collider>().enabled = false;
        OnDrop();
        StartCoroutine(DestroyObject());
    }



    private IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);

        Destroy(gameObject);
    }
}
