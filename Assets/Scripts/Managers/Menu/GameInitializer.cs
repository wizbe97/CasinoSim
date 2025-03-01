using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    public SaveManagerSO saveManager;

    private void Start()
    {

        saveManager.LoadAllData();          //Load all data
    }

    private void OnApplicationQuit()
    {
        saveManager.AutoSaveAll();          //Save All data
    }
}

