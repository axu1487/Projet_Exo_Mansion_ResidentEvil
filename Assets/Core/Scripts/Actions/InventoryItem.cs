using UnityEngine;

public class InventoryItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;

    public void Collect()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.AddItem(itemData);
            Destroy(gameObject);
        }
    }
}