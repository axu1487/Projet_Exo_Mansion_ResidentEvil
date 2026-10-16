using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public class OnOnceEvent : MonoBehaviour
{
    [Header("Événement")]
    [SerializeField] private UnityEvent onFirstTriggerOnly;

    [Header("Réinitialisation Automatique")]
    [SerializeField] private bool autoReset = false;

    [SerializeField, ShowIf("autoReset"), Tooltip("Délai en secondes avant de réinitialiser l'événement")]
    private float resetDelay = 5f;

    private bool hasBeenTriggered = false;
    private string eventKey;

    public void Trigger()
    {
        if (hasBeenTriggered) return;

        hasBeenTriggered = true;
        onFirstTriggerOnly?.Invoke();

        if (autoReset)
        {
            Invoke(nameof(ResetTrigger), resetDelay);
        }
    }

    public void ResetTrigger()
    {
        CancelInvoke(nameof(ResetTrigger));
        hasBeenTriggered = false;
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(ResetTrigger));
    }
}