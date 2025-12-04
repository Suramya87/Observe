using UnityEngine;

public class SpawnerButton : MonoBehaviour, IInteractable
{
    public OrbPoolSpawner spawner;
    public string Prompt => "Click to spawn";

    public void Interact(Transform interactor)
    {
        spawner?.Interact(interactor);
    }
}
