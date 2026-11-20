using System.Collections;
using UnityEngine;

public class RandomSoundPlayer : MonoBehaviour
{
	public AudioClip[] sounds;

	public float minInterval = 1f;

	public float maxInterval = 5f;

	private float emptyRoomChance = 0.28f;

	private AudioSource audioSource;

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
		if (sounds.Length == 0)
		{
			Debug.LogWarning("No sounds assigned to the RandomSoundPlayer.");
		}
		else
		{
			StartCoroutine(PlayRandomSounds());
		}
	}

	private IEnumerator PlayRandomSounds()
	{
		while (true)
		{
			float seconds = Random.Range(Mathf.Max(0.35f, minInterval * 0.65f), Mathf.Max(maxInterval, minInterval + 0.5f));
			if (Random.value < emptyRoomChance)
			{
				seconds += Random.Range(2.5f, 6f);
			}
			yield return new WaitForSeconds(seconds);
			AudioClip clip = sounds[Random.Range(0, sounds.Length)];
			audioSource.pitch = Random.Range(0.76f, 1.08f);
			audioSource.panStereo = Random.Range(-0.45f, 0.45f);
			audioSource.PlayOneShot(clip, Random.Range(0.38f, 0.92f));
		}
	}
}
