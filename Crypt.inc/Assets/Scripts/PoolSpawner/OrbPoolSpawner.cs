using UnityEngine;

public class OrbPoolSpawner : MonoBehaviour, IPowerConsumer, IInteractable
{
    public ObjectPool pool;
    public Transform spawnPoint;
    public float launchSpeed = 8f;
    public float cooldown = 0.15f;

    public string Prompt => "Click to spawn orb";

    bool powered = true;
    float nextTime;

    public void Interact(Transform interactor)
    {
        if (!powered || Time.time < nextTime || !pool || !spawnPoint) return;

        var orb = pool.Get(spawnPoint.position, spawnPoint.rotation);
        float s = Random.Range(0.8f, 1.3f);
        orb.transform.localScale = Vector3.one * s;

        var r = orb.GetComponent<Renderer>();
        if (r) r.material.SetColor("_EmissionColor",
             Color.HSVToRGB(Random.value, 0.8f, 1f) * 2f);

        if (orb)
        {
            var rb = orb.GetComponent<Rigidbody>();
            if (rb) rb.linearVelocity = spawnPoint.forward * launchSpeed;
        }
        nextTime = Time.time + cooldown;
    }

    public void OnPowerChanged(bool isOn) { powered = isOn; }
}
