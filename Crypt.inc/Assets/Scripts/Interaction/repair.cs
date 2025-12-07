using UnityEngine;
using System.Collections;

public class HammerRepairController_Hold_Fixed2 : MonoBehaviour
{
    [Header("References")]
    public PlayerControllerCC playerController;
    public GameObject hammerObject;
    public KeyCode repairKey = KeyCode.E;

    [Header("Repair Text Prefab")]
    public GameObject repairTextPrefab;
    public float repairTextDuration = 1f;

    [Header("Post-Repair Prefab")]
    public GameObject postRepairPrefab;
    public float postRepairDuration = 1f;

    [Header("Interaction Settings")]
    public float interactDistance = 3f;
    public LayerMask interactMask;

    private bool isRepairing = false;
    private Collider currentTarget;

    void Start()
    {
        if (hammerObject != null)
            hammerObject.SetActive(false);
    }

    void Update()
    {
        // Cast ray to see what we're pointing at
        Ray ray = new Ray(playerController.cam.position, playerController.cam.forward);
        Collider hitCollider = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
        {
            if (hit.collider.CompareTag("Repairable"))
                hitCollider = hit.collider;
        }

        // Start repair on key down
        if (Input.GetKeyDown(repairKey) && hitCollider != null)
        {
            StartRepair(hitCollider);
        }

        // Continue showing hammer while holding key
        if (Input.GetKey(repairKey))
        {
            if (isRepairing && hitCollider == currentTarget)
            {
                if (hammerObject != null && !hammerObject.activeSelf)
                    hammerObject.SetActive(true);
            }
            else
            {
                StopRepair();
            }
        }

        // Stop repair if key released
        if (Input.GetKeyUp(repairKey))
            StopRepair();
    }

    void StartRepair(Collider repairTarget)
    {
        if (isRepairing) return;

        isRepairing = true;
        currentTarget = repairTarget;

        if (hammerObject != null)
            hammerObject.SetActive(true);

        playerController.movementLocked = true;

        if (repairTextPrefab != null)
        {
            GameObject textInstance = Instantiate(repairTextPrefab);
            Destroy(textInstance, repairTextDuration);
        }

        StartCoroutine(EndRepairAfterDelay(repairTarget, repairTextDuration));
    }

    IEnumerator EndRepairAfterDelay(Collider repairedObject, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (hammerObject != null)
            hammerObject.SetActive(false);

        playerController.movementLocked = false;
        isRepairing = false;
        currentTarget = null;

        if (postRepairPrefab != null && repairedObject != null)
        {
            GameObject postInstance = Instantiate(postRepairPrefab, repairedObject.transform.position + Vector3.up, Quaternion.identity);
            Destroy(postInstance, postRepairDuration);
        }

        if (repairedObject != null)
            repairedObject.enabled = false;
    }

    void StopRepair()
    {
        if (!isRepairing) return;

        if (hammerObject != null)
            hammerObject.SetActive(false);

        playerController.movementLocked = false;
        isRepairing = false;
        currentTarget = null;
    }
}
