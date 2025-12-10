using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisualController: MonoBehaviour
{
    private PlayerModel model;
    private Animator animator;
    private Camera playerCamera; // Para efectos visuales
    private GameObject heldVisual;

    public PlayerVisualController(PlayerModel model, Animator animator, Camera camera)
    {
        this.model = model;
        this.animator = animator;
        this.playerCamera = camera;
    }
    
    /// <summary>
    /// Actualiza las animaciones según velocidad, estado de suelo y si tiene objeto.
    /// </summary>
    public void UpdateVisuals()
    {
        // Velocidad horizontal (ignora Y)
        Vector3 horizontalVelocity = new Vector3(model.currentVelocity.x, 0f, model.currentVelocity.z);
        float speed = horizontalVelocity.magnitude;

        // Actualiza parámetros del Animator
        animator.SetFloat("Velocity", speed);
        animator.SetBool("IsRunning", model.isRunning);
        animator.SetBool("IsGrounded", model.isGrounded);
        animator.SetBool("IsHoldingObject", model.isHoldingObject);

        // Maneja transiciones de estado
        //HandleAnimationTransitions(speed);
    }

    //private void HandleAnimationTransitions(float speed)
    //{
    //    // Aire → salto
    //    if (!model.isGrounded)
    //    {
    //        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
    //            animator.Play("Jump");
    //        return;
    //    }

    //    // Con objeto en la mano
    //    if (model.isHoldingObject)
    //    {
    //        model.Canvas_Animator.SetTrigger("PickItem");
    //        if (speed > 0.1f)
    //        {
    //            if (model.isRunning && speed > model.moveSpeed * 0.8f)
    //            {
    //                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("RunWithObject"))
    //                    animator.Play("RunWithObject");
    //            }
    //            else
    //            {
    //                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("WalkWithObject"))
    //                    animator.Play("WalkWithObject");
    //            }
    //        }
    //        else
    //        {
    //            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("IdleWithObject"))
    //                animator.Play("IdleWithObject");
    //        }
    //    }
    //    // Sin objeto
    //    else
    //    {
    //        if (speed > 0.1f)
    //        {
    //            if (model.isRunning && speed > model.moveSpeed * 0.8f)
    //            {
    //                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Running"))
    //                    animator.Play("Running");
    //            }
    //            else
    //            {
    //                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
    //                    animator.Play("Walking");
    //            }
    //        }
    //        else
    //        {
    //            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
    //                animator.Play("Idle");
    //        }
    //    }

    //}

    /// <summary>
    /// Reproduce animación de agarrar objeto
    /// </summary>
    public void PlayGrabAnimation()
    {
        animator.SetTrigger("Grab");
    }

    /// <summary>
    /// Reproduce animación de soltar objeto
    /// </summary>
    public void PlayReleaseAnimation()
    {
        animator.SetTrigger("Release");
    }

    /// <summary>
    /// Limpia el objeto que el jugador sostiene visualmente
    /// </summary>
    public void ClearHeldObject()
    {
        if (heldVisual != null)
        {
            heldVisual.transform.SetParent(null);
            heldVisual = null;
        }
    }

    /// <summary>
    /// Asigna un objeto que el jugador está sosteniendo
    /// </summary>
    public void SetHeldObject(GameObject obj)
    {
        heldVisual = obj;
        heldVisual.transform.SetParent(model.characterHand);
        heldVisual.transform.localPosition = Vector3.zero;
        heldVisual.transform.localRotation = Quaternion.identity;
    }
}
