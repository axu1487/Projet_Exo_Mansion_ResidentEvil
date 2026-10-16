using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class OnInteractEvent : MonoBehaviour
{
    [Header("Filtre")]
    [SerializeField] private bool useTagFilter = true;
    [SerializeField] private string targetTag = "Player";

    [Header("Input System")]
    [Tooltip("Le composant PlayerInput (récupéré automatiquement si laissé vide).")]
    [SerializeField] private PlayerInput playerInputComponent;

    [Tooltip("Nom de l'action à rechercher dans l'Input System.")]
    [SerializeField] private string actionName = "Interact";

    [Header("Événements")]
    [Tooltip("Déclenché lors de l'appui sur la touche d'interaction quand le joueur est dans la zone.")]
    [SerializeField] private UnityEvent onInteract;

    [Tooltip("Déclenché lorsque le joueur entre dans la zone (ex: afficher le prompt 'E').")]
    [SerializeField] private UnityEvent onEnterZone;

    [Tooltip("Déclenché lorsque le joueur sort de la zone (ex: masquer le prompt 'E').")]
    [SerializeField] private UnityEvent onExitZone;

    private InputAction interactAction;
    private bool isPlayerInside = false;

    private void Start()
    {
        if (playerInputComponent == null)
            playerInputComponent = FindFirstObjectByType<PlayerInput>();

        if (playerInputComponent != null)
        {
            interactAction = playerInputComponent.actions.FindAction(actionName);
        }
    }

    private void OnDisable()
    {
        isPlayerInside = false;
    }

    private void Update()
    {
        if (isPlayerInside && interactAction != null && interactAction.triggered)
        {
            onInteract?.Invoke();
        }
    }

    // --- LOGIQUE DE VALIDATION ---

    private bool IsValid(Component target)
    {
        if (!useTagFilter || string.IsNullOrEmpty(targetTag)) return true;
        return target.CompareTag(targetTag);
    }

    private void HandleEnter()
    {
        isPlayerInside = true;
        onEnterZone?.Invoke();
    }

    private void HandleExit()
    {
        isPlayerInside = false;
        onExitZone?.Invoke();
    }

    // --- CALLBACKS 3D ---

    private void OnTriggerEnter(Collider other)
    {
        if (IsValid(other)) HandleEnter();
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsValid(other)) HandleExit();
    }

    // --- CALLBACKS 2D ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsValid(other)) HandleEnter();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsValid(other)) HandleExit();
    }
}