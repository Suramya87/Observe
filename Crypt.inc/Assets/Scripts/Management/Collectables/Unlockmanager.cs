using UnityEngine;
using UnityEngine.SceneManagement;

public class UnlockManager : MonoBehaviour
{
    [Header("Special Item ID Required To Unlock Others")]
    public string requiredItemId;

    [Header("Objects Hidden Until Unlock")]
    public GameObject[] unlockables;

    [Header("IDs of items required for winning")]
    public string[] requiredWinItems;

    [Header("Scene to load when player wins")]
    public string winSceneName = "WinScene";

    private BaseDropoffZone dropoff;

    void Start()
    {
        dropoff = FindObjectOfType<BaseDropoffZone>();

        // Hide all unlockables at start
        foreach (var obj in unlockables)
            if (obj != null)
                obj.SetActive(false);
    }

    void Update()
    {
        if (dropoff == null) return;

        // Unlock all items if special item is in base
        if (dropoff.HasItemInBase(requiredItemId))
        {
            foreach (var obj in unlockables)
                if (obj != null)
                    obj.SetActive(true);
        }

        // Check for win condition
        if (AllWinItemsCollected())
        {
            SceneManager.LoadScene(winSceneName);
        }
    }

    bool AllWinItemsCollected()
    {
        foreach (string id in requiredWinItems)
        {
            if (!dropoff.HasItemInBase(id))
                return false;
        }
        return true;
    }
}
