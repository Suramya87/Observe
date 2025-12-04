using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PowerGeneratorButton : MonoBehaviour, IInteractable
{
    [TextArea] public string prompt = "Power: Toggle Generator";
    public string Prompt => prompt;

    [Header("Visuals")]
    public Color readyColor = new Color(0.3f, 1f, 0.3f, 1f); // OFF & not cooling
    public Color coolingColor = new Color(1f, 0.2f, 0.2f, 1f); // flashing during cooldown
    public Color onColor = new Color(1f, 0.9f, 0.2f, 1f); // grid ON
    public float flashHz = 3.0f;
    public float emission = 2.0f;

    Renderer rend;
    MaterialPropertyBlock mpb;
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    // cached state from events (optional but nice)
    bool isCooling;
    bool isOn;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
    }

    void OnEnable()
    {
        var grid = PowerGridManager.Instance;
        if (grid != null)
        {
            // keep lightweight mirrors for smoother visuals
            isCooling = grid.IsCooling;
            isOn = grid.IsOn;

            grid.OnCoolingChanged += b => isCooling = b;
            // we already know On/Off via TogglePower, but if you want exact mirroring,
            // register as a consumer:
            grid.Register(new ButtonConsumer(this));
        }
    }

    void OnDisable()
    {
        var grid = PowerGridManager.Instance;
        if (grid != null)
        {
            grid.OnCoolingChanged -= b => isCooling = b;
            // not strictly necessary to unregister ButtonConsumer if you don't keep a ref.
            // If you want clean unregistering, make ButtonConsumer a field.
        }
    }

    // Tiny consumer to mirror grid On/Off (optional)
    class ButtonConsumer : IPowerConsumer
    {
        readonly PowerGeneratorButton owner;
        public ButtonConsumer(PowerGeneratorButton o) { owner = o; }
        public void OnPowerChanged(bool isOn) { owner.isOn = isOn; }
    }

    void LateUpdate()
    {
        var grid = PowerGridManager.Instance;
        if (grid == null || rend == null) return;

        // fallbacks if events were missed
        isCooling = grid.IsCooling;
        isOn = grid.IsOn;

        Color c;
        if (isCooling)
        {
            float a = 0.5f + 0.5f * Mathf.Sin(Time.time * 2f * Mathf.PI * flashHz);
            c = Color.Lerp(coolingColor * 0.25f, coolingColor, a);
        }
        else if (!isOn)
        {
            c = readyColor;
        }
        else
        {
            c = onColor;
        }

        mpb.Clear();
        mpb.SetColor(BaseColorID, c);
        mpb.SetColor(EmissionColor, c * emission);
        rend.SetPropertyBlock(mpb);
    }

    public void Interact(Transform interactor)
    {
        var grid = PowerGridManager.Instance;
        if (grid == null) return;
        grid.TogglePower(); // locked while cooling, allowed once energy refilled
    }
}
