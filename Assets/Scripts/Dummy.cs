using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour {

    // Some scene specific things need references to these static instances in the inspector, so here's a middle man

    public LoadInfo loadInfo;
    PauseMenuScript pauseScript;
    public InventoryScript invScript;

	void Start () {
        loadInfo = LoadInfo.Instance;
        pauseScript = loadInfo.pauseController;
        invScript = InventoryScript.Instance;
	}
	
    public void SaveGame (int slot)
    {
        loadInfo.SaveGame(slot);
    }
    public void LoadGame (int slot)
    {
        loadInfo.LoadGame(slot);
    }
    public void OpenShop (NpcWorldScript obj)
    {
        pauseScript.SetupShop(obj);
    }
    public void CloseDialogue ()
    {
        pauseScript.CloseDialogue();
    }
    public void ClosePauseMenu ()
    {
        pauseScript.ClosePauseMenu();
    }
}
