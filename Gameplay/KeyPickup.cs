using UnityEngine;

public class KeyPickup : MonoBehaviour
{
	public Transform keyHolder;

	public AudioClip pickupSound;

	public AudioSource audioSource;

	public GameObject pickupText;

	private bool canPickup;

	private GameObject player;

	private GameObject key;
	private readonly PlayerOverlap overlap = new PlayerOverlap();
	public bool IsCarried { get { return keyHolder != null && transform.parent == keyHolder; } }

	private void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player");
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			if (IsCarried) return;
			overlap.Enter(other);
			canPickup = true;
			pickupText.SetActive(value: true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			overlap.Exit(other);
			canPickup = !IsCarried && overlap.Any;
			pickupText.SetActive(canPickup);
		}
	}

	private void Update()
	{
		if (Time.timeScale <= 0f) return;
		canPickup = !IsCarried && overlap.Any;
		if (canPickup && Input.GetKeyDown(KeyCode.E))
		{
			PickupKey();
		}
	}

	private void PickupKey()
	{
		if (IsCarried) return;
		key = gameObject;
		if (key != null)
		{
			key.transform.position = keyHolder.position;
			key.transform.rotation = keyHolder.rotation;
			key.transform.parent = keyHolder;
			pickupText.SetActive(value: false);
			audioSource.PlayOneShot(pickupSound);
			Debug.Log("Key picked up");
			canPickup = false;
			overlap.Clear();
			foreach (var shape in GetComponents<Collider>()) shape.enabled = false;
			var body = GetComponent<Rigidbody>();
			if (body != null) body.isKinematic = true;
		}
	}

	private void OnDisable() { overlap.Clear(); canPickup = false; if (pickupText != null) pickupText.SetActive(false); }
}
