using UnityEngine;

public class ShieldSwitch : MonoBehaviour, IInteractable
{
    [TextArea] public string prompt = "Press E to toggle shield";
    public DoorShield shield;    
    public string Prompt => prompt;

    public void Interact(Transform interactor)
    {
        if (shield) shield.Toggle();
    }
}
