using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEvent : MonoBehaviour
{
    [Header("Filtre")]
    [SerializeField] private bool useTagFilter = true;
    [SerializeField] private string targetTag = "Player";

    [Header("Parentage")]
    [Tooltip("Si vrai, l'objet entrant devient enfant de cet objet et perd son parent à la sortie.")]
    [SerializeField] private bool parentOnTrigger = false;

    [Header("Events")]
    [SerializeField] private UnityEvent onTriggerEnterEvent;
    [SerializeField] private UnityEvent onTriggerStayEvent;
    [SerializeField] private UnityEvent onTriggerExitEvent;

    // --- LOGIQUE COMMUNE ---

    private bool IsValid(Component target)
    {
        if (!useTagFilter || string.IsNullOrEmpty(targetTag)) return true;
        return target.CompareTag(targetTag);
    }

    private void HandleEnter(Transform targetTransform)
    {
        if (parentOnTrigger)
        {
            targetTransform.SetParent(transform);
        }
        onTriggerEnterEvent?.Invoke();
    }

    private void HandleExit(Transform targetTransform)
    {
        if (parentOnTrigger && targetTransform.parent == transform)
        {
            targetTransform.SetParent(null);
        }
        onTriggerExitEvent?.Invoke();
    }

    // --- CALLBACKS 3D ---

    private void OnTriggerEnter(Collider other)
    {
        if (IsValid(other)) HandleEnter(other.transform);
    }

    private void OnTriggerStay(Collider other)
    {
        if (IsValid(other)) onTriggerStayEvent?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsValid(other)) HandleExit(other.transform);
    }

    // --- CALLBACKS 2D ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsValid(other)) HandleEnter(other.transform);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsValid(other)) onTriggerStayEvent?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsValid(other)) HandleExit(other.transform);
    }
}