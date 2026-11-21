using UnityEngine;

public class ClosetHide : MonoBehaviour
{
	public Transform hidePosition;

	public Transform outsidePosition;

	public GameObject promptText;

	public AudioClip hideSound;

	public AudioClip exitSound;

	public AudioSource audioSource;

	private bool isPlayerInZone;

	private bool isHiding;

	private Transform player;

	private CharacterController playerController;

	private void Start()
	{
		promptText.SetActive(value: false);
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerController = player.GetComponent<CharacterController>();
	}

	private void Update()
	{
		if (isPlayerInZone && Input.GetKeyDown(KeyCode.E))
		{
			if (isHiding)
			{
				ExitCloset();
			}
			else
			{
				HideInCloset();
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			isPlayerInZone = true;
			promptText.SetActive(value: true);
			Debug.Log("Player entered the trigger zone");
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			isPlayerInZone = false;
			promptText.SetActive(value: false);
			Debug.Log("Player left the trigger zone");
		}
	}

	private void HideInCloset()
	{
		isHiding = true;
		promptText.SetActive(value: false);
		if (playerController != null)
		{
			playerController.enabled = false;
		}
		player.position = hidePosition.position;
		audioSource.PlayOneShot(hideSound);
		Debug.Log("Player hidden in the closet");
	}

	private void ExitCloset()
	{
		isHiding = false;
		promptText.SetActive(value: true);
		if (playerController != null)
		{
			playerController.enabled = true;
		}
		player.position = outsidePosition.position;
		audioSource.PlayOneShot(exitSound);
		Debug.Log("Player exited the closet");
	}
}
