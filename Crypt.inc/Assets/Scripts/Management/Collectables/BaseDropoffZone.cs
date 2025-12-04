using System.Collections.Generic;
using UnityEngine;

public class BaseDropoffZone : MonoBehaviour
{
    [Header("Stored Upgrades")]
    public List<UpgradeItem> itemsAtBase = new List<UpgradeItem>();

    public void RegisterItem(UpgradeItem item)
    {
        if (item == null) return;
        if (!itemsAtBase.Contains(item))
            itemsAtBase.Add(item);

        Debug.Log($"Item delivered to base: {item.ItemId}");
    }

    // For future upgrade unlock checks:
    public bool HasItemInBase(string itemId)
    {
        return itemsAtBase.Exists(i => i != null &&
                                       i.ItemId == itemId &&
                                       i.State == UpgradeItemState.InBase);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var inventory = other.GetComponent<PlayerUpgradeInventory>();
        if (inventory == null) return;

        // For now: auto-deposit all items when player steps into the zone
        inventory.DepositAll(this);
    }
}
