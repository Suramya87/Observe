using UnityEngine;
using System.Collections.Generic;

public class TabletCompass : MonoBehaviour
{
    [Tooltip("Player or camera root (used only for optional collection checks).")]
    public Transform player;

    [Tooltip("The RectTransform of the arrow (child of the tablet/canvas).")]
    public RectTransform arrow;

    [Tooltip("Collectible targets in order.")]
    public List<Transform> items;

    [Tooltip("If true, the script will still disable an item when within collectDistance.")]
    public bool autoCollect = true;

    [Tooltip("Distance threshold to auto-collect an item.")]
    public float collectDistance = 2f;

    private int currentIndex = 0;

    // If the script is attached to the tablet object, this is the tablet transform.
    // If you attach script elsewhere, set this to the tablet transform in inspector.
    public Transform tablet; // CHANGED: explicit tablet transform (set to this GameObject by default in Awake)

    void Awake()
    {
        if (tablet == null)
            tablet = this.transform;
    }

    void Update()
    {
        if (items == null || items.Count == 0 || currentIndex >= items.Count) return;

        Transform target = items[currentIndex];
        if (target == null) return;

        // ---------- Calculate direction in tablet-local space ----------
        // World direction from the tablet (the plane) to the target.
        Vector3 worldDirection = target.position - tablet.position;

        // Project onto the tablet's local plane: convert worldDirection into tablet local space.
        // localDir is expressed in the tablet's local axes (x = right, y = up, z = forward (normal)).
        Vector3 localDir = tablet.InverseTransformDirection(worldDirection);

        // We want the arrow to rotate inside the tablet plane (its local X-Y). Compute the angle on that plane.
        // Mathf.Atan2(y, x) gives the angle from the tablet's local +X axis toward +Y axis (in radians).
        float angleDegrees = Mathf.Atan2(localDir.y, localDir.x) * Mathf.Rad2Deg;

        // ---------- Apply rotation: keep arrow's local X and Y, only change Z ----------
        // We preserve the arrow's existing local X and Y rotation so the 2D image stays aligned with the tablet.
        Vector3 currentEuler = arrow.localEulerAngles;

        // Depending on how your arrow sprite is oriented, you may need to flip sign or offset by 90 degrees.
        // If arrow points the wrong way, try: angleDegrees = -angleDegrees; or angleDegrees += 90f;
        float zRotation = -angleDegrees; // CHANGED: negate to match typical UI coordinate directions (tweak if needed)

        arrow.localEulerAngles = new Vector3(currentEuler.x, currentEuler.y, -zRotation); // CHANGED: only Z set

        // ---------- Optional: auto-collect if close enough ----------
        if (autoCollect && player != null)
        {
            float distance = Vector3.Distance(player.position, target.position);
            if (distance < collectDistance)
            {
                CollectCurrentItem();
            }
        }
    }

    void CollectCurrentItem()
    {
        if (currentIndex < items.Count && items[currentIndex] != null)
        {
            items[currentIndex].gameObject.SetActive(false);
            currentIndex++;
        }
    }
}
