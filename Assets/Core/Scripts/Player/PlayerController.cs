using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [Header("Paramètres de Déplacement")]
    public float speed = 4f;
    public float backwardsSpeedMultiplier = 0.4f;
    public float rotationSpeed = 150f;
    public float turnSmoothTime = 0.1f;

    [Header("Références")]
    public Transform cam;

    [Header("États")]
    public bool isAiming;
    public bool canMove = true;

    // Instance unique du système d'inputs
    private InputSystem_Actions controls;
    private Animator animator;
    private NavMeshAgent agent;
    private float turnSmoothVelocity;

    // Repère verrouillé de la caméra pour éviter les sauts de direction
    private Vector3 cachedCamForward;
    private Vector3 cachedCamRight;
    private bool isMovingWithStick = false;

    private void Awake()
    {
        controls = new InputSystem_Actions();
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (cam == null && Camera.main != null)
        {
            cam = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Start()
    {
        LockCameraOrientation();
    }

    private void Update()
    {
        if (!canMove) return;

        // Lecture directe depuis l'Action Map "Player"
        Vector2 stickInput = controls.Player.Move.ReadValue<Vector2>();
        Vector2 dpadInput = controls.Player.DPad.ReadValue<Vector2>();
        isAiming = controls.Player.Aim.IsPressed();

        if (isAiming)
        {
            isMovingWithStick = false;
            HandleAimingLogic(stickInput, dpadInput);
            return;
        }

        if (animator != null) animator.SetBool("Aim", false);

        // 1. DÉPLACEMENT TANK (D-Pad)
        if (dpadInput.sqrMagnitude > 0.01f)
        {
            isMovingWithStick = false;
            HandleTankMovement(dpadInput);
        }
        // 2. DÉPLACEMENT MODERNE (Stick Analogique)
        else if (stickInput.sqrMagnitude > 0.05f)
        {
            HandleModernMovement(stickInput);
        }
        // 3. IMMOBILE
        else
        {
            isMovingWithStick = false;
            LockCameraOrientation(); // Re-capture la caméra actuelle dès qu'on s'arrête

            if (animator != null)
            {
                animator.SetFloat("SpeedZ", 0f);
                animator.SetFloat("Rotate", 0f);
            }
        }
    }

    private void HandleAimingLogic(Vector2 stickInput, Vector2 dpadInput)
    {
        if (animator != null)
        {
            animator.SetBool("Aim", true);
            animator.SetFloat("SpeedZ", 0f);
        }

        float turnInput = Mathf.Abs(stickInput.x) > 0.1f ? stickInput.x : dpadInput.x;
        transform.Rotate(0f, turnInput * rotationSpeed * Time.deltaTime, 0f);

        if (animator != null) animator.SetFloat("Rotate", turnInput);
    }

    private void HandleTankMovement(Vector2 dpad)
    {
        transform.Rotate(0f, dpad.x * rotationSpeed * Time.deltaTime, 0f);

        float currentSpeed = (dpad.y >= 0) ? speed : (speed * backwardsSpeedMultiplier);
        Vector3 displacement = transform.forward * (dpad.y * currentSpeed * Time.deltaTime);

        MoveCharacter(displacement);

        if (animator != null)
        {
            animator.SetFloat("Rotate", dpad.x);
            animator.SetFloat("SpeedZ", dpad.y);
        }
    }

    private void HandleModernMovement(Vector2 stick)
    {
        // Si on vient de commencer à pousser le stick, on verrouille la caméra active
        if (!isMovingWithStick)
        {
            LockCameraOrientation();
            isMovingWithStick = true;
        }

        // On utilise les directions verrouillées (cached) au lieu de réinterroger la caméra en continu
        Vector3 targetDirection = (cachedCamRight * stick.x + cachedCamForward * stick.y).normalized;

        if (targetDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 displacement = targetDirection * (speed * stick.magnitude * Time.deltaTime);
            MoveCharacter(displacement);

            if (animator != null)
            {
                animator.SetFloat("SpeedZ", stick.magnitude);
                animator.SetFloat("Rotate", 0f);
            }
        }
    }

    private void LockCameraOrientation()
    {
        if (cam == null)
        {
            if (Camera.main != null) cam = Camera.main.transform;
            else return;
        }

        // Capture les axes horizontal/vertical de la caméra courante projetés au sol (Y = 0)
        cachedCamForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        cachedCamRight = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;
    }

    private void MoveCharacter(Vector3 displacement)
    {
        if (agent != null && agent.enabled)
        {
            agent.Move(displacement);
        }
        else
        {
            transform.position += displacement;
        }
    }

    public void ToggleInventory(bool open)
    {
        if (open)
        {
            controls.Player.Disable();
            controls.UI.Enable();
            canMove = false;
        }
        else
        {
            controls.UI.Disable();
            controls.Player.Enable();
            canMove = true;
        }
    }
}