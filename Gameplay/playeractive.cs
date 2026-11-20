using UnityEngine;

public class playeractive : MonoBehaviour
{
	public GameObject play2;

	private void Start()
	{
		play2.SetActive(value: true);
		Time.timeScale = 1f;
	}

	private void Update()
	{
	}
}
