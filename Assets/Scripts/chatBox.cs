using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatBox : MonoBehaviour
{
	public GameObject _box = null;
	public Transform container;
	public RectTransform chatContent;

	public TMP_InputField _field = null;

	public Text _header;

	public GameObject connected_players;

	public EventSystem eventSystem;
	public GraphicRaycaster graphicRaycaster;
}