using UnityEngine;

public enum UpgradeItemState
{
    InWorld,      // sitting out on the terrain
    InInventory,  // currently carried by the player
    InBase        // delivered to base
}

public class UpgradeItem : MonoBehaviour, IInteractable
{
    [Header("ID / State")]
    [SerializeField] string itemId = "Relic_A";   // set per prefab
    [SerializeField] UpgradeItemState state = UpgradeItemState.InWorld;

    [Header("Visuals (optional)")]
    [Tooltip("World model to show when the item is in the world.")]
    public GameObject worldModel;   // assign the mesh/visual here

    // --- IInteractable ---
    public string Prompt => $"Pick up {itemId}";
    // ---------------------

    public string ItemId => itemId;
    public UpgradeItemState State => state;

    void Awake()
    {
        ApplyVisualState();
    }

    public void SetState(UpgradeItemState newState)
    {
        if (state == newState) return;
        state = newState;
        ApplyVisualState();
    }

    void ApplyVisualState()
    {
        if (worldModel)
            worldModel.SetActive(state == UpgradeItemState.InWorld);
    }

    // Called by PlayerControllerCC.Interact() when you left-click this object
    public void Interact(Transform interactor)
    {
        if (state != UpgradeItemState.InWorld) return;

        var inventory =
            interactor.GetComponent<PlayerUpgradeInventory>() ??
            interactor.GetComponentInParent<PlayerUpgradeInventory>();

        if (inventory == null)
        {
            Debug.LogWarning("[UpgradeItem] Interactor has no PlayerUpgradeInventory.");
            return;
        }

        if (inventory.TryPickup(this))
        {
            SetState(UpgradeItemState.InInventory);
            Debug.Log($"[UpgradeItem] Picked up {itemId}, now InInventory.");
        }
    }
}
