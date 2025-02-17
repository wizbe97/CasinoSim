using Steamworks;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
	private static bool _initialized;

	public string App_id = 480.ToString();


	private void Awake()
	{
		if (_initialized)
			return;

		//if (SteamAPI.RestartAppIfNecessary((AppId_t)App_id)) // Replace 480 with your App ID
		//{
		//	Application.Quit();
		//	return;
		//}

		if (!SteamAPI.Init())
		{
			Debug.LogError("SteamAPI.Init() failed. Check Steam is running and your App ID is correct.");
			Application.Quit();
			return;
		}

		_initialized = true;
		DontDestroyOnLoad(gameObject);
	}

	private void OnEnable()
	{
		if (_initialized)
			SteamAPI.RunCallbacks();
	}

	void OnApplicationQuit()
	{
		SteamAPI.Shutdown();
	}
}
