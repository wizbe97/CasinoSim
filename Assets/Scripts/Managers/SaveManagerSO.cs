using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "SaveManager", menuName = "Game/Managers/SaveManager")]
public class SaveManagerSO : ScriptableObject
{
    public GameManagerSO gameManager;


    private const string SaveFileDogPath = "OwnedDogs";
    private const string SaveFileCatPath = "OwnedCats";
    private const string SaveFileBalancePath = "Balance";
    private string saveDirectory;

    public int currentSlot = 1;


    private void StartEmpty()
    {
        gameManager.playerBalanceManager.ClearBalance();
    }

    public void SaveBalance(bool isAutoSave = false)
    {
        CheckAutoSave();

        string json = JsonUtility.ToJson(new BalanceData { balance = gameManager.playerBalanceManager.playerBalance.balance });
        File.WriteAllText(CombinePath(SaveFileBalancePath, isAutoSave ? 0 : currentSlot), json);
    }

    public void LoadBalance()
    {
        if (File.Exists(CombinePath(SaveFileBalancePath, currentSlot)))
        {
            string json = File.ReadAllText(CombinePath(SaveFileBalancePath, currentSlot));
            BalanceData balanceData = JsonUtility.FromJson<BalanceData>(json);
            gameManager.playerBalanceManager.playerBalance.balance = balanceData.balance;
        }
    }

    private void CheckAutoSave()
    {
        //we make sure we don't override slot 0 when saving manually because slot 0 will be kept for auto saving
        if (currentSlot == 0)
        {
            //loop through slots to see if we have a empty slot to manually save
            for (int i = 1; i < 4; i++)
            {
                if (!IsDataSaved(i))
                {
                    currentSlot = i;
                    break;
                }
            }
        }
    }
    public void AutoSaveAll()
    {

        SaveBalance(isAutoSave: true);

        SaveTimestamp(0); // Slot 0 for autosave
    }

    private void SaveTimestamp(int slot)
    {
        string timestampPath = CombinePath($"Slot_{slot}_time", 0);
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        File.WriteAllText(timestampPath, timestamp);
    }

    public string GetSaveTime(int slot)
    {
        string timestampPath = CombinePath($"Slot_{slot}_time", 0);

        if (File.Exists(timestampPath))
            return File.ReadAllText(timestampPath);
        else
            return "No save time available.";

    }

    public void SaveAllData()
    {

        SaveBalance();

        SaveTimestamp(currentSlot);
    }

    public void LoadAllData()
    {
        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        LoadBalance();
    }

    public bool IsDataSaved(int slot)
    {
        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
        string combinedPath = CombinePath(SaveFileDogPath, slot);
        return File.Exists(combinedPath);
    }

    public void RemoveSlot(int slot)
    {
        string[] paths =
        {
        CombinePath(SaveFileDogPath, slot),
        CombinePath(SaveFileCatPath, slot),
        CombinePath(SaveFileBalancePath, slot)
    };

        foreach (string path in paths)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private string CombinePath(string path, int slot)
    {
        return Path.Combine(saveDirectory, $"{path}_{slot}");
    }
}

[Serializable]
public class BalanceData
{
    public int balance;
}

