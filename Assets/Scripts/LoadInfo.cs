using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class LoadInfo : MonoBehaviour { // Holds party info and info for loading levels like where to put the player or what enemies to load into battle

    public static LoadInfo Instance;

    [System.Obsolete("To do: use multiple sprite sheets, and set icons somewhere else")]
    public Sprite[] iconsSpriteSheet;
    public Sprite nullIcon;
    public Sprite unavailableIcon;
    public Sprite checkedIcon;
    public Sprite scrollIcon;
    public Sprite selectedIcon;
    public List<GameObject> partyMembers;
    public GameObject worldPlayer;
    public NpcWorldScript worldEnemy; // The enemy collided with who started battle
    [System.Obsolete("Use hard mode bool instead")]
    public int difficultyLevel = 1;
    public bool hardMode = false;
    public bool pause = false;
    public PauseMenuScript pauseController;
    public Dictionary<GameObject, CharacterScript> scriptDict; // might be useful for taking damage on world screen
    public Dictionary<string, Attack> attacksDatabase = new Dictionary<string, Attack>();
    public Dictionary<string, Trait> traitsDatabase = new Dictionary<string, Trait>();
    public GameState gameState;

    public float playTime = 0; // Time played in seconds

    public enum GameState
    {
        MainMenu,
        World, 
        Battle, 
        Pause,
        Cutscene
    }

    [System.Obsolete]
    public List<GameObject> enemies;

    public Scene returnLevel; // what level to return to when a battle is won
    public Vector3 spawnCoords;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += ResetPosition;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= ResetPosition;
    }

    void Awake () {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            DontDestroyOnLoad(this);
            Instance = this;
        }

        foreach (Attack atk in Resources.LoadAll<Attack>("Attack Prefabs"))
        {

            attacksDatabase.Add
                (atk.name, atk);
        }

        foreach (Trait trait in Resources.LoadAll<Trait>("Trait Prefabs"))
        {

            traitsDatabase.Add
                (trait.name, trait);
        }

        iconsSpriteSheet = Resources.LoadAll<Sprite>("SpriteSheets/Icons");
        scriptDict = new Dictionary<GameObject, CharacterScript>();

        for (int n = 0; n < partyMembers.Count; n++)
        {
            partyMembers[n] = Instantiate(partyMembers[n], transform);
            partyMembers[n].name = partyMembers[n].name.Replace("(Clone)", "");
            DontDestroyOnLoad(partyMembers[n]);

            scriptDict.Add(partyMembers[n], partyMembers[n].GetComponent<CharacterScript>());
        }



	}

	void Update () {
        playTime += Time.deltaTime;

        if (Input.GetButtonDown("Pause"))
        {
            if (Input.GetAxisRaw("Pause") > 0)
            {
                switch (gameState)
                {
                    case GameState.World:
                        pause = true;
                        //SceneManager.LoadScene("PauseMenu", LoadSceneMode.Additive);
                        pauseController.SetupMain();
                        gameState = GameState.Pause;
                        break;

                    case GameState.Pause:
                        //pause = false;
                        //SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("PauseMenu"));
                        pauseController.ClosePauseMenu();
                        gameState = GameState.World;
                        break;

                    case GameState.Battle:

                        break;

                    case GameState.Cutscene:

                        break;
                }
            }
            else //if (Input.GetAxisRaw("Pause") < 0)
            {
                pauseController.ToggleCommand();
                Debug.Log("Opening command input...");
            }
        }
	}

    public void EnterBAttle (GameObject obj)
    {
        returnLevel = SceneManager.GetActiveScene();
        spawnCoords = transform.position;
        worldEnemy = obj.GetComponent<NpcWorldScript>();
        pause = true;

        SceneManager.LoadScene("Battle", LoadSceneMode.Additive);
        Debug.Log("ENTERING BATTLE");
    }

    public void NewGame()
    {
        Debug.LogWarning("Not completely implimented");
        SceneManager.LoadScene("World01");
        pauseController.ClosePauseMenu();
        gameState = GameState.World;
    }

    public void QuitGame ()
    {
        Application.Quit();
    }

    public void SaveGame (int slot)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(Application.dataPath + "/saves/save" + slot + ".save", FileMode.Create);

        SaveData saveData = new SaveData();

        bf.Serialize(stream, saveData);

        stream.Close();
    }

    public void LoadGame (int slot)
    {
        if (File.Exists(Application.dataPath + "/saves/save" + slot + ".save"))
        {
            string finalText = ("<b>LOADING SAVE " + slot);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(Application.dataPath + "/saves/save" + slot + ".save", FileMode.Open);

            SaveData saveData = bf.Deserialize(stream) as SaveData;

            stream.Close();

            foreach (GameObject obj in partyMembers)
            {
                finalText += ("\nDestroying " + obj.name);
                Destroy(obj);
            }
            partyMembers = new List<GameObject>(); //saveData.partyMembers;
            scriptDict = new Dictionary<GameObject, CharacterScript>();

            finalText += ("\n" + saveData.partyMembersData.Length + " party members found in save");
            foreach (CharacterData charData in saveData.partyMembersData)
            {
                GameObject temp = Instantiate(new GameObject(charData.name, typeof(CharacterScript)), transform);
                CharacterScript charScript = temp.GetComponent<CharacterScript>();

                charScript.ImportStats(charData);
                temp.tag = "Player";

                partyMembers.Add(temp);
                scriptDict.Add(temp, charScript);
                finalText += ("\nAdding " + temp.name + " to partyMembers from save");

                temp.SetActive(false);
            }

            hardMode = saveData.hardMode;

            returnLevel = SceneManager.GetSceneByName(saveData.returnLevelName);
            spawnCoords = new Vector3(saveData.spawnCoordsSeperate[0], saveData.spawnCoordsSeperate[1]);

            InventoryScript.Instance.inventory = new Dictionary<Item, int>();
            for (int n = 0; n < saveData.inventoryAmounts.Length; n++)
            {
                InventoryScript.Instance.ModifyInventory(saveData.inventoryAmounts[n], name : saveData.inventoryItems[n]); //inventory.Add(saveData.inventoryItems[n], saveData.inventoryAmounts[n]);
            }

            playTime = saveData.playTime;

            pauseController.ClosePauseMenu();
            gameState = GameState.World;

            SceneManager.LoadScene(saveData.returnLevelName);
            ResetPosition();
            Debug.Log(finalText + "</b>");
        }
        else
        {
            Debug.LogError("Save data not found");
            NewGame();
        }
    }

    public void ResetPosition ()
    {
        worldPlayer.transform.position = spawnCoords;
    }

    public void ResetPosition(Scene scene, LoadSceneMode mode)
    {
        worldPlayer.transform.position = spawnCoords;
    }
}

