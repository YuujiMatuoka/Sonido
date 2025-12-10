using System.Collections;
using UnityEngine;

public class TimeTravelPanel : TimeTravelObject, IInteractable
{
    [Header("Panel Settings")]
    [SerializeField] private Transform playerTargetPoint;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float timeTravel = 3.5f;
    [SerializeField] private AnimationCurve movementCurve;
    [SerializeField] private AudioSource mySound;


    private bool isActivating = false;

    public void Interact()
    {
        if (!isActivating)
        {
            StartCoroutine(ActivateTimeTravelSequence());
            GameManager.Instance.PlayerManager.GetComponent<Rigidbody>().useGravity = true;
            isActivating = false;
        }
    }

    private IEnumerator ActivateTimeTravelSequence()
    {
        isActivating = true;
        var playerTransform = GameManager.Instance.PlayerManager.transform;
        GameManager.Instance.PlayerManager.GetComponent<Rigidbody>().useGravity = false;

        Vector3 startPos = playerTransform.position;
        Quaternion startRot = playerTransform.rotation;

        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curveValue = movementCurve.Evaluate(t);

            playerTransform.position = Vector3.Lerp(
                startPos,
                playerTargetPoint.position,
                curveValue
            );

            playerTransform.rotation = Quaternion.Slerp(
                startRot,
                playerTargetPoint.rotation,
                curveValue
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerTransform.position = playerTargetPoint.position;
        playerTransform.rotation = playerTargetPoint.rotation;

        yield return new WaitForSeconds(0.5f);
        UIManager.Instance.OnTimeTravelEffect();
        mySound.Play();
        GameManager.Instance.SetGameState(GameState.TimeTravel);
        yield return new WaitForSeconds(timeTravel);
    }

    public override void OnTimeChanged(TimeState newTimeState)
    {
        base.OnTimeChanged(newTimeState);
    }
}


