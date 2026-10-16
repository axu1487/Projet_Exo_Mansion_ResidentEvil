using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }
    public List<ItemData> items = new List<ItemData>();
    
    // Événement prévenant l'UI qu'il y a du changement
    public event Action OnInventoryChanged;

    private void Awake()
    {
        // Sécurité : détruit le doublon s'il en existe déjà un
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddItem(ItemData item)
    {
        items.Add(item);
        Debug.Log($"Objet ramassé : {item.itemName}");
        OnInventoryChanged?.Invoke(); // Déclenche l'événement
    }

    public bool HasItem(ItemData item)
    {
        return items.Contains(item);
    }

    public bool RemoveItem(ItemData item)
    {
        bool removed = items.Remove(item);
        if (removed)
        {
            OnInventoryChanged?.Invoke(); // Déclenche l'événement
        }
        return removed;
    }
}