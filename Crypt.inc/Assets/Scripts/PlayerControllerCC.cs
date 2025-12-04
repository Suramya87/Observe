using UnityEngine;


// coconut
[RequireComponent(typeof(CharacterController))]
public class PlayerControllerCC : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.5f;
    public float crouchMultiplier = 0.5f;
    public float crouchHeight = 1f;
    public float crawlMultiplier = 0.15f;
    public float crawlHeight = 0.5f;
    public float standHeight = 2f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    private bool isCrouching = false;
    private bool isCrawling = false;

    [Header("Mouse Look")]
    public Transform cam;
    public float mouseSensitivity = 1.2f;
    public float maxLookX = 85f;

    [Header("Camera Positions")]
    public Vector3 standCamPos = new Vector3(0, 0.9f, 0);
    public Vector3 crouchCamPos = new Vector3(0, 0.6f, 0);
    public Vector3 crawlCamPos = new Vector3(0, 0.3f, 0);
    public float camLerpSpeed = 8f;

    [Header("Interaction")]
    public float interactDistance = 3f;
    public LayerMask interactMask = ~0;

    CharacterController cc;
    Vector3 velocity;
    float rotX;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Bail early while paused (no camera, no movement, no interaction)
        if (GamePauseController.IsPaused) return;

        Look();
        Move();
        Interact();
    }

    void Look()
    {
        float mx = Input.GetAxis("Mouse X") * 10f * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * 10f * mouseSensitivity;

        transform.Rotate(0f, mx, 0f);
        rotX = Mathf.Clamp(rotX - my, -maxLookX, maxLookX);
        if (cam) cam.localRotation = Quaternion.Euler(rotX, 0f, 0f);
    }

    // Movement with crouch and crawl
    void Move()
    {
        bool grounded = cc.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -2f;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 input = (transform.right * x + transform.forward * z).normalized;

        // Toggle crouch
        if (Input.GetButtonDown("Crouch"))
        {
            isCrawling = false;
            isCrouching = !isCrouching;
            cc.height = isCrouching ? crouchHeight : standHeight;
        }

        // Toggle crawl
        if (Input.GetButtonDown("Crawl"))
        {
            isCrouching = false;
            isCrawling = !isCrawling;
            cc.height = isCrawling ? crawlHeight : standHeight;
        }

        // Determine movement speed
        float speed = moveSpeed;
        if (Input.GetButton("Sprint") && !isCrouching && !isCrawling)
            speed *= sprintMultiplier;
        else if (isCrouching)
            speed *= crouchMultiplier;
        else if (isCrawling)
            speed *= crawlMultiplier;

        // Apply movement
        cc.Move(input * speed * Time.deltaTime);

        // Jump
        if (grounded && Input.GetButtonDown("Jump") && !isCrawling)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }

    void UpdateCameraHeight()
    {
        if (!cam) return;

        // pick target camera position based on state
        Vector3 targetCamPos = standCamPos;

        if (isCrouching)
            targetCamPos = crouchCamPos;
        else if (isCrawling)
            targetCamPos = crawlCamPos;

        // smooth transition
        cam.localPosition = Vector3.Lerp(
            cam.localPosition,
            targetCamPos,
            Time.deltaTime * camLerpSpeed
        );
    }

    // LEFT-CLICK to interact
    void Interact()
    {
        if (!cam) { Debug.LogWarning("[Player] Camera reference is missing."); return; }

        // only on click
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = new Ray(cam.position, cam.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Collide))
            return;

        var hitGO = hit.collider ? hit.collider.gameObject : null;
        if (!hitGO) return;

        // collect all MonoBehaviours, filter manually to avoid any Missing Script weirdness
        MonoBehaviour[] monos = hitGO.GetComponentsInParent<MonoBehaviour>(true);

        IInteractable chosen = null;
        PoweredInteractable preferredGate = null;

        for (int i = 0; i < monos.Length; i++)
        {
            var mb = monos[i];
            if (!mb) continue;

            if (mb is PoweredInteractable pi)
                preferredGate = pi; // remember if we find a gate

            if (mb is IInteractable ii && chosen == null)
                chosen = ii; // first interactable as fallback
        }

        // prefer the powered gate if present
        if (preferredGate != null) chosen = preferredGate;

        if (chosen == null)
        {
            Debug.Log($"[Player] No IInteractable found on '{hitGO.name}' (hit collider: {hit.collider.name}).");
            return;
        }

        try
        {
            chosen.Interact(transform);
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex, hitGO);
        }
    }

}
