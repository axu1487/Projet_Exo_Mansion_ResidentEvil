using UnityEngine;
using UnityEngine.Events;

public class OnProgressEvent : MonoBehaviour
{
    public enum LogicMode { SimpleCount, SequenceOrder }

    [Header("Configuration")]
    [SerializeField] private LogicMode mode = LogicMode.SimpleCount;

    [Header("Settings Simple Counter")]
    [SerializeField] private int targetCount = 3;

    [Header("Settings Sequence Order")]
    [SerializeField] private int[] requiredSequence = { 1, 2, 3 };
    [Tooltip("Si vrai, attend que la longueur totale de la séquence soit atteinte avant de réinitialiser en cas d'erreur.")]
    [SerializeField] private bool resetOnlyAtEnd = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onStepValid;      // À chaque étape réussie ou à chaque clic
    [SerializeField] private UnityEvent onSuccess;        // Quand tout est validé
    [SerializeField] private UnityEvent onFailedOrReset;  // Si erreur de séquence ou retrait d'objet

    private int currentStepIndex = 0;
    private bool hasSequenceError = false;

    public void Increment()
    {
        AddStep(0);
    }

    public void Decrement()
    {
        if (mode == LogicMode.SimpleCount && currentStepIndex > 0)
        {
            currentStepIndex--;
            onFailedOrReset.Invoke();
        }
    }

    public void AddStep(int stepID)
    {
        if (mode == LogicMode.SimpleCount)
        {
            currentStepIndex++;
            onStepValid.Invoke();

            if (currentStepIndex >= targetCount)
            {
                onSuccess.Invoke();
            }
        }
        else if (mode == LogicMode.SequenceOrder)
        {
            if (requiredSequence.Length == 0) return;

            if (resetOnlyAtEnd)
            {
                // 1. Si la valeur ne correspond pas à l'étape actuelle, on marque l'erreur
                if (stepID != requiredSequence[currentStepIndex])
                {
                    hasSequenceError = true;
                }

                currentStepIndex++;
                onStepValid.Invoke();

                // 2. On attend d'avoir atteint la longueur totale de la séquence
                if (currentStepIndex >= requiredSequence.Length)
                {
                    if (!hasSequenceError)
                    {
                        onSuccess.Invoke();
                        currentStepIndex = 0;
                    }
                    else
                    {
                        // Séquence terminée avec au moins une erreur : réinitialisation complète
                        ResetProgress();
                    }
                }
            }
            else
            {
                // Ancien comportement : réinitialisation immédiate dès la première erreur
                if (stepID == requiredSequence[currentStepIndex])
                {
                    currentStepIndex++;
                    onStepValid.Invoke();

                    if (currentStepIndex >= requiredSequence.Length)
                    {
                        onSuccess.Invoke();
                        currentStepIndex = 0;
                    }
                }
                else
                {
                    ResetProgress();
                }
            }
        }
    }

    public void ResetProgress()
    {
        currentStepIndex = 0;
        hasSequenceError = false;
        onFailedOrReset.Invoke();
    }
}