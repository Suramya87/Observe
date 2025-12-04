using System.Collections;
using UnityEngine;

public class LightningController : MonoBehaviour
{
    [Header("Refs")]
    public Light lightningLight;
    public AudioSource thunderAudio;
    public AudioClip[] thunderClips;

    [Header("Timing (Auto Mode)")]
    public float minDelay = 5f;
    public float maxDelay = 15f;

    [Header("Flash Settings")]
    public float flashIntensity = 6f;

    Coroutine autoRoutine;
    bool autoEnabled = false;

    void Awake()
    {
        if (lightningLight != null)
            lightningLight.intensity = 0f;
    }

    void Start()
    {
        // Optional: start in auto mode by default
        StartAutoLightning();
    }

    // -------- AUTO MODE CONTROL (for scripts / animation events) --------

    public void StartAutoLightning()
    {
        if (autoEnabled) return;
        autoEnabled = true;
        autoRoutine = StartCoroutine(LightningRoutine());
    }

    public void StopAutoLightning()
    {
        autoEnabled = false;

        if (autoRoutine != null)
        {
            StopCoroutine(autoRoutine);
            autoRoutine = null;
        }

        if (lightningLight != null)
            lightningLight.intensity = 0f;
    }

    IEnumerator LightningRoutine()
    {
        while (autoEnabled)
        {
            float wait = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(wait);

            // do one lightning + thunder
            yield return StartCoroutine(DoFlashAndThunder());
        }
    }

    // -------- MANUAL STRIKE (for Animation Events) --------

    // Call this from an Animation Event exactly where you want lightning
    public void TriggerLightningOnce()
    {
        StartCoroutine(DoFlashAndThunder());
    }

    IEnumerator DoFlashAndThunder()
    {
        if (lightningLight == null)
            yield break;

        // Multiple flickers
        int flashes = Random.Range(2, 4);
        for (int i = 0; i < flashes; i++)
        {
            lightningLight.intensity = flashIntensity;
            yield return new WaitForSeconds(0.05f);

            lightningLight.intensity = 0f;
            yield return new WaitForSeconds(0.07f);
        }

        // Thunder after delay
        if (thunderAudio != null && thunderClips != null && thunderClips.Length > 0)
        {
            float delay = Random.Range(0.3f, 1.5f);
            yield return new WaitForSeconds(delay);

            var clip = thunderClips[Random.Range(0, thunderClips.Length)];
            thunderAudio.PlayOneShot(clip);
        }
    }
}

