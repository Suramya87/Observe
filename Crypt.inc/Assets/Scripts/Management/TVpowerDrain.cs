using UnityEngine;

[RequireComponent(typeof(TVScreenController))]
public class TVPowerDrainer : MonoBehaviour, IPowerDrainer
{
    public float drainWhenOn = 2f; // units / sec
    TVScreenController tv;

    void Awake() => tv = GetComponent<TVScreenController>();
    void OnEnable() => PowerGridManager.Instance?.RegisterDrainer(this);
    void OnDisable() => PowerGridManager.Instance?.UnregisterDrainer(this);

    public float CurrentDrainPerSecond => (tv && tv.IsOn) ? drainWhenOn : 0f;
}
