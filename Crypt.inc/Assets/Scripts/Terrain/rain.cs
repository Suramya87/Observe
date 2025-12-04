using UnityEngine;

public class RainController : MonoBehaviour
{
    public ParticleSystem rain;

    void Awake()
    {
        if (rain == null)
            rain = GetComponent<ParticleSystem>();
    }

    // Call this from an Animation Event if you want to start rain
    public void StartRain()
    {
        if (rain == null) return;

        var emission = rain.emission;
        emission.enabled = true;
        rain.Play();
    }

    // Call this from an Animation Event when you want rain to stop
    public void StopRain()
    {
        if (rain == null) return;

        var emission = rain.emission;
        emission.enabled = false;
        // Stop emitting and clear existing drops so they don't fall through ceilings later
        rain.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
