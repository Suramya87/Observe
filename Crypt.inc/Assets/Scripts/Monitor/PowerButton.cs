using UnityEngine;

public class TVPowerButton : MonoBehaviour, IInteractable
{
    public TVScreenController screen;
    [TextArea] public string prompt = "Press E to power TV";

    // make sure Prompt is never null
    public string Prompt => prompt ?? "";

    public void Interact(Transform interactor)
    {
        Debug.Log("[TVPowerButton] Interact called");

        // Failsafe: don’t toggle if grid is off
        if (PowerGridManager.Instance && !PowerGridManager.Instance.IsOn)
        {
            Debug.Log("[TVPowerButton] Ignoring click, grid is OFF");
            return;
        }

        if (!screen)
        {
            Debug.LogWarning("[TVPowerButton] No TVScreenController assigned.");
            return;
        }

        try
        {
            Debug.Log("[TVPowerButton] Calling screen.TogglePower()");
            screen.TogglePower();
        }
        catch (System.ArgumentNullException ane)
        {
            var site = ane.TargetSite;
            string where =
                site == null
                ? "(unknown)"
                : $"{site.DeclaringType?.FullName}.{site.Name}";

            Debug.LogError($"[TVPowerButton DEBUG] ArgumentNullException from: {where}\nParam: {ane.ParamName}\nMsg: {ane.Message}", screen);
            Debug.LogException(ane, screen);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[TVPowerButton DEBUG] Non-null exception in TogglePower");
            Debug.LogException(ex, screen);
        }
    }
}
