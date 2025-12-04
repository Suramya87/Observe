using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgradeInventory : MonoBehaviour
{
    [Header("Debug")]
    public List<UpgradeItem> carriedItems = new List<UpgradeItem>();

    public bool TryPickup(UpgradeItem item)
    {
        if (item == null) return false;
        if (carriedItems.Contains(item)) return false;

        carriedItems.Add(item);
        // (Optional) parent to player if you want:
        // item.transform.SetParent(transform);

        Debug.Log($"Picked up upgrade item: {item.ItemId}");
        return true;
    }

    public void DepositAll(BaseDropoffZone dropoff)
    {
        if (dropoff == null) return;

        foreach (var item in carriedItems)
        {
            if (item == null) continue;

            item.SetState(UpgradeItemState.InBase);
            dropoff.RegisterItem(item);
        }

        carriedItems.Clear();
    }
}
