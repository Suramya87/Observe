using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PowerIndicator : MonoBehaviour, IPowerConsumer
{
    // ---------- Global visual events ----------
    public static System.Action<float> OnWarmupPulse;  // called with t in [0,1]
    public static System.Action<bool> OnVisualSnap;   // instant on/off

    [Header("Colors")]
    public Color onColor = new Color(1f, 0.85f, 0.2f, 1f);
    public Color offColor = new Color(0.08f, 0.08f, 0.08f, 1f);
    public float emission = 2.0f;

    Renderer rend;
    MaterialPropertyBlock mpb;
    bool powered;

    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int LegacyColorID = Shader.PropertyToID("_Color");
    static readonly int EmissionID = Shader.PropertyToID("_EmissionColor");

    [Header("Optional energy bar")]
    public Transform fillBar;           // child mesh to scale on X (pivot on left)
    public bool dimWithEnergy = false;  // also dim color by energy ratio

    void HandleEnergyChanged(float ratio)
    {
        // Left→right fill: scale X and keep the left edge anchored (set pivot of mesh to left).
        if (fillBar)
        {
            var s = fillBar.localScale;
            s.x = Mathf.Max(0f, ratio);
            fillBar.localScale = s;
        }

        if (dimWithEnergy)
        {
            // Optional: dim overall brightness by ratio
            var baseC = powered ? onColor : offColor;
            var c = Color.Lerp(offColor, baseC, Mathf.Clamp01(ratio));
            Apply(c);
        }
    }

    void OnEnable()
    {
        rend = GetComponent<Renderer>();
        if (mpb == null) mpb = new MaterialPropertyBlock();

        PowerGridManager.Instance?.Register(this);
        OnWarmupPulse += HandleWarmupPulse;
        OnVisualSnap += HandleSnap;
    }

    void OnDisable()
    {
        PowerGridManager.Instance?.Unregister(this);
        OnWarmupPulse -= HandleWarmupPulse;
        OnVisualSnap -= HandleSnap;
    }

    public void OnPowerChanged(bool isOn)
    {
        powered = isOn;
        Apply(isOn ? onColor : offColor);
    }

    void HandleWarmupPulse(float t)
    {
        if (powered) return;
        t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
        Apply(Color.Lerp(offColor, onColor, t * 0.9f));
    }

    void HandleSnap(bool isOn) => Apply(isOn ? onColor : offColor);

    void Apply(Color c)
    {
        if (!rend)
        {
            // Try to recover if renderer got removed at runtime.
            rend = GetComponent<Renderer>();
            if (!rend) return;
        }
        if (mpb == null) mpb = new MaterialPropertyBlock();

        mpb.Clear();
        // These ids are valid even if the material lacks the property – Unity just ignores it.
        mpb.SetColor(BaseColorID, c);
        mpb.SetColor(LegacyColorID, c);
        mpb.SetColor(EmissionID, c * emission);

        rend.SetPropertyBlock(mpb);
    }

    public static void BroadcastWarmup(float t) => OnWarmupPulse?.Invoke(t);
    public static void SnapAll(bool isOn) => OnVisualSnap?.Invoke(isOn);
}
