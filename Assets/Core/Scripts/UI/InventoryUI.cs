using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Structure UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject inventoryPanel; 
    [SerializeField] private InventorySlotUI[] slots;

    [Header("Références")]
    [SerializeField] private PlayerController playerController;

    private InputSystem_Actions controls;
    private bool isOpen = false;

    private void Awake()
    {
        controls = new InputSystem_Actions();

        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Start()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryChanged += RefreshUI; //
        }

        SetInventoryState(false); //[cite: 1]
    }

    private void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryChanged -= RefreshUI; //[cite: 1]
        }
    }

    private void Update()
    {
        // Détection via l'action "Inventory" de l'Input System
        if (controls.Player.Inventory.WasPressedThisFrame() || (isOpen && controls.UI.Cancel.WasPressedThisFrame()))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        SetInventoryState(!isOpen);
    }

    private void SetInventoryState(bool open)
    {
        isOpen = open;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = open ? 1f : 0f; //[cite: 1]
            canvasGroup.interactable = open;    //[cite: 1]
            canvasGroup.blocksRaycasts = open;  //[cite: 1]
        }

        Time.timeScale = open ? 0f : 1f;
        // Bascule les contrôles du joueur (gel des déplacements / activation de l'UI)
        if (playerController != null)
        {
            playerController.ToggleInventory(open);
        }

        if (open) RefreshUI(); //[cite: 1]
    }

    private void RefreshUI()
    {
        if (PlayerInventory.Instance == null) return; //[cite: 1]

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < PlayerInventory.Instance.items.Count) //[cite: 1]
            {
                slots[i].SetItem(PlayerInventory.Instance.items[i]); //[cite: 1]
            }
            else
            {
                slots[i].ClearSlot(); //[cite: 1]
            }
        }
    }
}