[System.Serializable]
public class SaveData
{
    public CharacterData[] partyMembersData;
    public bool hardMode;

    public string returnLevelName;
    public float[] spawnCoordsSeperate;

    public string[] inventoryItems;
    public int[] inventoryAmounts;

    public float playTime;

    public SaveData()
    {
        /*partyMembers = new List<CharacterScript>(); //LoadInfo.Instance.partyMembers;
        foreach (GameObject obj in LoadInfo.Instance.partyMembers)
        {
            partyMembers.Add(LoadInfo.Instance.scriptDict[obj]);
        }*/

        partyMembersData = new CharacterData[LoadInfo.Instance.partyMembers.Count];
        for (int p = 0; p < LoadInfo.Instance.partyMembers.Count; p++)
        {
            if (LoadInfo.Instance.partyMembers[p])
            {
                partyMembersData[p] = new CharacterData(LoadInfo.Instance.scriptDict[LoadInfo.Instance.partyMembers[p]]);
            }
            else
            {
                partyMembersData[p] = null;
            }

        }

        hardMode = LoadInfo.Instance.hardMode;

        returnLevelName = SceneManager.GetActiveScene().name; //LoadInfo.Instance.returnLevel.name;

        spawnCoordsSeperate = new float[2];
        spawnCoordsSeperate[0] = LoadInfo.Instance.worldPlayer.transform.position.x;
        spawnCoordsSeperate[1] = LoadInfo.Instance.worldPlayer.transform.position.y;

        inventoryItems = new string[InventoryScript.Instance.inventory.Count];
        inventoryAmounts = new int[InventoryScript.Instance.inventory.Count];

        int n = 0;
        foreach (KeyValuePair<Item, int> pair in InventoryScript.Instance.inventory)
        {
            inventoryItems[n] = pair.Key.name;
            inventoryAmounts[n] = pair.Value;
            n++;
        }

        playTime = LoadInfo.Instance.playTime;
    }
}
