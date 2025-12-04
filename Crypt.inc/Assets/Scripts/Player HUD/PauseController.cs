using UnityEngine;
using UnityEngine.EventSystems;

public class GamePauseController : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("UI Roots")]
    [Tooltip("Panel you want to show when paused (your Options menu)")]
    public GameObject pausePanel;  
    public GameObject hideWhenPausedRoot; 

    [Header("Cursor")]
    public bool unlockCursorOnPause = true;

    void Awake()
    {
        if (pausePanel) pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;

        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (pausePanel) pausePanel.SetActive(true);
        if (hideWhenPausedRoot) hideWhenPausedRoot.SetActive(false);

        if (unlockCursorOnPause)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (pausePanel) pausePanel.SetActive(false);
        if (hideWhenPausedRoot) hideWhenPausedRoot.SetActive(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Quit()
    {
        Debug.Log("testing");
    }
}
