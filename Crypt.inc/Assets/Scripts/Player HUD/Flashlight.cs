using UnityEngine;

public class FlashlightToggle : MonoBehaviour
{
    [Header("Assign the flashlight object (child)")]
    public GameObject flashlight;

    [Header("Key to toggle")]
    public KeyCode toggleKey = KeyCode.F;

    void Start()
    {
        if (flashlight != null)
            flashlight.SetActive(false); 
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && flashlight != null)

        {
            Debug.Log("on or off?");
            flashlight.SetActive(!flashlight.activeSelf);
        }
    }
}
