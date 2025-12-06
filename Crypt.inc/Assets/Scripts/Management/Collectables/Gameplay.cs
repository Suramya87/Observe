using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    [Header("Collectibles")]
    public List<Transform> items; // All items to collect

    [Header("Player / Compass")]
    public Transform player;
    public RectTransform arrow;   // UI arrow on tablet/canvas
    public Transform tablet;      // For compass local plane

    [Header("Win Settings")]
    public string winSceneName = "WinScene";

    [Header("Performance Settings")]
    public float checkInterval = 0.2f; // How often to check for collection (seconds)

    private List<Transform> collectedItems = new List<Transform>();
    private float timer = 0f;

    void Update()
    {
        // Timer for distance-based collection
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            CheckForCollection();
        }

        HandleCompass();
        CheckWinCondition();
    }

    private void CheckForCollection()
    {
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item != null && !collectedItems.Contains(item) &&
                Vector3.Distance(player.position, item.position) < 2f) // collection distance
            {
                collectedItems.Add(item);
                item.gameObject.SetActive(false); // disappear
                Debug.Log($"Collected item: {item.name}");
            }
        }
    }

    private void HandleCompass()
    {
        if (arrow == null || items.Count == 0) return;

        // Find next uncollected item
        Transform target = null;
        foreach (var item in items)
        {
            if (!collectedItems.Contains(item) && item.gameObject.activeInHierarchy)
            {
                target = item;
                break;
            }
        }

        if (target == null) return;

        // Compass direction in tablet local space
        Vector3 worldDir = target.position - tablet.position;
        Vector3 localDir = tablet.InverseTransformDirection(worldDir);
        float angle = Mathf.Atan2(localDir.y, localDir.x) * Mathf.Rad2Deg;
        Vector3 currentEuler = arrow.localEulerAngles;
        arrow.localEulerAngles = new Vector3(currentEuler.x, currentEuler.y, angle);
    }

    private void CheckWinCondition()
    {
        if (collectedItems.Count >= items.Count)
        {
            Debug.Log("All items collected! Loading win scene...");
            SceneManager.LoadScene(winSceneName);
        }
    }
}
