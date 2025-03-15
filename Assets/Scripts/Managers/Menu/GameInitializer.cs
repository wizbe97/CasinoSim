using UnityEngine;
using Photon.Pun;


public class GameInitializer : MonoBehaviourPunCallbacks
{
    public SaveManagerSO saveManager;

    private void Start()
    {

        saveManager.LoadAllData();          //Load all data
    }

    private void OnApplicationQuit()
    {
        PhotonNetwork.Disconnect();
        saveManager.AutoSaveAll();          //Save All data
    }
}

