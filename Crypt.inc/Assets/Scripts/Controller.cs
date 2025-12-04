using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject optionsPanel;
    [SerializeField] GameObject creditsPanel;

    [Header("Scenes")]
    [SerializeField] string gameSceneName = "Level 1"; // change to your play scene name

    public void StartGame() => SceneManager.LoadScene(gameSceneName);

    public void OpenOptions(bool open = true)
    {
        if (optionsPanel) optionsPanel.SetActive(open);
        if (open && creditsPanel) creditsPanel.SetActive(false);
    }

    public void OpenCredits(bool open = true)
    {
        if (creditsPanel) creditsPanel.SetActive(open);
        if (open && optionsPanel) optionsPanel.SetActive(false);
    }

    public void Back()
    {
        OpenOptions(false);
        OpenCredits(false);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
