using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimpleSceneTransition : MonoBehaviour
{
	public string sceneToLoad;

	public Button transitionButton;

	private void Start()
	{
		transitionButton.onClick.AddListener(OnTransitionButtonClick);
	}

	private void OnTransitionButtonClick()
	{
		SceneManager.LoadScene(sceneToLoad);
	}
}
