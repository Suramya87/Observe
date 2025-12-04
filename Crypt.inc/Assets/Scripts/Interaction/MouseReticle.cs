using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MouseReticle : MonoBehaviour
{
    public RectTransform reticle;

    [Header("Behavior")]
    public float baseSize = 12f;
    public float clickSize = 24f;
    public float followSmooth = 0f;

    [Tooltip("If true and the cursor is UNLOCKED, reticle follows mouse. " +
             "If false (or cursor is LOCKED), reticle stays centered (FPS).")]
    public bool followMouseWhenUnlocked = false;

    Vector3 vel;
    float currentSize;

    void Awake()
    {
        currentSize = baseSize;
    }

    void Update()
    {
        bool isDown =
#if ENABLE_INPUT_SYSTEM
            Mouse.current != null ? Mouse.current.leftButton.isPressed :
#endif
            Input.GetMouseButton(0);

        float targetSize = isDown ? clickSize : baseSize;
        currentSize = Mathf.Lerp(currentSize, targetSize, 20f * Time.deltaTime);

        if (!reticle) return;

        Vector2 targetPos;

        bool locked = Cursor.lockState == CursorLockMode.Locked;
        if (!locked && followMouseWhenUnlocked)
        {
#if ENABLE_INPUT_SYSTEM
            targetPos =
                Mouse.current != null ? Mouse.current.position.ReadValue() :
                (Vector2)Input.mousePosition;
#else
            targetPos = Input.mousePosition;
#endif
        }
        else
        {
            targetPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        if (followSmooth <= 0f)
            reticle.position = targetPos;
        else
            reticle.position = Vector3.SmoothDamp(reticle.position, targetPos, ref vel, 1f / followSmooth);

        reticle.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentSize);
        reticle.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentSize);
    }
}
