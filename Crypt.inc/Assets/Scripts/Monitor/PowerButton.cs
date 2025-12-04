using UnityEngine;

public class TVPowerButton : MonoBehaviour, IInteractable
{
    public TVScreenController screen;
    [TextArea] public string prompt = "Press E to power TV";
    public string Prompt => prompt;

    public void Interact(Transform interactor)
    {
        // Failsafe: don’t toggle if grid is off
        if (PowerGridManager.Instance && !PowerGridManager.Instance.IsOn)
        {
            Debug.Log("[TVPowerButton] Ignoring click, grid is OFF");
            return;
        }

        if (screen) screen.TogglePower();
    }


}
