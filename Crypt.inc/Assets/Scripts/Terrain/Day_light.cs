using UnityEngine;
using UnityEngine.Events;

public class DayNightCycle : MonoBehaviour
{
    // -------------------------
    //       TIME SETTINGS
    // -------------------------
    [Header("Time Settings")]
    [Range(0f, 24f)] public float timeOfDay = 12f;
    public float defaultCycleLength = 300f;

    [Header("Starting Time")]
    [Range(0f, 24f)] public float startTimeOfDay = 8f; // <-- You can type starting time here

    [Header("Control")]
    public bool cycleEnabled = true;        // freeze/unfreeze time
    public bool realTimeSetAllowed = true;  // allows animator/UI to change time manually

    // -------------------------
    //     LENGTH MULTIPLIERS
    // -------------------------
    [Header("Length Multipliers")]
    public float dayLengthMultiplier = 1f;
    public float nightLengthMultiplier = 1f;

    // -------------------------
    //        REFERENCES
    // -------------------------
    [Header("References")]
    public Light directionalLight;
    public Material skyboxMaterial;

    // -------------------------
    //      VISUAL SETTINGS
    // -------------------------
    [Header("Lighting Settings")]
    public Gradient lightColor;
    public AnimationCurve lightIntensity;
    public AnimationCurve skyboxExposure;

    // -------------------------
    //       FOG SETTINGS
    // -------------------------
    [Header("Fog Settings")]
    public Color dayFogColor = Color.gray;
    public Color nightFogColor = Color.black;
    public float dayFogDensity = 0.01f;
    public float nightFogDensity = 0.8f;

    // -------------------------
    //         EVENTS
    // -------------------------
    [Header("Events")]
    public UnityEvent onSunrise;
    public UnityEvent onSunset;
    public UnityEvent onNewDay;

    // Internal state
    private bool sunriseTriggered = false;
    private bool sunsetTriggered = false;

    private float currentExposure = 1f;
    private float currentSunIntensity = 1f;
    private Color currentSunColor = Color.white;

    public int currentDay = 1;
    public bool isNight = false;

    // -------------------------
    //           START
    // -------------------------
    void Start()
    {
        // Set starting time typed in inspector
        timeOfDay = Mathf.Clamp(startTimeOfDay, 0f, 24f);

        // Update scene immediately to reflect starting time
        UpdateLighting();
    }

    // -------------------------
    //          UPDATE
    // -------------------------
    void Update()
    {
        if (cycleEnabled)
            AdvanceTime();

        UpdateLighting();
        HandleEvents();
    }

    // -------------------------
    //     TIME CONTROL API
    // -------------------------

    public void SetTime(float newTime)
    {
        if (!realTimeSetAllowed) return;

        timeOfDay = Mathf.Clamp(newTime, 0f, 24f);
        UpdateLighting();
    }

    public void FreezeTime(bool freeze)
    {
        cycleEnabled = !freeze;
    }

    // -------------------------
    //   INTERNAL TIME UPDATE
    // -------------------------
    private void AdvanceTime()
    {
        float halfCycle = defaultCycleLength / 2f;

        float actualDayLength = halfCycle * dayLengthMultiplier;
        float actualNightLength = halfCycle * nightLengthMultiplier;

        float daySpeed = 12f / actualDayLength;
        float nightSpeed = 12f / actualNightLength;

        bool isDay = timeOfDay >= 6f && timeOfDay < 18f;

        float speed = isDay ? daySpeed : nightSpeed;

        timeOfDay += Time.deltaTime * speed;

        if (timeOfDay >= 24f)
            timeOfDay = 0f;
    }

    // -------------------------
    //       LIGHTING UPDATE
    // -------------------------
    private void UpdateLighting()
    {
        float t = timeOfDay / 24f;
        bool night = (timeOfDay >= 18f || timeOfDay < 6f);

        // Sun rotation
        float sunAngle;

        if (!night)
        {
            float dayT = (timeOfDay - 6f) / 12f;
            sunAngle = Mathf.Lerp(-90f, 90f, dayT);
        }
        else
        {
            float nightT = timeOfDay >= 18f
                ? (timeOfDay - 18f) / 12f
                : (timeOfDay + 6f) / 12f;

            sunAngle = Mathf.Lerp(90f, 270f, nightT);
        }

        directionalLight.transform.localRotation =
            Quaternion.Euler(sunAngle, 170f, 0);

        // Sun color
        Color targetSunColor = night ? Color.black : lightColor.Evaluate(t);
        currentSunColor = Color.Lerp(currentSunColor, targetSunColor, Time.deltaTime * 2f);
        directionalLight.color = currentSunColor;

        // Sun intensity
        float targetIntensity = night ? 0.2f : lightIntensity.Evaluate(t);
        currentSunIntensity = Mathf.Lerp(currentSunIntensity, targetIntensity, Time.deltaTime * 2f);
        directionalLight.intensity = currentSunIntensity;

        // Skybox exposure
        float targetExposure = night ? 0.2f : skyboxExposure.Evaluate(t);
        currentExposure = Mathf.Lerp(currentExposure, targetExposure, Time.deltaTime * 2f);

        if (skyboxMaterial)
            skyboxMaterial.SetFloat("_Exposure", currentExposure);

        // Ambient light
        RenderSettings.ambientLight = Color.Lerp(
            RenderSettings.ambientLight,
            night ? Color.black : Color.white,
            Time.deltaTime * 2f
        );

        RenderSettings.ambientIntensity = Mathf.Lerp(
            RenderSettings.ambientIntensity,
            night ? 0f : 1f,
            Time.deltaTime * 2f
        );

        RenderSettings.reflectionIntensity = Mathf.Lerp(
            RenderSettings.reflectionIntensity,
            night ? 0f : 1f,
            Time.deltaTime * 2f
        );

        // Fog
        RenderSettings.fogColor = Color.Lerp(
            RenderSettings.fogColor,
            night ? nightFogColor : dayFogColor,
            Time.deltaTime * 2f
        );

        RenderSettings.fogDensity = Mathf.Lerp(
            RenderSettings.fogDensity,
            night ? nightFogDensity : dayFogDensity,
            Time.deltaTime * 2f
        );
    }

    // -------------------------
    //        EVENT HANDLING
    // -------------------------
    private void HandleEvents()
    {
        // Sunrise at 6
        if (timeOfDay >= 6f && timeOfDay < 6.05f && !sunriseTriggered)
        {
            onSunrise.Invoke();
            onNewDay.Invoke();
            currentDay++;

            isNight = false;

            sunriseTriggered = true;
            sunsetTriggered = false;
        }

        // Sunset at 18
        if (timeOfDay >= 18f && timeOfDay < 18.05f && !sunsetTriggered)
        {
            onSunset.Invoke();
            isNight = true;

            sunsetTriggered = true;
            sunriseTriggered = false;
        }
    }
}
