using UnityEngine;

public class CursorScript : MonoBehaviour
{
	private void Start()
	{
		HouseDread.ClearImage(Camera.main);
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}
}
