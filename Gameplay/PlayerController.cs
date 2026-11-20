using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public float walkSpeed = 2f;

	public float sprintSpeed = 4f;

	public float crouchSpeed = 1f;

	public float lookSensitivity = 2f;

	public float footstepInterval = 0.5f;

	public AudioClip[] footstepSounds;

	public AudioSource audioSource;

	private CharacterController characterController;

	private PlayerFootstepNoise footstepNoise;

	private float footstepTimer;

	private Vector3 moveDirection;

	private float verticalLookRotation;
	private Camera view;

	public float maxLookAngle = 60f;

	private bool isCrouching;

	private void Start()
	{
		characterController = GetComponent<CharacterController>();
		view = GetComponentInChildren<Camera>();
		footstepNoise = GetComponent<PlayerFootstepNoise>();
		footstepTimer = 0f;
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void Update()
	{
		if (Time.timeScale <= 0f || !characterController.enabled) return;
		LookAround();
		HandleCrouch();
		Move();
		if (!isCrouching && characterController.isGrounded &&
			new Vector2(characterController.velocity.x, characterController.velocity.z).sqrMagnitude > 0.04f)
		{
			footstepTimer += Time.deltaTime;
			if (footstepTimer >= footstepInterval)
			{
				PlayFootstepSound();
				footstepTimer = 0f;
			}
		}
		else
		{
			footstepTimer = 0f;
		}
	}

	private void LookAround()
	{
		float yAngle = Input.GetAxis("Mouse X") * lookSensitivity;
		float num = Input.GetAxis("Mouse Y") * lookSensitivity;
		base.transform.Rotate(0f, yAngle, 0f);
		verticalLookRotation -= num;
		verticalLookRotation = Mathf.Clamp(verticalLookRotation, 0f - maxLookAngle, maxLookAngle);
		if (view != null) view.transform.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
	}

	private void Move()
	{
		float num = walkSpeed;
		if (Input.GetKey(KeyCode.LeftShift) && !isCrouching)
		{
			num = sprintSpeed;
		}
		if (isCrouching)
		{
			num = crouchSpeed;
		}
		float y = moveDirection.y;
		moveDirection = (base.transform.forward * Input.GetAxis("Vertical") + base.transform.right * Input.GetAxis("Horizontal")).normalized * num;
		moveDirection.y = y;
		if (characterController.isGrounded && moveDirection.y < 0f) moveDirection.y = -1f;
		moveDirection.y += Physics.gravity.y * Time.deltaTime;
		characterController.Move(moveDirection * Time.deltaTime);
	}

	private void PlayFootstepSound()
	{
		if (footstepNoise != null) footstepNoise.GenerateNoise(transform.position);
		if (footstepSounds != null && footstepSounds.Length != 0 && audioSource != null)
		{
			AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
			audioSource.PlayOneShot(clip);
		}
	}

	private void HandleCrouch()
	{
		if (Input.GetKey(KeyCode.LeftControl))
		{
			isCrouching = true;
			characterController.height = 1.8f;
		}
		else
		{
			isCrouching = false;
			characterController.height = 2f;
		}
	}
}
