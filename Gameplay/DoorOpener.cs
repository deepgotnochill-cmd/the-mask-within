using UnityEngine;

public class DoorOpener : MonoBehaviour
{
    public GameObject door;
    public GameObject key;
    public AudioClip doorOpenSound;
    public AudioSource audioSource;
    public GameObject texti;
    public GameObject door1;
    private KeyPickup lockKey;
    private bool nearby;
    private bool isOpen;

    private void Start()
    {
        if (key != null) lockKey = key.GetComponent<KeyPickup>();
        if (texti != null) texti.SetActive(false);
    }

    private void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) nearby = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) nearby = false; }

    private void Update()
    {
        if (Time.timeScale <= 0f || EscapeJournal.PlayerHidden || !nearby || isOpen) return;
        if (lockKey == null || !lockKey.IsCarried)
        {
            EscapeJournal.Current?.Prompt("Locked   ·   An attic key fits here");
            return;
        }

        EscapeJournal.Current?.Prompt("E   Unlock the attic door");
        if (!Input.GetKeyDown(KeyCode.E) || door == null) return;
        isOpen = true;
        EscapeJournal.Current?.Advance(EscapeJournal.Stage.Escape);
        if (audioSource != null && doorOpenSound != null) audioSource.PlayOneShot(doorOpenSound);
        if (door1 != null) door1.SetActive(true);
        door.SetActive(false);
    }
}
