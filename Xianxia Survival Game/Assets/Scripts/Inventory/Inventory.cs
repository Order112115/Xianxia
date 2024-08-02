using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    #region Singleton
    public static Inventory instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of Inventory found!");
            return;
        }
        instance = this;
    }
    #endregion

    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public List<Item> items = new List<Item>();
    public int inventorySize = 20;

    public bool AddItem(Item item)
    {
        if (items.Count >= inventorySize)
        {
            Debug.Log("Inventory full!");
            return false;
        }
        items.Add(item);

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();

        Debug.Log(item.itemName + " added to inventory.");
        return true;
    }

    public void RemoveItem(Item item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);

            if (onItemChangedCallback != null)
                onItemChangedCallback.Invoke();

            Debug.Log(item.itemName + " removed from inventory.");
        }
        else
        {
            Debug.Log(item.itemName + " not found in inventory.");
        }
    }
}