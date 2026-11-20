using UnityEngine;

public class PlayerItemPickup : MonoBehaviour
{
	public Transform holdPosition;

	public float holdDistance = 2f;

	public LayerMask pickableLayer;

	public PlayerFootstepNoise footstepNoise;

	private GameObject heldItem;

	private Rigidbody heldItemRigidbody;

	private Camera playerCamera;

	private void Start()
	{
		playerCamera = Camera.main;
	}

	private void Update()
	{
		if (Time.timeScale <= 0f || EscapeJournal.PlayerHidden) return;
		if (Input.GetMouseButtonDown(0))
		{
			TryPickupItem();
		}
		if (heldItem != null && !Input.GetMouseButton(0))
		{
			DropItem();
		}
		if (heldItem != null)
		{
			MoveHeldItem();
		}
	}

	private void TryPickupItem()
	{
		Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		Debug.DrawRay(ray.origin, ray.direction * holdDistance, Color.red, 2f);
		if (Physics.Raycast(ray, out var hitInfo, holdDistance, pickableLayer))
		{
			Debug.Log("Raycast hit: " + hitInfo.collider.name);
			heldItem = hitInfo.collider.gameObject;
			heldItemRigidbody = heldItem.GetComponent<Rigidbody>();
			if (heldItemRigidbody != null)
			{
				heldItemRigidbody.isKinematic = true;
				Debug.Log("Picked up item: " + heldItem.name);
			}
			else
			{
				Debug.LogWarning("Picked up item has no Rigidbody: " + heldItem.name);
			}
		}
		else
		{
			Debug.Log("No pickable item found in raycast.");
		}
	}

	private void DropItem()
	{
		if (heldItem != null)
		{
			if (heldItemRigidbody != null)
			{
				heldItemRigidbody.isKinematic = false;
			}
			heldItem = null;
			heldItemRigidbody = null;
			Debug.Log("Dropped item.");
		}
	}

	private void MoveHeldItem()
	{
		heldItem.transform.position = holdPosition.position;
		heldItem.transform.rotation = holdPosition.rotation;
	}
}
