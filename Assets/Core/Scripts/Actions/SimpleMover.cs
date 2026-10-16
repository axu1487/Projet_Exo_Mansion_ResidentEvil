using System.Collections;
using UnityEngine;

public class SimpleMover : MonoBehaviour
{
    [Header("Position")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0, 2f, 0);

    [Header("Rotation")]
    [SerializeField] private Vector3 targetRotationOffset = new Vector3(0, 90f, 0);

    [Header("Animation")]
    [SerializeField] private float duration = 1f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool isOpen = false;
    private Coroutine moveCoroutine;

    private void Awake()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    public void ToggleMove()
    {
        isOpen = !isOpen;

        Vector3 targetPos = isOpen ? startPosition + targetOffset : startPosition;
        Quaternion targetRot = isOpen ? startRotation * Quaternion.Euler(targetRotationOffset) : startRotation;

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(AnimateMove(targetPos, targetRot));
    }

    private IEnumerator AnimateMove(Vector3 destinationPos, Quaternion destinationRot)
    {
        Vector3 initialPos = transform.localPosition;
        Quaternion initialRot = transform.localRotation;
        float time = 0;

        while (time < duration)
        {
            float t = Mathf.SmoothStep(0, 1, time / duration);

            transform.localPosition = Vector3.Lerp(initialPos, destinationPos, t);
            transform.localRotation = Quaternion.Slerp(initialRot, destinationRot, t);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = destinationPos;
        transform.localRotation = destinationRot;
    }

    public void ResetToStart()
    {
        if (!isOpen) return;

        isOpen = false;

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(AnimateMove(startPosition, startRotation));
    }
}