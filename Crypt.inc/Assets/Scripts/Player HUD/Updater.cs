using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthHUD : MonoBehaviour
{
    [Header("Refs")]
    public PlayerHealth player;    
    public Slider bar;            
    public TMP_Text label;         

    void OnEnable()
    {
        if (player)
        {
            player.OnHealthChanged += HandleChanged;
            HandleChanged(player.currentHealth, player.maxHealth);
        }
    }

    void OnDisable()
    {
        if (player) player.OnHealthChanged -= HandleChanged;
    }

    void HandleChanged(int current, int max)
    {
        if (bar) bar.value = max > 0 ? (float)current / max : 0f;
        if (label) label.text = $"{current}/{max}";
    }
}
