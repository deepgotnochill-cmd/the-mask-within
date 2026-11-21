using System.Collections;
using UnityEngine;

public class triggerEvent : MonoBehaviour
{
	public GameObject object1;

	public GameObject object2;

	public GameObject object3;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			object1.SetActive(value: true);
			object2.SetActive(value: true);
			object3.SetActive(value: false);
			StartCoroutine(ActivateObjectsWithDelay());
		}
	}

	private IEnumerator ActivateObjectsWithDelay()
	{
		yield return new WaitForSeconds(3f);
	}
}
