using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TVScreenController : MonoBehaviour
{
    [Header("Screen")]
    public Renderer screenRenderer;
    public RenderTexture screenRT;
    public Color offColor = Color.black;
    public bool startsOn = true;

    [Header("Inputs")]
    public Camera[] sources;
    public int startSourceIndex = 0;
    public string[] sourceNames; // optional: if empty, we'll use camera gameObject names

    bool isOn;
    int activeIndex = -1;
    MaterialPropertyBlock mpb;

    static readonly int BaseMapID = Shader.PropertyToID("_BaseMap");
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

    // --- NEW: public accessors for UI/buttons ---
    public bool IsOn => isOn;
    public int ActiveIndex => activeIndex;
    public int SourceCount => sources?.Length ?? 0;

    void Awake()
    {
        if (!screenRenderer) screenRenderer = GetComponent<Renderer>();
        mpb ??= new MaterialPropertyBlock();

        if (sources != null)
            foreach (var cam in sources)
                if (cam) cam.targetTexture = screenRT;

        SelectSource(Mathf.Clamp(startSourceIndex, 0, (sources?.Length ?? 1) - 1));
        SetPower(startsOn);
    }

    public void TogglePower() => SetPower(!isOn);

    void ClearRenderTexture()
    {
        if (!screenRT) return;
        var prev = RenderTexture.active;
        RenderTexture.active = screenRT;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = prev;
    }

    public void SetPower(bool on)
    {
        isOn = on;

        if (sources != null)
            for (int i = 0; i < sources.Length; i++)
                if (sources[i]) sources[i].enabled = isOn && (i == activeIndex);

        if (!isOn)
        {
            if (sources != null)
                foreach (var cam in sources)
                    if (cam) cam.targetTexture = null;

            ClearRenderTexture();

            if (sources != null)
                foreach (var cam in sources)
                    if (cam) cam.targetTexture = screenRT;
        }

        ApplyScreen();
    }

    public void SelectSource(int index)
    {
        if (sources == null || sources.Length == 0) return;

        activeIndex = Mathf.Clamp(index, 0, sources.Length - 1);

        for (int i = 0; i < sources.Length; i++)
        {
            var cam = sources[i];
            if (!cam) continue;
            cam.targetTexture = screenRT;
            cam.enabled = isOn && (i == activeIndex);
        }

        ApplyScreen();
    }

    // --- NEW: cycle helper (positive or negative step) ---
    public void NextSource(int step = 1)
    {
        if (sources == null || sources.Length == 0) return;
        int n = sources.Length;
        int next = ((activeIndex + step) % n + n) % n; // wrap even for negative step
        SelectSource(next);
    }

    public string CurrentSourceLabel()
    {
        if (SourceCount == 0) return "No Feed";
        if (sourceNames != null && ActiveIndex < sourceNames.Length && !string.IsNullOrEmpty(sourceNames[ActiveIndex]))
            return sourceNames[ActiveIndex];
        var cam = sources[ActiveIndex];
        return cam ? cam.gameObject.name : $"Cam {ActiveIndex + 1}";
    }

    void ApplyScreen()
    {
        if (!screenRenderer) return;

        mpb.Clear();

        if (isOn && screenRT)
        {
            mpb.SetTexture(BaseMapID, screenRT);
            mpb.SetColor(BaseColorID, Color.white);
        }
        else
        {
            mpb.SetTexture(BaseMapID, null);
            mpb.SetColor(BaseColorID, offColor);
        }

        screenRenderer.SetPropertyBlock(mpb);
    }
}
