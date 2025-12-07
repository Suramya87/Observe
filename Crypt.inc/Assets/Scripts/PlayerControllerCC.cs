using UnityEngine;

// coconut
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
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

    // ---------- Footstep data ----------
    [System.Serializable]
    public class FootstepSet
    {
        public string name;          // e.g. "Concrete", "Grass"
        public LayerMask layers;     // which layers this set applies to
        public AudioClip[] clips;    // sounds for that surface
    }

    [Header("Footsteps")]
    [Tooltip("Different footsteps per surface type")]
    public FootstepSet[] footstepSets;

    [Tooltip("Seconds between steps at normal walking speed")]
    [Range(0.05f, 1.0f)]
    public float baseStepInterval = 0.4f;

    [Tooltip("Multiplier on step interval while sprinting ( <1 = faster, >1 = slower )")]
    public float sprintStepMultiplier = 0.75f;

    [Tooltip("Multiplier on step interval while crouching ( >1 = slower )")]
    public float crouchStepMultiplier = 1.5f;

    [Tooltip("Multiplier on step interval while crawling ( >1 = slower )")]
    public float crawlStepMultiplier = 2.0f;

    [Tooltip("Minimum movement magnitude to count as 'walking' for sounds")]
    public float minMoveMagnitudeForSteps = 0.1f;

    [Tooltip("Max distance for downward surface raycast")]
    public float footstepRayDistance = 1.5f;

    [Tooltip("Which layers count as ground for surface detection")]
    public LayerMask groundMask = ~0;

    [HideInInspector] public bool movementLocked = false;


    float stepTimer = 0f;
    AudioSource footstepSource;     // now private, auto-grabbed
    // ----------------------------------

    CharacterController cc;
    Vector3 velocity;
    float rotX;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        footstepSource = GetComponent<AudioSource>();

        // optional: make sure it doesn't auto-play anything
        footstepSource.playOnAwake = false;
    }

    void Update()
    {
        if (GamePauseController.IsPaused) return;

            if (!movementLocked)
            {
                Look();
                Move();
            }

        UpdateCameraHeight();
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
        bool isSprinting = Input.GetButton("Sprint") && !isCrouching && !isCrawling;
        float speed = moveSpeed;

        if (isSprinting)
            speed *= sprintMultiplier;
        else if (isCrouching)
            speed *= crouchMultiplier;
        else if (isCrawling)
            speed *= crawlMultiplier;

        Vector3 move = input * speed;
        cc.Move(move * Time.deltaTime);

        // footsteps
        HandleFootsteps(grounded, input, isSprinting);

        // Jump
        if (grounded && Input.GetButtonDown("Jump") && !isCrawling)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }

    void HandleFootsteps(bool grounded, Vector3 input, bool isSprinting)
    {
        if (!grounded)
        {
            stepTimer = 0f;
            return;
        }

        bool isMoving = input.sqrMagnitude > (minMoveMagnitudeForSteps * minMoveMagnitudeForSteps);
        if (!isMoving)
        {
            stepTimer = 0f;
            return;
        }

        float interval = baseStepInterval;

        if (isCrawling)
            interval *= crawlStepMultiplier;
        else if (isCrouching)
            interval *= crouchStepMultiplier;
        else if (isSprinting)
            interval *= sprintStepMultiplier;

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            FootstepSet set = GetCurrentFootstepSet();
            PlayFootstep(set);
            stepTimer = interval;
        }
    }

    FootstepSet GetCurrentFootstepSet()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, footstepRayDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            int surfaceLayerMask = 1 << hit.collider.gameObject.layer;

            for (int i = 0; i < footstepSets.Length; i++)
            {
                FootstepSet set = footstepSets[i];
                if (set != null && (set.layers.value & surfaceLayerMask) != 0)
                    return set;
            }
        }

        // no match → no sound
        return null;
    }

    void PlayFootstep(FootstepSet set)
    {
        if (!footstepSource) return;
        if (set == null || set.clips == null || set.clips.Length == 0) return;

        int index = Random.Range(0, set.clips.Length);
        AudioClip clip = set.clips[index];
        if (clip != null)
            footstepSource.PlayOneShot(clip);
    }

    void UpdateCameraHeight()
    {
        if (!cam) return;

        Vector3 targetCamPos = standCamPos;

        if (isCrouching)
            targetCamPos = crouchCamPos;
        else if (isCrawling)
            targetCamPos = crawlCamPos;

        cam.localPosition = Vector3.Lerp(
            cam.localPosition,
            targetCamPos,
            Time.deltaTime * camLerpSpeed
        );
    }

    void Interact()
    {
        if (!cam) { Debug.LogWarning("[Player] Camera reference is missing."); return; }
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = new Ray(cam.position, cam.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Collide))
            return;

        var hitGO = hit.collider ? hit.collider.gameObject : null;
        if (!hitGO) return;

        MonoBehaviour[] monos = hitGO.GetComponentsInParent<MonoBehaviour>(true);

        IInteractable chosen = null;
        PoweredInteractable preferredGate = null;

        for (int i = 0; i < monos.Length; i++)
        {
            var mb = monos[i];
            if (!mb) continue;

            if (mb is PoweredInteractable pi)
                preferredGate = pi;

            if (mb is IInteractable ii && chosen == null)
                chosen = ii;
        }

        if (preferredGate != null) chosen = preferredGate;

        if (chosen == null)
        {
            Debug.Log($"[Player] No IInteractable found on '{hitGO.name}' (hit collider: {hit.collider.name}).");
            return;
        }

        Debug.Log($"[Player] Calling Interact on {chosen} (type {chosen.GetType().Name})", hitGO);

        try
        {
            chosen.Interact(transform);
        }
        catch (System.ArgumentNullException ane)
        {
            // 🔍 This is the important bit:
            var site = ane.TargetSite;
            string where =
                site == null
                ? "(unknown)"
                : $"{site.DeclaringType?.FullName}.{site.Name}";

            Debug.LogError($"[DEBUG] ArgumentNullException thrown in: {where}\nParameter: {ane.ParamName}\nMessage: {ane.Message}", hitGO);
            Debug.LogException(ane, hitGO);
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex, hitGO);
        }
    }


}
