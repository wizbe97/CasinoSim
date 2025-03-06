using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SaveManager", menuName = "Game/Managers/SaveManager")]
public class SaveManagerSO : ScriptableObject
{
    public GameManagerSO gameManager;


    private const string SaveFileBalancePath = "Balance";
    private const string SaveFileItemsPath = "Items";
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

    public void SaveItemData(bool isAutoSave = false)
    {
        CheckAutoSave();

        Item[] items = FindObjectsByType<Item>(FindObjectsSortMode.None);
        List<ItemData> itemDataList = new List<ItemData>();

        foreach (Item item in items)
        {
            itemDataList.Add(new ItemData
            {
                itemID = item.itemID,
                itemPosition = item.transform.position,
                itemRotation = item.transform.eulerAngles
            });
        }

        string json = JsonUtility.ToJson(new ItemDataWrapper { items = itemDataList });
        File.WriteAllText(CombinePath(SaveFileItemsPath, isAutoSave ? 0 : currentSlot), json);
    }




    public void LoadBalance()
    {
        Debug.Log("Loading Balance");
        string path = CombinePath(SaveFileBalancePath, currentSlot);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            BalanceData balanceData = JsonUtility.FromJson<BalanceData>(json);
            gameManager.playerBalanceManager.playerBalance.balance = balanceData.balance;
        }
        else
            StartEmpty();
    }

    public void LoadItemData()
    {
        Debug.Log("Loading Item Positions");
        string path = CombinePath(SaveFileItemsPath, currentSlot);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ItemDataWrapper itemDataWrapper = JsonUtility.FromJson<ItemDataWrapper>(json);

            List<Item> existingItems = FindObjectsByType<Item>(FindObjectsSortMode.None).ToList();

            foreach (ItemData itemData in itemDataWrapper.items)
            {
                // Find an existing item with the same ID
                Item existingItem = existingItems.FirstOrDefault(i => i.itemID == itemData.itemID);

                if (existingItem != null)
                {
                    // Update its position and rotation instead of creating a new one
                    existingItem.transform.position = itemData.itemPosition;
                    existingItem.transform.rotation = Quaternion.Euler(itemData.itemRotation);

                    // Remove from the list so we don’t update it again
                    existingItems.Remove(existingItem);
                }
                else
                {
                    // If no existing item found, instantiate a new one
                    GameObject itemPrefab = gameManager.GetItemPrefabByID(itemData.itemID);
                    if (itemPrefab != null)
                    {
                        GameObject newItem = Instantiate(itemPrefab, itemData.itemPosition, Quaternion.Euler(itemData.itemRotation));
                        Item newItemComponent = newItem.GetComponent<Item>();
                        newItemComponent.itemID = itemData.itemID;
                    }
                }
            }
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
        SaveItemData(isAutoSave: true);

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
        SaveItemData();

        SaveTimestamp(currentSlot);
    }

    public void LoadAllData()
    {
        Debug.Log("LoadAllData Called");  // Debugging line to check if it's running

        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        LoadBalance();
        LoadItemData();
    }


    public bool IsDataSaved(int slot)
    {
        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
        string combinedPath = CombinePath(SaveFileBalancePath, slot);
        return File.Exists(combinedPath);
    }

    public void RemoveSlot(int slot)
    {
        string[] paths =
        {

        CombinePath(SaveFileBalancePath, slot),
        CombinePath(SaveFileItemsPath, slot)
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
        return Path.Combine(saveDirectory, $"{path}_{slot}.json");
    }
}

[Serializable]
public class BalanceData
{
    public int balance;
}

[Serializable]
public class ItemData
{
    public string itemID;
    public Vector3 itemPosition;
    public Vector3 itemRotation;
}


[Serializable]
public class ItemDataWrapper
{
    public List<ItemData> items;
}

