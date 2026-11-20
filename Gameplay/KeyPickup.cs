using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public Transform keyHolder;
    public AudioClip pickupSound;
    public AudioSource audioSource;
    public GameObject pickupText;

    public bool IsCarried { get; private set; }
    private bool nearby;

    private void Start()
    {
        if (pickupText != null) pickupText.SetActive(false);
    }

    private void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) nearby = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) nearby = false; }

    private void Update()
    {
        if (Time.timeScale <= 0f || EscapeJournal.PlayerHidden || !nearby || IsCarried || keyHolder == null) return;
        EscapeJournal.Current?.Prompt("E   Take the attic key");
        if (!Input.GetKeyDown(KeyCode.E)) return;

        IsCarried = true;
        var body = GetComponent<Rigidbody>();
        if (body != null) body.isKinematic = true;
        foreach (var shape in GetComponentsInChildren<Collider>()) shape.enabled = false;
        transform.SetParent(keyHolder, true);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        if (audioSource != null && pickupSound != null) audioSource.PlayOneShot(pickupSound);
        EscapeJournal.Current?.Advance(EscapeJournal.Stage.Attic);
    }
}
