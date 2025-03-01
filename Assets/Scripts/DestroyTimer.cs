using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTimer : MonoBehaviour
{
	private void Start()
	{
		Invoke("Destroy", 15f);
	}
	private void Destroy()
	{
		Destroy(gameObject);
	}
}