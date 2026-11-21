using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
	public GameObject pauseMenuUI;

	public Button resumeButton;

	public GameObject pl;

	private bool isPaused;

	private void Start()
	{
		pauseMenuUI.SetActive(value: false);
		resumeButton.onClick.AddListener(ResumeGame);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (isPaused)
			{
				ResumeGame();
			}
			else
			{
				PauseGame();
			}
		}
	}

	private void PauseGame()
	{
		pauseMenuUI.SetActive(value: true);
		Time.timeScale = 0f;
		AudioListener.pause = true;
		isPaused = true;
		pl.SetActive(value: false);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	private void ResumeGame()
	{
		pauseMenuUI.SetActive(value: false);
		Time.timeScale = 1f;
		AudioListener.pause = false;
		isPaused = false;
		pl.SetActive(value: true);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
	private void OnDestroy() { Time.timeScale = 1f; AudioListener.pause = false; }
}
