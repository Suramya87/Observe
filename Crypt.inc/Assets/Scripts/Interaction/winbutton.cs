using UnityEngine;
using UnityEngine.SceneManagement;

public class WinButton : MonoBehaviour, IInteractable
{
    [Header("Scene to Load")]
    public string nextSceneName;

    public string Prompt => "Press to win!";

    public void Interact(Transform interactor)
    {
        Debug.Log("Win button pressed! You win!");
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
