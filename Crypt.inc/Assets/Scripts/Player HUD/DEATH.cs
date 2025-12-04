using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerHealth player;         
    public GameObject deathPanel;       
    public GameObject hideWhenDeadRoot; 

    [Header("Scenes")]
    public string gameSceneName = "Level 1"; 
    public string mainMenuSceneName = "MainMenu"; 

    bool shown;

    void Awake()
    {
        if (deathPanel) deathPanel.SetActive(false);
    }

    void OnEnable()
    {
        if (player) player.OnDied += HandleDied;
    }

    void OnDisable()
    {
        if (player) player.OnDied -= HandleDied;
    }

    void HandleDied()
    {
        if (shown) return;
        shown = true;

        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (hideWhenDeadRoot) hideWhenDeadRoot.SetActive(false);
        if (deathPanel) deathPanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(gameSceneName);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
