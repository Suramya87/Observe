using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PowerGridManager : MonoBehaviour
{
    public static PowerGridManager Instance { get; private set; }

    // ---------- Energy ----------
    [Header("Energy")]
    public float maxEnergy = 100f;
    public float startEnergy = 100f;
    public float cooldownSeconds = 6f;      // must wait this long before restart
    public bool allowIdleRegen = false;     // optional: refill when no drains
    public float idleRegenPerSecond = 0f;   // leave 0 for no regen

    // Handy read-only 0..1 for UI
    public float Energy01 => (maxEnergy <= 0f) ? 0f : energy / maxEnergy;

    // ---------- Warmup ----------
    [Header("Warmup")]
    public float warmupSeconds = 1.0f;

    // ---------- Debug ----------
    [Header("Debug")]
    public bool verbose = true;
    [SerializeField] float logChunk = 1f;   // condense logs: print when ≥ this drained

    // ---------- Events ----------
    public System.Action<float> OnEnergyChanged;      // ratio 0..1
    public System.Action<bool> OnCoolingChanged;     // true when cooling starts/ends
    public System.Action<float> OnCooldownProgress;   // 0..1 over the cooldown window

    // ---------- State ----------
    bool isOn;
    public bool IsOn => isOn;

    bool isCooling;
    public bool IsCooling => isCooling;

    float energy;
    [SerializeField] bool debugIsOn;

    // used to condense drain logging
    float drainAccum;

    // restart lock (stronger than just isCooling)
    float restartReadyAt = -1f;

    Coroutine warmupCo;
    Coroutine cooldownCo;

    // consumers get ON/OFF (TV, powered interactables)
    readonly List<IPowerConsumer> consumers = new();

    // drains report drain/sec (doors closed, TV on, etc.)
    readonly List<IPowerDrainer> drainers = new();

    // ---------- Lifecycle ----------
    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        energy = Mathf.Clamp(startEnergy, 0, maxEnergy);
        FireEnergyChanged();
    }

    void Update()
    {
        if (!isOn) return;

        // Sum up drains
        float totalDrain = 0f;
        for (int i = 0; i < drainers.Count; i++)
        {
            var d = drainers[i];
            if (d == null) continue;
            totalDrain += Mathf.Max(0f, d.CurrentDrainPerSecond);
        }

        if (totalDrain > 0f)
        {
            float before = energy;
            energy -= totalDrain * Time.deltaTime;

            // condensed logging
            float consumedNow = Mathf.Max(0f, before - energy);
            drainAccum += consumedNow;

            if (drainAccum >= Mathf.Max(0.01f, logChunk) || energy <= 0f)
            {
                float ratio = (maxEnergy <= 0f) ? 0f : energy / maxEnergy;
                // Debug.Log($"[PowerGrid] Drain {drainAccum:0.00} (rate {totalDrain:0.00}/s) → Energy {energy:0.00}/{maxEnergy} ({ratio:P0})");
                drainAccum = 0f;
            }

            if (energy <= 0f)
            {
                energy = 0f;
                FireEnergyChanged();
                ForceShutdownAndCooldown();
                return;
            }

            FireEnergyChanged();
        }
        else if (allowIdleRegen && idleRegenPerSecond > 0f)
        {
            float before = energy;
            energy = Mathf.Min(maxEnergy, energy + idleRegenPerSecond * Time.deltaTime);

            float gained = energy - before;
            if (gained >= Mathf.Max(0.01f, logChunk))
            {
                float ratio = (maxEnergy <= 0f) ? 0f : energy / maxEnergy;
                // Debug.Log($"[PowerGrid] Regen {gained:0.00} (rate {idleRegenPerSecond:0.00}/s) → Energy {energy:0.00}/{maxEnergy} ({ratio:P0})");
            }

            FireEnergyChanged();
        }
    }

    void FireEnergyChanged() =>
        OnEnergyChanged?.Invoke((maxEnergy <= 0f) ? 0f : energy / maxEnergy);

    // ---------- Public control ----------
    public void TogglePower()
    {
        // if (verbose) Debug.Log($"[PowerGrid] Toggle pressed. isOn={isOn}, isCooling={isCooling}, warmupCo={(warmupCo != null)}");

        // HARD gate by timestamp so restarts are impossible before the window ends
        if (Time.time < restartReadyAt)
        {
            // if (verbose) Debug.Log($"[PowerGrid] Restart locked ({Mathf.CeilToInt(restartReadyAt - Time.time)}s left).");
            return;
        }

        if (isCooling) return;      // extra guard
        if (warmupCo != null) return;

        if (isOn) TurnOff();
        else
        {
            if (energy <= 0f) return; // no juice
            TurnOn();
        }
    }

    public void TurnOn()
    {
        if (isOn || isCooling) return;
        if (warmupCo != null) StopCoroutine(warmupCo);
        warmupCo = StartCoroutine(CoWarmup());
    }

    public void TurnOff()
    {
        if (warmupCo != null) { StopCoroutine(warmupCo); warmupCo = null; }
        SetPowerState(false);
    }

    void SetPowerState(bool on)
    {
        isOn = on;
        debugIsOn = on;

        // if (verbose) Debug.Log($"[PowerGrid] SetPowerState({on})");

        // keep your indicator lights in sync
        PowerIndicator.SnapAll(on);

        // notify logical consumers
        NotifyAll(on);
    }

    IEnumerator CoWarmup()
    {
        float t = 0f;
        while (t < warmupSeconds)
        {
            t += Time.deltaTime;
            PowerIndicator.BroadcastWarmup(Mathf.Clamp01(t / warmupSeconds));
            yield return null;
        }
        warmupCo = null;

        // Guard: no energy? don't turn on
        if (energy <= 0f) yield break;

        SetPowerState(true);
    }

    void ForceShutdownAndCooldown()
    {
        // if (verbose) Debug.Log("[PowerGrid] Energy depleted → shutdown & cooldown");

        SetPowerState(false);

        // establish the lock window right now (stronger than only isCooling)
        restartReadyAt = Time.time + Mathf.Max(0f, cooldownSeconds);

        if (cooldownCo != null) StopCoroutine(cooldownCo);
        cooldownCo = StartCoroutine(CoCooldown());
    }

    IEnumerator CoCooldown()
    {
        isCooling = true;
        OnCoolingChanged?.Invoke(true);
        OnCooldownProgress?.Invoke(0f);

        float start = Time.time;
        float end = restartReadyAt;

        while (Time.time < end)
        {
            float t = Mathf.InverseLerp(start, end, Time.time); // 0..1
            OnCooldownProgress?.Invoke(t);
            yield return null;
        }

        isCooling = false;
        cooldownCo = null;
        OnCoolingChanged?.Invoke(false);
        OnCooldownProgress?.Invoke(1f);

        // ---------- REFILL so restart is possible ----------
        // choose one; defaulting to startEnergy:
        energy = Mathf.Clamp(startEnergy, 0f, maxEnergy);
        // Or: energy = maxEnergy;              // full refill
        // Or: energy = Mathf.Max(energy, maxEnergy * 0.25f); // 25% reserve
        FireEnergyChanged();

        // clear the timestamp lock
        restartReadyAt = -1f;
    }

    // ---------- Consumers (want ON/OFF) ----------
    public void Register(IPowerConsumer c)
    {
        if (c == null) return;
        if (!consumers.Contains(c))
        {
            consumers.Add(c);
            c.OnPowerChanged(isOn);   // push current state immediately
            if (verbose)
            {
                var mb = c as MonoBehaviour;
                // Debug.Log($"[PowerGrid] + Registered consumer {(mb ? mb.name : c.GetType().Name)}");
            }
        }
    }

    public void Unregister(IPowerConsumer c)
    {
        if (c == null) return;
        if (consumers.Remove(c) && verbose)
        {
            var mb = c as MonoBehaviour;
            // Debug.Log($"[PowerGrid] - Unregistered consumer {(mb ? mb.name : c.GetType().Name)}");
        }
    }

    void NotifyAll(bool on)
    {
        // if (verbose) Debug.Log($"[PowerGrid] NotifyAll({on}) to {consumers.Count} consumers");
        for (int i = 0; i < consumers.Count; i++)
        {
            var c = consumers[i];
            // if (c == null) { if (verbose) Debug.Log($"[PowerGrid]   consumer {i} is NULL"); continue; }

            var mb = c as MonoBehaviour;
            string who = mb ? mb.name : c.GetType().Name;

            try
            {
                // if (verbose) Debug.Log($"[PowerGrid]   -> notifying {who} ({c.GetType().Name}) with {on}");
                c.OnPowerChanged(on);
            }
            catch (System.Exception ex)
            {
                // Debug.LogError($"[PowerGrid]   !!! {who} threw during OnPowerChanged({on}):\n{ex}");
            }
        }
    }

    // ---------- Drainers (report drain/sec) ----------
    public void RegisterDrainer(IPowerDrainer d)
    {
        if (d == null) return;
        if (!drainers.Contains(d)) drainers.Add(d);
    }

    public void UnregisterDrainer(IPowerDrainer d)
    {
        if (d == null) return;
        drainers.Remove(d);
    }
}

// Simple interface for “things that consume energy”
public interface IPowerDrainer
{
    float CurrentDrainPerSecond { get; }
}
