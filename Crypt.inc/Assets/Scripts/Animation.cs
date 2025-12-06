using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationSceneController : MonoBehaviour
{
    public Animator animator;
    public string nextSceneName;
    public float animationLength = 5f; // set to your animation duration

    void Start()
    {
        StartCoroutine(WaitAndLoad());
    }

    System.Collections.IEnumerator WaitAndLoad()
    {
        yield return new WaitForSeconds(animationLength);
        SceneManager.LoadScene(nextSceneName);
    }
}
