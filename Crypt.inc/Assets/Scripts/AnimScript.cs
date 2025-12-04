using UnityEngine;

public class IntroSequenceEvents : MonoBehaviour
{
    // Optional, if you want to literally call the same Interact the player uses:
    public PowerGeneratorButton generatorButton;

    // Called by Animation Event
    public void TurnOnGenerator()
    {
        if (generatorButton != null)
        {
            // reuse your existing interaction logic
            generatorButton.Interact(null);   // interactor not needed here
        }
        else
        {
            // fallback: just flip the global power directly
            PowerGridManager.Instance?.TurnOn();
        }
    }
}
