using UnityEngine;
using System.Collections.Generic;

public class TabletCompass : MonoBehaviour
{
    public Transform player;          // Player transform
    public RectTransform arrow;       // UI arrow inside world-space canvas
    public List<Transform> items;     // Collectibles in order
    private int currentIndex = 0;

    void Update()
    {
        if (items.Count == 0 || currentIndex >= items.Count) return;

        Transform target = items[currentIndex];

        // Direction from tablet to target in world space
        Vector3 direction = target.position - player.position;
        direction.y = 0f; // ignore vertical

        // Convert world direction to local rotation for arrow
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        arrow.localRotation = Quaternion.Euler(0, 0, -angle);

        // Optional: collect item if close enough
        float distance = Vector3.Distance(player.position, target.position);
        if (distance < 2f)
        {
            CollectCurrentItem();
        }
    }

    void CollectCurrentItem()
    {
        items[currentIndex].gameObject.SetActive(false);
        currentIndex++;
    }
}
