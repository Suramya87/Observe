using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsUI : MonoBehaviour
{
    [Header("Widgets")]
    public Toggle infiniteHealthToggle;
    public Slider hardModeSlider;      
    public TMP_Text hardModeLabel;         

    void OnEnable()
    {
        var gs = GameSettings.Instance;
        if (!gs) return;

        if (infiniteHealthToggle) infiniteHealthToggle.isOn = gs.infiniteHealth;

        if (hardModeSlider)
        {
            hardModeSlider.minValue = 1f;
            hardModeSlider.maxValue = 3f;
            hardModeSlider.wholeNumbers = false;
            hardModeSlider.value = gs.damageMultiplier;
        }

        UpdateLabel(gs.damageMultiplier);

        if (infiniteHealthToggle) infiniteHealthToggle.onValueChanged.AddListener(OnInfiniteChanged);
        if (hardModeSlider) hardModeSlider.onValueChanged.AddListener(OnHardChanged);
    }

    void OnDisable()
    {
        if (infiniteHealthToggle) infiniteHealthToggle.onValueChanged.RemoveListener(OnInfiniteChanged);
        if (hardModeSlider) hardModeSlider.onValueChanged.RemoveListener(OnHardChanged);
    }

    void OnInfiniteChanged(bool v) => GameSettings.Instance?.SetInfiniteHealth(v);

    void OnHardChanged(float v)
    {
        GameSettings.Instance?.SetDamageMultiplier(v);
        UpdateLabel(v);
    }

    void UpdateLabel(float mult)
    {
        if (hardModeLabel) hardModeLabel.text = $"Damage x{mult:0.0}";
    }
}
