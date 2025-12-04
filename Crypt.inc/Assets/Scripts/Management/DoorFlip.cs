// DoorPowerPoller.cs
using UnityEngine;

public class DoorPowerPoller : MonoBehaviour
{
    public DoorShield door;
    public bool openWhenNoPower = true;
    public bool closeOnPowerReturn = false;

    bool lastGridOn = true;

    void Awake()
    {
        if (!door) door = GetComponent<DoorShield>();
    }

    void Update()
    {
        var grid = PowerGridManager.Instance;
        bool gridOn = grid != null && grid.IsOn;

        if (gridOn == lastGridOn) return;
        lastGridOn = gridOn;

        if (!gridOn && openWhenNoPower && door) door.SetOpen(true);
        else if (gridOn && closeOnPowerReturn && door) door.SetOpen(false);
    }
}
