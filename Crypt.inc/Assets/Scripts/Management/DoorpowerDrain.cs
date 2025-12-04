using UnityEngine;

[RequireComponent(typeof(DoorShield))]
public class DoorPowerDrainer : MonoBehaviour, IPowerDrainer
{
    public float drainWhenClosed = 1f;  // units / sec
    DoorShield door;

    void Awake() => door = GetComponent<DoorShield>();
    void OnEnable() => PowerGridManager.Instance?.RegisterDrainer(this);
    void OnDisable() => PowerGridManager.Instance?.UnregisterDrainer(this);

    public float CurrentDrainPerSecond => (door && !door.IsOpen) ? drainWhenClosed : 0f;
}
