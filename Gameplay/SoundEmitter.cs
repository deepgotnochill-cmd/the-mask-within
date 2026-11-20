using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
	public float soundRadius = 10f;

	private void MakeNoise()
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, soundRadius);
		for (int i = 0; i < array.Length; i++)
		{
			EnemyAI component = array[i].GetComponent<EnemyAI>();
			if (component != null)
			{
				component.HearSound(base.transform.position);
			}
		}
	}
}
