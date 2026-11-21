using UnityEngine;

public class PlayerFootstepNoise : MonoBehaviour
{
	public float noiseRadius = 10f;

	public LayerMask enemyLayer;
	private readonly Collider[] nearby = new Collider[16];

	public void GenerateNoise(Vector3 soundPosition)
	{
		int count = Physics.OverlapSphereNonAlloc(soundPosition, noiseRadius, nearby, enemyLayer);
		EnemyAI previous = null;
		for (int i = 0; i < count; i++)
		{
			var enemy = nearby[i].GetComponent<EnemyAI>();
			if (enemy != null && enemy != previous) enemy.HearSound(soundPosition);
			previous = enemy;
		}
	}
}
