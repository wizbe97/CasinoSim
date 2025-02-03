using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InviteCard : MonoBehaviour
{
	public Button Accept;


	public void Deny()
	{
		Destroy(gameObject);
	}
}