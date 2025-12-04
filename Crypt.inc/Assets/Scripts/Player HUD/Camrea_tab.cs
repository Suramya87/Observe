using UnityEngine;

public class TabletToggle : MonoBehaviour
{
    [Header("Assign the tab object (child)")]
    public GameObject tablet;

    [Header("Key to toggle")]
    public KeyCode toggleKey = KeyCode.F;

    
    void Start()
    {
        if (tablet != null)
            tablet.SetActive(false); 
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && tablet != null)

        {
            Debug.Log("Screenager lol");
            tablet.SetActive(!tablet.activeSelf);
        }
    }
}