using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TVScreenTeleporter : MonoBehaviour
{
    [Header("Refs")]
    public TVScreenController tv;
    public Transform[] arrivalPoints;
    public Spawner spawner;

    [Header("Defaults")]
    public float forwardOffset = 1.2f;
    public bool matchCameraYawOnly = true;
    public LayerMask groundMask = ~0;
    public float snapDownDistance = 3f;

    [Header("Gating")]
    public bool requirePowerOn = true;

    Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Teleport the player
        Warp(other.transform);

        // Optional: for debugging/visuals, we can highlight the current camera's drone spawn point
        // ActivateDroneSpawnPoints();
    }

    public void Warp(Transform playerRoot)
    {
        if (!tv || tv.SourceCount == 0) return;

        Camera cam = tv.sources[tv.ActiveIndex];
        if (!cam) return;

        Vector3 dstPos;
        Quaternion dstRot;

        // Optional arrival point per camera
        if (arrivalPoints != null &&
            tv.ActiveIndex < arrivalPoints.Length &&
            arrivalPoints[tv.ActiveIndex] != null)
        {
            dstPos = arrivalPoints[tv.ActiveIndex].position;
            dstRot = arrivalPoints[tv.ActiveIndex].rotation;
        }
        else
        {
            var cT = cam.transform;
            dstPos = cT.position + cT.forward * forwardOffset;
            dstRot = matchCameraYawOnly
                ? Quaternion.Euler(0f, cT.eulerAngles.y, 0f)
                : cT.rotation;
        }

        // Snap to ground
        if (Physics.Raycast(dstPos + Vector3.up, Vector3.down, out var hit, snapDownDistance + 1f, groundMask))
            dstPos = hit.point;

        var cc = playerRoot.GetComponent<CharacterController>();
        if (cc)
        {
            cc.enabled = false;
            playerRoot.SetPositionAndRotation(dstPos, dstRot);
            cc.enabled = true;
        }
        else
        {
            playerRoot.SetPositionAndRotation(dstPos, dstRot);
        }
    }

    void ActivateDroneSpawnPoints()
    {
        if (spawner == null || spawner.droneSpawnPoints.Length == 0) return;

        // Disable all spawn points
        foreach (var point in spawner.droneSpawnPoints)
        {
            if (point != null)
                point.gameObject.SetActive(false);
        }

        // Enable spawn point for active camera
        int camIndex = tv.ActiveIndex;
        if (camIndex < spawner.droneSpawnPoints.Length && spawner.droneSpawnPoints[camIndex] != null)
        {
            spawner.droneSpawnPoints[camIndex].gameObject.SetActive(true);
        }
    }
}
