using System.Collections;
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

	private bool isPlayerInZone;

	private bool isCranking;

	private bool isCrankCompleted;

	private float crankTimer;

	private Quaternion initialRotation;
	private readonly PlayerOverlap overlap = new PlayerOverlap();
	private Coroutine returning;
	private float nextNoise;

	public GameObject key;

	public GameObject Text1;

	private void Start()
	{
		initialRotation = crankHandle.rotation;
	}

	private void Update()
	{
		if (Time.timeScale <= 0f) return;
		isPlayerInZone = overlap.Any;
		if (isPlayerInZone && !isCrankCompleted && Input.GetKey(KeyCode.E))
		{
			if (!isCranking)
			{
				isCranking = true;
				if (returning != null) { StopCoroutine(returning); returning = null; }
				crankTimer = 0f;
				Debug.Log("Cranking started");
				audioSource.clip = crankSound;
				audioSource.loop = true;
				audioSource.Play();
			}
			crankTimer += Time.deltaTime;
			float angle = crankRotationSpeed * Time.deltaTime;
			crankHandle.Rotate(rotationAxis, angle);
			if (Time.time >= nextNoise) {
				footstepNoiseEmitter.GenerateNoise(crankHandle.position);
				nextNoise = Time.time + 0.4f;
			}
			if (crankTimer >= crankDuration)
			{
				CompleteCranking();
			}
		}
		else if (isCranking)
		{
			isCranking = false;
			crankTimer = 0f;
			Debug.Log("Cranking interrupted or player left the zone");
			audioSource.Stop();
			ReturnHandle();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			overlap.Enter(other);
			isPlayerInZone = true;
			Debug.Log("Player entered the trigger zone");
			Text1.SetActive(!isCrankCompleted);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			overlap.Exit(other);
			if (overlap.Any) return;
			isPlayerInZone = false;
			isCranking = false;
			crankTimer = 0f;
			Debug.Log("Player left the trigger zone");
			audioSource.Stop();
			if (!isCrankCompleted) ReturnHandle();
			Text1.SetActive(value: false);
		}
	}

	private void CompleteCranking()
	{
		isCranking = false;
		isCrankCompleted = true;
		crankTimer = 0f;
		audioSource.Stop();
		audioSource.PlayOneShot(completionSound);
		objectToActivate.SetActive(value: true);
		Debug.Log("Cranking complete! Object activated.");
		Text1.SetActive(value: false);
		key.SetActive(value: true);
	}

	private IEnumerator ReturnCrankToInitialPosition()
	{
		Quaternion currentRotation = crankHandle.rotation;
		float elapsedTime = 0f;
		float returnDuration = 1f;
		while (elapsedTime < returnDuration)
		{
			crankHandle.rotation = Quaternion.Slerp(currentRotation, initialRotation, elapsedTime / returnDuration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		crankHandle.rotation = initialRotation;
		returning = null;
	}

	private void ReturnHandle() {
		if (returning != null) StopCoroutine(returning);
		returning = StartCoroutine(ReturnCrankToInitialPosition());
	}

	private void OnDisable() {
		overlap.Clear(); isPlayerInZone = false; isCranking = false; crankTimer = 0f;
		if (audioSource != null) audioSource.Stop();
		if (Text1 != null) Text1.SetActive(false);
		StopAllCoroutines(); returning = null;
	}
}
