using UnityEngine;
using UnityEngine.SceneManagement; // Needed to load scenes
using System.Collections;

public class AreaStandController_LoadScene : MonoBehaviour
{
    [Header("References")]
    public PlayerControllerCC playerController;
    public GameObject standTextPrefab;        // Optional UI prefab for feedback
    public GameObject completePrefab;         // Optional effect prefab

    [Header("Stage 1: Large Area")]
    public Collider largeArea;                // Big area collider
    public float stage1Duration = 5f;         // Time to stay in large area

    [Header("Stage 2: Small Area")]
    public Collider smallArea;                // Smaller final target area

    [Header("Post-Completion")]
    public float effectDuration = 1f;         // How long the effect lasts

    [Header("Scene to Load")]
    public string nextSceneName;              // Name of scene to load when task completed

    private float timer = 0f;
    private bool stage1Completed = false;
    private bool taskCompleted = false;

    void Update()
    {
        if (taskCompleted) return;

        Vector3 playerPos = playerController.transform.position;

        // Stage 1: Large area
        if (!stage1Completed)
        {
            if (largeArea.bounds.Contains(playerPos))
            {
                timer += Time.deltaTime;

                if (standTextPrefab != null && timer <= stage1Duration)
                {
                    if (!standTextPrefab.activeSelf)
                        standTextPrefab.SetActive(true);
                }

                if (timer >= stage1Duration)
                {
                    stage1Completed = true;
                    timer = 0f;

                    if (standTextPrefab != null)
                        standTextPrefab.SetActive(false);

                    if (completePrefab != null)
                        Instantiate(completePrefab, playerPos + Vector3.up, Quaternion.identity);
                }
            }
            else
            {
                timer = 0f;
            }
        }
        else // Stage 2: Small area
        {
            if (smallArea.bounds.Contains(playerPos))
            {
                taskCompleted = true;

                if (completePrefab != null)
                    Instantiate(completePrefab, playerPos + Vector3.up, Quaternion.identity);

                Debug.Log("Task completed! Loading next scene...");

                // Load the new scene
                if (!string.IsNullOrEmpty(nextSceneName))
                    SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
