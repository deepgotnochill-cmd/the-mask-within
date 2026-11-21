using System.Collections;
using UnityEngine;

public class RandomSoundPlayer : MonoBehaviour
{
	public AudioClip[] sounds;

	public float minInterval = 1f;

	public float maxInterval = 5f;

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
			float seconds = Random.Range(minInterval, maxInterval);
			yield return new WaitForSeconds(seconds);
			AudioClip clip = sounds[Random.Range(0, sounds.Length)];
			audioSource.PlayOneShot(clip);
		}
	}
}
