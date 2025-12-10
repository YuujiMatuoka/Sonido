using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class BrokenState<T> : State<T>
{
    private BunnyModel model;
    private BunnyView view;
    private FSM<T> fsm;

    private float prepareTime = 1f; // Tiempo para rotar y levantarse
    //private float rotationSpeedRepaired = 180f; // Velocidad de rotación en grados por segundo
    private float liftForce = 0.3f; // Fuerza para levantar el robot

    public BrokenState(BunnyModel model, BunnyView view, FSM<T> fsm)
    {
        this.model = model;
        this.view = view;
        this.fsm = fsm;
    }

    public override void Init()
    {
        view.SetBroken(true);
    }

    public override void Execute()
    {
        model.Move(Vector3.zero); // Para asegurarnos de que se detiene
        if (model.IsRepairing())
        {
            view.SetBroken(false);
            model.ChangeValueBool(true);

            model.StartCoroutine(Prepare());
        }
    }

    public override void Sleep()
    {
       
    }

    private IEnumerator Prepare()
    {

        // Rotar el robot para que esté de pie (ejemplo: rotar alrededor del eje X)
        Quaternion targetRotation = Quaternion.Euler(model.transform.eulerAngles.x, model.transform.eulerAngles.y, 0f); // Rotar solo en z para levantarse
        float rotationTimer = 0f;
        while (rotationTimer < prepareTime)
        {
            rotationTimer += Time.deltaTime;
            float rotationProgress = Mathf.Clamp01(rotationTimer / prepareTime);
            model.Rb.MoveRotation(Quaternion.Slerp(model.transform.rotation, targetRotation, rotationProgress));
            // O podrías usar AddTorque para una rotación basada en física más realista
            // Vector3 torque = Vector3.right * rotationSpeed * Time.deltaTime;
            // rb.AddTorque(torque);
            yield return null;
        }
        model.Rb.MoveRotation(targetRotation); // Asegurar la rotación final

        // Mover el robot ligeramente hacia arriba (ejemplo: aplicar una fuerza)
        model.Rb.AddForce(Vector3.up * liftForce, ForceMode.Impulse);
        model.Rb.useGravity = true;

        while (!model.IsGrounded())
        {
            yield return null; // esperar a que caiga
        }

        yield return new WaitForSeconds(0.2f); // pequeña pausa opcional para estabilidad
    }


}


