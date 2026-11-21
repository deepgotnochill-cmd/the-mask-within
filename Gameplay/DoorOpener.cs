using UnityEngine;

public class DoorOpener : MonoBehaviour
{
	public GameObject door;

	public GameObject key;

	public AudioClip doorOpenSound;

	public AudioSource audioSource;

	private bool isOpen;
	private readonly PlayerOverlap overlap = new PlayerOverlap();
	private KeyPickup carriedKey;

	public GameObject texti;

	public GameObject door1;

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			overlap.Enter(other);
			TryUnlock();
			texti.SetActive(!isOpen);
		}
	}

	private void Update() { if (Time.timeScale > 0f && !isOpen && overlap.Any) TryUnlock(); }
	private void TryUnlock() {
		if (isOpen || key == null) return;
		if (carriedKey == null) carriedKey = key.GetComponent<KeyPickup>();
		if (carriedKey != null && carriedKey.IsCarried) OpenDoor();
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player")
		{
			overlap.Exit(other);
			texti.SetActive(!isOpen && overlap.Any);
		}
	}

	private void OpenDoor()
	{
		if (door != null)
		{
			if (doorOpenSound != null) AudioSource.PlayClipAtPoint(doorOpenSound, transform.position, audioSource != null ? audioSource.volume : 1f);
			door.SetActive(value: false);
			isOpen = true;
			texti.SetActive(false);
			door1.SetActive(value: true);
		}
	}
	private void OnDisable() { overlap.Clear(); if (texti != null) texti.SetActive(false); }
}
