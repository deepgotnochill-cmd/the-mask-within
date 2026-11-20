using UnityEngine;

public class PlayerFootstepNoise : MonoBehaviour
{
	public float noiseRadius = 10f;
	public LayerMask enemyLayer;
	private CharacterController mover;

	private void Awake()
	{
		mover = GetComponent<CharacterController>();
		if (mover != null && CompareTag("Player") && GetComponent<ClosetHide>() == null)
			gameObject.AddComponent<ClosetHide>();
	}

	public void GenerateNoise(Vector3 soundPosition)
	{
		float radius = noiseRadius;
		if (mover != null)
		{
			Vector3 speed = mover.velocity;
			float horizontal = speed.x * speed.x + speed.z * speed.z;
			if (horizontal < 0.04f || !mover.enabled) return;
			radius *= Input.GetKey(KeyCode.LeftShift) ? 1.5f : 0.65f;
		}
		foreach (Collider candidate in Physics.OverlapSphere(soundPosition, radius, enemyLayer))
		{
			var hunter = candidate.GetComponentInParent<EnemyAI>();
			if (hunter != null) hunter.HearSound(soundPosition);
		}
	}
}
