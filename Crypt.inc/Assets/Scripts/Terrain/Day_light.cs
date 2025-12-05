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
    [Range(0f, 24f)] public float startTimeOfDay = 8f;

    [Header("Control")]
    public bool cycleEnabled = true;
    public bool realTimeSetAllowed = true;

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
    [Header("Performance")]
    public float lightingUpdateInterval = 0.05f; 
    private float lightingTimer = 0f;


    // -------------------------
    //         EVENTS
    // -------------------------
    [Header("Events")]
    public UnityEvent onSunrise;
    public UnityEvent onSunset;
    public UnityEvent onNewDay;

    [Header("Extra Time-of-Day Events")]
    public UnityEvent onMorning;
    public UnityEvent onNoon;
    public UnityEvent onEvening;

    // Internal state
    private bool sunriseTriggered = false;
    private bool sunsetTriggered = false;
    private bool morningTriggered = false;
    private bool noonTriggered = false;
    private bool eveningTriggered = false;

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
        timeOfDay = Mathf.Clamp(startTimeOfDay, 0f, 24f);
        UpdateLighting();
    }

    // -------------------------
    //          UPDATE
    // -------------------------
    void Update()
    {
        if (cycleEnabled)
            AdvanceTime();

        lightingTimer += Time.deltaTime;
        if (lightingTimer >= lightingUpdateInterval)
        {
            lightingTimer = 0f;
            UpdateLighting();  
        }

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
        // Fraction of day for gradient/curves (0-1)
        float t = timeOfDay / 24f;

        // -----------------
        // Sun visibility
        // -----------------
        bool sunVisible = timeOfDay >= 5f && timeOfDay <= 18f;

        // -----------------
        // Sun rotation (circular)
        // -----------------
        // 0° at 6 AM (sunrise), 90° at noon, 180° at 14 (sunset), 270° at midnight
        float sunAngle = (timeOfDay / 24f) * 360f - 90f;
        if (directionalLight != null)
        {
            directionalLight.transform.localRotation = Quaternion.Euler(sunAngle, 170f, 0);
            directionalLight.enabled = sunVisible; // hide during night
        }

        // -----------------
        // Sun color
        // -----------------
        Color targetSunColor;
        if (timeOfDay >= 5f && timeOfDay < 7f) // Dawn
            targetSunColor = Color.Lerp(Color.black, lightColor.Evaluate(0.25f), Mathf.InverseLerp(5f, 7f, timeOfDay));
        else if (timeOfDay >= 7f && timeOfDay < 14f) // Day
            targetSunColor = lightColor.Evaluate(Mathf.InverseLerp(7f, 14f, timeOfDay));
        else if (timeOfDay >= 14f && timeOfDay <= 18f) // Evening
            targetSunColor = Color.Lerp(lightColor.Evaluate(0.75f), Color.black, Mathf.InverseLerp(14f, 18f, timeOfDay));
        else // Night
            targetSunColor = Color.black;

        currentSunColor = Color.Lerp(currentSunColor, targetSunColor, Time.deltaTime * 2f);
        if (directionalLight != null) directionalLight.color = currentSunColor;

        // -----------------
        // Sun intensity
        // -----------------
        float targetIntensity = 0f;
        if (timeOfDay >= 5f && timeOfDay < 7f)        // Dawn: 0 → 1
            targetIntensity = Mathf.InverseLerp(5f, 7f, timeOfDay);
        else if (timeOfDay >= 7f && timeOfDay < 14f)  // Day: full brightness
            targetIntensity = 1f;
        else if (timeOfDay >= 14f && timeOfDay <= 18f) // Evening: 1 → 0
            targetIntensity = Mathf.InverseLerp(18f, 14f, timeOfDay);
        else
            targetIntensity = 0f; // Night

        currentSunIntensity = Mathf.Lerp(currentSunIntensity, targetIntensity, Time.deltaTime * 2f);
        if (directionalLight != null) directionalLight.intensity = currentSunIntensity;

        // -----------------
        // Skybox exposure
        // -----------------
        float targetExposure;
        if (timeOfDay >= 5f && timeOfDay < 7f)        // Dawn: subtle rise
            targetExposure = Mathf.Lerp(0.2f, skyboxExposure.Evaluate(0.25f), Mathf.InverseLerp(5f, 7f, timeOfDay));
        else if (timeOfDay >= 7f && timeOfDay < 14f)  // Day
            targetExposure = skyboxExposure.Evaluate(Mathf.InverseLerp(7f, 14f, timeOfDay));
        else if (timeOfDay >= 14f && timeOfDay <= 18f) // Evening: decrease
            targetExposure = Mathf.Lerp(skyboxExposure.Evaluate(0.75f), 0.2f, Mathf.InverseLerp(14f, 18f, timeOfDay));
        else
            targetExposure = 0.2f; // Night

        currentExposure = Mathf.Lerp(currentExposure, targetExposure, Time.deltaTime * 2f);
        if (skyboxMaterial != null) skyboxMaterial.SetFloat("_Exposure", currentExposure);

        // -----------------
        // Ambient light & reflection
        // -----------------
        Color targetAmbientColor = (timeOfDay >= 5f && timeOfDay < 18f) ? Color.white : Color.black;
        float targetAmbientIntensity = (timeOfDay >= 5f && timeOfDay < 18f) ? 1f : 0f;
        float targetReflectionIntensity = targetAmbientIntensity;

        RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, targetAmbientColor, Time.deltaTime * 2f);
        RenderSettings.ambientIntensity = Mathf.Lerp(RenderSettings.ambientIntensity, targetAmbientIntensity, Time.deltaTime * 2f);
        RenderSettings.reflectionIntensity = Mathf.Lerp(RenderSettings.reflectionIntensity, targetReflectionIntensity, Time.deltaTime * 2f);

        // -----------------
        // Fog
        // -----------------
        Color targetFogColor = (timeOfDay >= 5f && timeOfDay < 18f) ? dayFogColor : nightFogColor;
        float targetFogDensity = (timeOfDay >= 5f && timeOfDay < 18f) ? dayFogDensity : nightFogDensity;

        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetFogColor, Time.deltaTime * 2f);
        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetFogDensity, Time.deltaTime * 2f);
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

            // Reset other triggers for the new day
            morningTriggered = false;
            noonTriggered = false;
            eveningTriggered = false;
        }

        // Morning at 9
        if (timeOfDay >= 9f && timeOfDay < 9.05f && !morningTriggered)
        {
            onMorning.Invoke();
            morningTriggered = true;
        }

        // Noon at 12
        if (timeOfDay >= 12f && timeOfDay < 12.05f && !noonTriggered)
        {
            onNoon.Invoke();
            noonTriggered = true;
        }

        // Evening at 16
        if (timeOfDay >= 16f && timeOfDay < 16.05f && !eveningTriggered)
        {
            onEvening.Invoke();
            eveningTriggered = true;
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
