using UnityEngine;

[DisallowMultipleComponent]
public class PowerBatteryDisplay : MonoBehaviour, IPowerConsumer
{
    [Header("Fill (inner column you scale)")]
    [Tooltip("Child mesh that represents the battery 'fill'. Pivot can be centered; we keep bottom anchored.")]
    public Transform fill;

    [Header("Shell (outer housing you tint)")]
    public Renderer shellRenderer;

    [Header("Colors")]
    public Color lowColor = new Color(1f, 0.2f, 0.2f, 1f);   // red
    public Color midColor = new Color(1f, 0.9f, 0.2f, 1f);   // yellow
    public Color highColor = new Color(0.2f, 1f, 0.2f, 1f);   // green
    public Color offTint = new Color(0.08f, 0.08f, 0.08f, 1f);
    [Range(0f, 4f)] public float shellEmission = 1.75f;

    [Header("Cooldown flash (optional)")]
    public bool flashOnCooldown = true;
    public float flashSpeed = 6f;               // Hz
    public float flashDim = 0.35f;            // how dark the trough gets

    // cached
    Vector3 startScale;
    Vector3 startPos;
    MaterialPropertyBlock mpb;
    bool powered;

    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int EmissionID = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        if (fill)
        {
            startScale = fill.localScale;
            startPos = fill.localPosition;
        }
        if (shellRenderer)
            mpb = new MaterialPropertyBlock();
    }

    void OnEnable()
    {
        // Subscribe to power & energy
        var grid = PowerGridManager.Instance;
        if (grid != null)
        {
            grid.OnEnergyChanged += HandleEnergy;   // ratio 0..1
            grid.Register(this);                    // push current on/off immediately
        }
    }

    void OnDisable()
    {
        var grid = PowerGridManager.Instance;
        if (grid != null)
        {
            grid.OnEnergyChanged -= HandleEnergy;
            grid.Unregister(this);
        }
    }

    // ---- IPowerConsumer ----
    public void OnPowerChanged(bool isOn)
    {
        powered = isOn;
        // force a tint refresh using last known energy (ask grid for current %)
        var grid = PowerGridManager.Instance;
        float r = 0f;
        if (grid != null)
        {
            // There isn't a public getter for raw energy; reuse last broadcast via a tiny hack:
            // call HandleEnergy with 0..1 soon after. If you want, expose a public Energy01 on the grid.
        }
        // If you add PowerGridManager public float Energy01 => (maxEnergy<=0)?0:energy/maxEnergy;
        // you can do:
        // if (grid != null) HandleEnergy(grid.Energy01);
    }

    void Update()
    {
        // purely visual cooldown flash on the shell
        var grid = PowerGridManager.Instance;
        if (!flashOnCooldown || shellRenderer == null || grid == null || !grid.IsCooling) return;

        // multiply current color by a pulsing factor
        float t = 0.5f + 0.5f * Mathf.Sin(Time.time * flashSpeed);
        float k = Mathf.Lerp(flashDim, 1f, t);

        shellRenderer.GetPropertyBlock(mpb);
        var baseC = mpb.GetColor(BaseColorID);
        var baseE = mpb.GetColor(EmissionID);
        mpb.SetColor(BaseColorID, baseC * k);
        mpb.SetColor(EmissionID, baseE * k);
        shellRenderer.SetPropertyBlock(mpb);
    }

    // ---- Energy % → visuals ----
    void HandleEnergy(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);

        // 1) Scale the fill vertically, keep bottom anchored (pivot can be centered)
        if (fill)
        {
            float newY = Mathf.Max(0.0001f, startScale.y * ratio);
            var s = fill.localScale; s.y = newY; fill.localScale = s;

            // original bottom = startPos.y - 0.5*startScale.y
            // new center     = bottom + 0.5*newY
            var p = fill.localPosition;
            p.y = startPos.y - 0.5f * startScale.y + 0.5f * newY;
            fill.localPosition = p;
        }

        // 2) Tint shell by energy and power state
        if (shellRenderer)
        {
            // red -> yellow -> green
            Color byEnergy = (ratio < 0.5f)
                ? Color.Lerp(lowColor, midColor, ratio * 2f)
                : Color.Lerp(midColor, highColor, (ratio - 0.5f) * 2f);

            var final = powered ? byEnergy : offTint;

            mpb.Clear();
            mpb.SetColor(BaseColorID, final);
            mpb.SetColor(EmissionID, final * shellEmission);
            shellRenderer.SetPropertyBlock(mpb);
        }
    }
}
