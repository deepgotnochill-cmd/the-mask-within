using UnityEngine;

// The shipped bedrooms use a solid bed collider. Park the character at the place
// they entered and move their viewpoint under the frame, so exiting always
// restores a known standing position without forcing the controller into a wall.
public class ClosetHide : MonoBehaviour
{
    public Transform hidePosition;
    public Transform outsidePosition;
    public GameObject promptText;
    public AudioClip hideSound;
    public AudioClip exitSound;
    public AudioSource audioSource;

    private Transform player;
    private CharacterController controller;
    private PlayerController movement;
    private Rigidbody body;
    private Camera eyes;
    private Vector3 eyePosition;
    private Quaternion eyeRotation;
    private bool bodyWasKinematic;
    private bool hidden;
    private bool inZone;
    private bool playerMode;
    private Collider[] beds;
    private Collider nearbyBed;
    private EnemyAI hunter;
    private float blockedUntil;
    private GUIStyle caption;

    private void Start()
    {
        var owner = GameObject.FindGameObjectWithTag("Player");
        if (owner == null) { enabled = false; return; }
        player = owner.transform;
        controller = owner.GetComponent<CharacterController>();
        movement = owner.GetComponent<PlayerController>();
        body = owner.GetComponent<Rigidbody>();
        eyes = owner.GetComponentInChildren<Camera>();
        playerMode = owner == gameObject;
        if (playerMode)
        {
            beds = new[] { FindBed("bed_3"), FindBed("bed_3 (1)") };
            foreach (var candidate in Resources.FindObjectsOfTypeAll<EnemyAI>())
                if (candidate.gameObject.scene == gameObject.scene) { hunter = candidate; break; }
        }
        else if (promptText != null) promptText.SetActive(false);
    }

    private static Collider FindBed(string name)
    {
        var bed = GameObject.Find(name);
        return bed != null ? bed.GetComponent<Collider>() : null;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f || controller == null || movement == null || eyes == null) return;
        if (playerMode && !hidden) nearbyBed = ReachableBed();
        bool near = playerMode ? nearbyBed != null : inZone && hidePosition != null;
        if ((!hidden && !near) || !Input.GetKeyDown(playerMode ? KeyCode.H : KeyCode.E)) return;
        if (hidden) Leave();
        else if (hunter != null && hunter.CanWitnessHiding) blockedUntil = Time.time + 1.6f;
        else Enter();
    }

    private Collider ReachableBed()
    {
        Vector3 origin = controller.bounds.center;
        int mask = Physics.DefaultRaycastLayers & ~(1 << player.gameObject.layer);
        foreach (var bed in beds)
        {
            if (bed == null) continue;
            Vector3 offset = bed.ClosestPoint(origin) - origin;
            if (offset.sqrMagnitude > 1.25f * 1.25f) continue;
            if (!Physics.Raycast(origin, offset.normalized, out var hit, offset.magnitude + 0.08f, mask, QueryTriggerInteraction.Ignore)
                || hit.collider == bed) return bed;
        }
        return null;
    }

    private void Enter()
    {
        hidden = true;
        eyePosition = eyes.transform.localPosition;
        eyeRotation = eyes.transform.localRotation;
        movement.enabled = false;
        controller.enabled = false;
        if (body != null) { bodyWasKinematic = body.isKinematic; body.isKinematic = true; }
        if (playerMode)
        {
            Bounds frame = nearbyBed.bounds;
            Vector3 exitDirection = Vector3.ProjectOnPlane(player.position - frame.center, Vector3.up).normalized;
            if (exitDirection.sqrMagnitude < 0.01f) exitDirection = player.forward;
            Vector3 edge = nearbyBed.ClosestPoint(frame.center + exitDirection * 5f);
            Vector3 viewpoint = edge - exitDirection * 0.22f;
            viewpoint.y = frame.min.y + 0.22f;
            eyes.transform.SetPositionAndRotation(viewpoint, Quaternion.LookRotation(exitDirection, Vector3.up));
        }
        else
            eyes.transform.SetPositionAndRotation(hidePosition.position, hidePosition.rotation);
        if (audioSource != null && hideSound != null) audioSource.PlayOneShot(hideSound);
    }

    private void Leave()
    {
        hidden = false;
        eyes.transform.localPosition = eyePosition;
        eyes.transform.localRotation = eyeRotation;
        if (body != null) body.isKinematic = bodyWasKinematic;
        controller.enabled = true;
        movement.enabled = true;
        if (audioSource != null && exitSound != null) audioSource.PlayOneShot(exitSound);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerMode || !other.CompareTag("Player")) return;
        inZone = true;
        if (promptText != null) promptText.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerMode || !other.CompareTag("Player")) return;
        inZone = false;
        if (promptText != null) promptText.SetActive(false);
    }

    private void OnGUI()
    {
        if (!playerMode || (!hidden && nearbyBed == null) || Time.timeScale <= 0f) return;
        if (caption == null)
        {
            caption = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter };
            caption.normal.textColor = new Color(0.94f, 0.92f, 0.86f);
        }
        float unit = Mathf.Clamp(Screen.height / 720f, 0.7f, 2f);
        caption.fontSize = Mathf.RoundToInt(16f * unit);
        string text = hidden ? "Under the bed   ·   H to get out" :
            Time.time < blockedUntil ? "Break his line of sight first." : "H   ·   Hide under the bed";
        GUI.Box(new Rect(Screen.width / 2f - 145f * unit, 17f * unit, 290f * unit, 34f * unit), text, caption);
    }
}
