using UnityEngine;

public enum UpgradeItemState
{
    InWorld,
    InInventory,
    InBase
}

public class UpgradeItem : MonoBehaviour, IInteractable
{
    [Header("ID / State")]
    [SerializeField] string itemId = "Relic_A";   
    [SerializeField] UpgradeItemState state = UpgradeItemState.InWorld;

    [Header("Visuals")]
    public GameObject worldModel;

    public string Prompt => $"Pick up {itemId}";
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

    public void Interact(Transform interactor)
    {
        if (state != UpgradeItemState.InWorld) return;

        var inventory =
            interactor.GetComponent<PlayerUpgradeInventory>() ??
            interactor.GetComponentInParent<PlayerUpgradeInventory>();

        if (inventory == null) return;

        if (inventory.TryPickup(this))
        {
            SetState(UpgradeItemState.InInventory);
        }
    }
}
