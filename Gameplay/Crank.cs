using UnityEngine;

public class Crank : MonoBehaviour
{
    public GameObject objectToActivate;
    public float crankDuration = 10f;
    public Transform crankHandle;
    public float crankRotationSpeed = 360f;
    public Vector3 rotationAxis = Vector3.forward;
    public AudioSource audioSource;
    public AudioClip crankSound;
    public AudioClip completionSound;
    public PlayerFootstepNoise footstepNoiseEmitter;
    public GameObject key;
    public GameObject Text1;

    private bool nearby;
    private bool turning;
    private bool completed;
    private float progress;
    private float nextNoise;
    private Quaternion rest;

    private void Start()
    {
        if (crankHandle != null) rest = crankHandle.localRotation;
        if (Text1 != null) Text1.SetActive(false);
    }

    private void Update()
    {
        if (completed || Time.timeScale <= 0f) return;
        if (EscapeJournal.PlayerHidden) { StopTurning(); return; }
        if (!nearby || !Input.GetKey(KeyCode.E))
        {
            StopTurning();
            if (nearby) EscapeJournal.Current?.Prompt("Hold E   Turn the stair crank", progress);
            return;
        }

        if (!turning)
        {
            turning = true;
            if (audioSource != null && crankSound != null)
            {
                audioSource.clip = crankSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        float duration = Mathf.Max(0.1f, crankDuration);
        progress = Mathf.Min(1f, progress + Time.deltaTime / duration);
        RotateHandle();
        if (Time.time >= nextNoise)
        {
            if (footstepNoiseEmitter != null) footstepNoiseEmitter.GenerateNoise(transform.position);
            nextNoise = Time.time + 0.65f;
        }
        EscapeJournal.Current?.Prompt("Turning   ·   Release E to let go", progress);
        if (progress < 1f) return;

        completed = true;
        StopTurning();
        if (audioSource != null && completionSound != null) audioSource.PlayOneShot(completionSound);
        if (objectToActivate != null) objectToActivate.SetActive(true);
        if (key != null) key.SetActive(true);
        EscapeJournal.Current?.Advance(EscapeJournal.Stage.Key);
    }

    private void RotateHandle()
    {
        if (crankHandle != null)
            crankHandle.localRotation = rest * Quaternion.AngleAxis(progress * crankDuration * crankRotationSpeed, rotationAxis.normalized);
    }

    private void StopTurning()
    {
        if (!turning) return;
        turning = false;
        if (audioSource != null) { audioSource.Stop(); audioSource.loop = false; }
        // A mechanical ratchet catches after each third of a full turn.
        progress = Mathf.Floor(progress * 3f) / 3f;
        RotateHandle();
    }

    private void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) nearby = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) nearby = false; }
    private void OnDisable() { nearby = false; StopTurning(); }
}
