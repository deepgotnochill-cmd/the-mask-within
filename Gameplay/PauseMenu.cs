using UnityEngine;

public class PauseMenu : MonoBehaviour
{
	private bool isPaused;

	public GameObject pauseMenuPanel;

	public GameObject plauer;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			TogglePauseMenu();
		}
	}

	private void TogglePauseMenu()
	{
		isPaused = !isPaused;
		if (isPaused)
		{
			Time.timeScale = 0f;
			pauseMenuPanel.SetActive(value: true);
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}
		else
		{
			Time.timeScale = 1f;
			pauseMenuPanel.SetActive(value: false);
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
	}
}
