using UnityEngine;

public interface IGrapplePoint
{
    bool IsValid();
    Vector3 GetPosition();
    void OnGrappleStart();
    void OnGrappleEnd();
}