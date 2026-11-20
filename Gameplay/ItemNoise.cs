using UnityEngine;

public class ItemNoise : MonoBehaviour
{
	public PlayerFootstepNoise footstepNoise;

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.relativeVelocity.magnitude > 2f)
		{
			footstepNoise.GenerateNoise(base.transform.position);
			Debug.Log("Item hit ground and made noise.");
		}
	}
}
