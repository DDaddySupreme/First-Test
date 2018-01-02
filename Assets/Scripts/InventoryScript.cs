using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class InventoryScript : MonoBehaviour
{
    public static InventoryScript Instance;
    //[System.Obsolete("Use item prefabs instead")]
    public static Dictionary<string, Item> Database = new Dictionary<string, Item>();

    // Public party inventory. Item : quantity. Money is the 'Krn' item
    public Dictionary<Item, int> inventory = new Dictionary<Item, int>();

    [Space]
    public Item defaultWeapon;
    public Item currency; // Used for detecting how much currency you have n other scripts. Actual value is heald in inventory
    public int Krn
    {
        get
        {
            return inventory[currency];
        }
        
    }

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        else
        {
            DontDestroyOnLoad(this);
            Instance = this;
        }

        foreach (Item item in Resources.LoadAll<Item>("Item Prefabs"))
        {
            Database.Add(item.name, item);
            Debug.Log(item.name);
        }

        if (!inventory.ContainsKey(currency))
        {
            ModifyInventory(0, currency);
        }
        /*ModifyInventory("BodyFluid", 3);
        ModifyInventory("BrainFluid", 3);
        ModifyInventory("SoulFluid", 3);
        ModifyInventory("LifeJuice", 3);
        ModifyInventory("Rock", 3);
        ModifyInventory("DebugKill", 10);*/

    }

    public List<Item> GetListType(List<Item.ItemType> types)
    {
        List<Item> list = new List<Item>();
        foreach (KeyValuePair<Item, int> pair in inventory)
        {
            foreach (Item.ItemType type in types)
            {
                if (pair.Key.Type == type)
                {
                    list.Add(pair.Key);
                }
            }
        }
        return list;
    }

    // Add or remove items from inventory
    public void ModifyInventory(int num, Item item)
    {
        if (num > 0)
        {
            Debug.Log("Adding " + num + " " + item.name + "(s) to inventory");
            if (LoadInfo.Instance.gameState != LoadInfo.GameState.Battle)
            {
                LoadInfo.Instance.pauseController.SetupNotification(new Notification( "Got " + num + " " + item.name + "(s).", 
                    new string[1] { "Good" }, new UnityEngine.Events.UnityAction[1] { (() => LoadInfo.Instance.pauseController.CloseNotification()) }));

            }
        }
        else if (num < 0)
        {
            Debug.Log("Removing " + num * -1 + " " + item.name + "(s) from inventory");
            if (LoadInfo.Instance.gameState != LoadInfo.GameState.Battle)
            {
                LoadInfo.Instance.pauseController.SetupNotification(new Notification("Got " + num + " " + item.name + "(s).", 
                    new string[1] { "Okay" }, new UnityEngine.Events.UnityAction[1] { (() => LoadInfo.Instance.pauseController.CloseNotification()) }));

            }
        }
        else if (item != currency)
        {
            Debug.LogWarning("Trying to add 0 " + item.name + "(s) to inventory!");
            return;
        }
        else
        {
            Debug.Log("Recieved/lost " + num + " " + currency.name);
        }

        foreach (KeyValuePair<Item, int> obj in inventory)
        {
            if (obj.Key == item)
            {
                inventory[obj.Key] += num;
                if (inventory[obj.Key] <= 0 && item != currency)
                {
                    inventory.Remove(obj.Key);
                }
                return;
            }
        }
        inventory.Add(item, num);
    }

    public void ModifyInventory(int num, string name)
    {
        Item item = null;

        if (Database.ContainsKey(name)) //.Replace(" ", "")))
        {
            item = Database[name]; //.Replace(" ", "")];
        }
        else
        {
            Debug.LogError("Item name (" + name + ") is not in item database");
            return;
        }

        ModifyInventory(num, item);
    }

    // Item class moved to "Item.cs"

}
