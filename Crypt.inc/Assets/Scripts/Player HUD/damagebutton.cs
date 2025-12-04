using UnityEngine;

public class DamagePlayerButton : MonoBehaviour, IInteractable
{
    [TextArea] public string prompt = "Click to take damage";
    public string Prompt => prompt;

    public int damageAmount = 10;
    public float cooldown = 0.2f;
    float nextUseTime;

    public void Interact(Transform interactor)
    {
        if (Time.time < nextUseTime) return;

        var ph = interactor.GetComponentInParent<PlayerHealth>();
        if (!ph) ph = FindObjectOfType<PlayerHealth>();

        if (ph)
        {
            Debug.Log($"[DamageButton] Clicked by '{interactor.name}'. Applying {damageAmount} dmg to '{ph.name}'.");
            ph.TakeDamage(damageAmount);
        }
        else
        {
            Debug.LogWarning("[DamageButton] No PlayerHealth found.");
        }

        nextUseTime = Time.time + cooldown;
    }
}
