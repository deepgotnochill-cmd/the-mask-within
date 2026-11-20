using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
	public string sceneToLoad;

	public float fadeDuration = 1f;

	public CanvasGroup fadeCanvasGroup;

	private bool isFading;

	private void Start()
	{
		fadeCanvasGroup.alpha = 0f;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && !isFading)
		{
			StartCoroutine(FadeAndChangeScene());
		}
	}

	private IEnumerator FadeAndChangeScene()
	{
		isFading = true;
		yield return StartCoroutine(Fade(1f));
		SceneManager.LoadScene(sceneToLoad);
		yield return StartCoroutine(Fade(0f));
		isFading = false;
	}

	private IEnumerator Fade(float targetAlpha)
	{
		float startAlpha = fadeCanvasGroup.alpha;
		float elapsedTime = 0f;
		while (elapsedTime < fadeDuration)
		{
			fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		fadeCanvasGroup.alpha = targetAlpha;
	}
}
