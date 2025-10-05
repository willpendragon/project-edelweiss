using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentInventoryManager : MonoBehaviour
{
    public static Inventory CurrentInventory { get; private set; }

    [SerializeField] private Inventory inventoryAsset; // drag the SO from Assets here

    void Awake()
    {
        if (CurrentInventory == null)
        {
            CurrentInventory = Instantiate(inventoryAsset);
            DontDestroyOnLoad(this.gameObject); // persist this GameObject
        }
        else
        {
            Destroy(gameObject); // prevent duplicates
        }
    }
}
