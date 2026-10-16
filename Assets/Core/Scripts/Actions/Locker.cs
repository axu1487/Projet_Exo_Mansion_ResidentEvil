using UnityEngine;
using UnityEngine.Events;

public class Locker : MonoBehaviour
{
    [SerializeField] private ItemData requiredKey;
    [SerializeField] private bool consumeKeyOnUse = false;
    [SerializeField] private UnityEvent completeEvent;
    [SerializeField] private UnityEvent onFailedEvent;
    

    public void TryOpen()
    {
        if (PlayerInventory.Instance.HasItem(requiredKey))
        {
            completeEvent.Invoke();
            
            if (consumeKeyOnUse)
            {
                PlayerInventory.Instance.RemoveItem(requiredKey);
            }
        }
        else
        {
            onFailedEvent.Invoke();
        }
    }
}