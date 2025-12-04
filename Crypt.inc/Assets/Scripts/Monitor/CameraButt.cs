using UnityEngine;

public class TVCycleButton : MonoBehaviour, IInteractable
{
    public TVScreenController tv;
    [TextArea] public string promptPrefix = "Cycle Feed";
    public bool requirePower = true; // if true, ignores clicks when TV is off

    public string Prompt
    {
        get
        {
            if (!tv) return promptPrefix;
            var label = tv.CurrentSourceLabel();
            return $"{promptPrefix} ({label})";
        }
    }

    public void Interact(Transform interactor)
    {
        if (!tv) return;
        if (requirePower && !tv.IsOn) return;

        tv.NextSource(1);
        // Optional: Debug
        // Debug.Log($"TV feed -> {tv.ActiveIndex}: {tv.CurrentSourceLabel()}");
    }
}
