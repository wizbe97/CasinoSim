using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameManager", menuName = "Game/Managers/GameManager")]
public class GameManagerSO : ScriptableObject
{
    public PlayerBalanceManagerSO playerBalanceManager;
    public SaveManagerSO saveManager;


    public List<ItemPrefabEntry> itemPrefabs; // List to assign in the Inspector
    private Dictionary<string, GameObject> itemPrefabDictionary; // Actual lookup dictionary

    private void OnEnable()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        itemPrefabDictionary = new Dictionary<string, GameObject>();

        foreach (ItemPrefabEntry entry in itemPrefabs)
        {
            if (!itemPrefabDictionary.ContainsKey(entry.itemID))
            {
                itemPrefabDictionary.Add(entry.itemID, entry.prefab);
            }
            else
            {
                Debug.LogWarning($"Duplicate itemID detected: {entry.itemID}");
            }
        }
    }

    public GameObject GetItemPrefabByID(string itemID)
    {
        if (itemPrefabDictionary == null)
        {
            InitializeDictionary(); // Ensure the dictionary is initialized
        }

        if (itemPrefabDictionary.TryGetValue(itemID, out GameObject prefab))
        {
            return prefab;
        }

        Debug.LogError($"Item prefab not found for itemID: {itemID}");
        return null;
    }

}


[System.Serializable]
public class ItemPrefabEntry
{
    public string itemID;
    public GameObject prefab;
}
