using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartRobot : PickableTimeTravelObject
{
    private bool isAttached = false;
    private Transform attachedToRobot;
    [SerializeField] GameObject popUp;

    public override void OnTimeChanged(TimeState newTimeState)
    {
        base.OnTimeChanged(newTimeState);
        if (newTimeState == TimeState.L1)
            DetachFromRobot();
    }

    public void AttachToRobot(Transform robot)
    {
        if (isAttached) return;
        
        // Guardamos referencia
        attachedToRobot = robot;

        // Detenemos movimiento antes de enganchar
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Desactivamos colisiones físicas (no triggers)
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            if (!col.isTrigger)
                col.enabled = false;
        }

        // Ignorar colisiones futuras entre esta pieza y el robot
        foreach (var myCol in GetComponentsInChildren<Collider>())
        {
            foreach (var robotCol in robot.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(myCol, robotCol);
            }
        }

        // Posicionar correctamente en el robot
        transform.SetParent(robot);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Desactivar guardado de posición si estás usando time travel
        //usePositionSaving = false;
        visualL1Origen.SetActive(false);
        visualOriginBroken.SetActive(false);

        isAttached = true;
        Debug.Log($"{gameObject.name} attached to {robot.name}");
    }

    public bool IsAttached()
    {
        return isAttached;
    }

    public void DetachFromRobot()
    {
        if (isAttached)
        {
            transform.SetParent(null);
            GetComponent<Collider>().enabled = true; // Activar el collider al separar
            attachedToRobot = null;
            isAttached = false;
            Debug.Log($"{gameObject.name} detached from robot");
        }
    }
}
