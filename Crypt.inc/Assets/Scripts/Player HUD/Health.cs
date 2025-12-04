using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action OnDied;

    void Awake()
    {
        currentHealth = Mathf.Clamp(currentHealth <= 0 ? maxHealth : currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;
        currentHealth = Mathf.Clamp(currentHealth + Mathf.Max(0, amount), 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        var gs = GameSettings.Instance;

        if (gs && gs.infiniteHealth)
        {
            return;
        }

        float mult = gs ? gs.damageMultiplier : 1f;
        int final = Mathf.Max(0, Mathf.RoundToInt(amount * Mathf.Max(0f, mult)));

        if (final <= 0) return;

        currentHealth = Mathf.Clamp(currentHealth - final, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0) OnDied?.Invoke();

    }
}
