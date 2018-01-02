using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class PauseMenuScript : MonoBehaviour
{
    // To do: orginize all this
    [SerializeField] Canvas actualCanvas;
    [SerializeField] GameObject mainCanvas;
    [SerializeField] GameObject characterCanvas;
    [SerializeField] GameObject techTreeCanvas;
    [SerializeField] GameObject partyCanvas;
    [SerializeField] GameObject equipCanvas;
    [SerializeField] GameObject mapCanvas;
    [SerializeField] GameObject settingsCanvas;
    [SerializeField] GameObject saveLoadCanvas;
    [SerializeField] GameObject dialogueCanvas;
    [SerializeField] GameObject startScreenCanvas;
    [SerializeField] GameObject notificationCanvas;

    public InputField commandInput;
    public Sprite backgroundSprite;
    public Sprite buttonSprite;

    public GameObject[] partyIcons = new GameObject[4];
    public int scrollInt;
    public int buttonNum;
    public int equipNum;
    public int currentDialogue;
    public int shopPriceModifier;
    public int buyNum;
    public List<Dialogue.DialogueMain> dialogue;
    public GameObject selectedCharacter;
    public Attack selectedAttack;
    public List<Trait> traitsList;
    public List<Button> inputList;
    //public List<Item> EquipSlots;
    public List<Item> itemsList;
    public MenuState menuState;
    public InputState inputState;

    public Text displayText;
    public Image displayPortrait;

    private bool skipText;

    public enum MenuState
    {
        None,
        StartScreen,
        Main,
        Character,
        Party,
        Settings,
        Inventory,
        Map,
        Dialogue,
        Shop
    }

    public enum InputState
    {
        Category,
        Attack,
        Item,
        Options
    }

    public enum ButtonState
    {
        Normal,
        Highlighted,
        Disabled,
    }

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------------

    private void Start()
    {
        partyIcons[0] = GameObject.Find("Party Icon 0");
        partyIcons[1] = GameObject.Find("Party Icon 1");
        partyIcons[2] = GameObject.Find("Party Icon 2");
        partyIcons[3] = GameObject.Find("Party Icon 3");
        SetupStartScreen();
        CloseDialogue();

        commandInput.onEndEdit.AddListener(ProcessCommandInput);
    }

    public void ClosePauseMenu()
    {
        menuState = MenuState.None;
        LoadInfo.Instance.gameState = LoadInfo.GameState.World;
        LoadInfo.Instance.pause = false;
        mainCanvas.SetActive(false);
        characterCanvas.SetActive(false);
        techTreeCanvas.SetActive(false);
        partyCanvas.SetActive(false);
        equipCanvas.SetActive(false);
        mapCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        saveLoadCanvas.SetActive(false);
        startScreenCanvas.SetActive(false);
        Debug.Log("Pause menu closed");

        actualCanvas.enabled = false;
        actualCanvas.enabled = true;
    }

    public void SetupStartScreen()
    {
        foreach (Image img in actualCanvas.GetComponentsInChildren<Image>())
        {
            if (img.tag == "CanvasBackground")
            {
                img.sprite = backgroundSprite;
                img.type = Image.Type.Tiled;
            }
            else if (img.tag == "InputButton")
            {
                img.sprite = buttonSprite;
                img.type = Image.Type.Tiled;
            }
        }

        if (commandInput.interactable)
        {
            commandInput.interactable = false;
            commandInput.transform.localPosition = new Vector3(commandInput.transform.localPosition.x, 500f);
        }

        menuState = MenuState.StartScreen;
        LoadInfo.Instance.pause = true;
        mainCanvas.SetActive(false);
        characterCanvas.SetActive(false);
        techTreeCanvas.SetActive(false);
        partyCanvas.SetActive(false);
        equipCanvas.SetActive(false);
        mapCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        saveLoadCanvas.SetActive(false);
        startScreenCanvas.SetActive(true);
        notificationCanvas.SetActive(false);

        actualCanvas.enabled = false;
        actualCanvas.enabled = true;
    }

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void SetupMain()
    {
        foreach (Image img in actualCanvas.GetComponentsInChildren<Image>())
        {
            if (img.tag == "CanvasBackground")
            {
                img.sprite = backgroundSprite;
                img.type = Image.Type.Tiled;
            }
            else if (img.tag == "InputButton")
            {
                img.sprite = buttonSprite;
                img.type = Image.Type.Tiled;
            }
        }

        if (commandInput.interactable)
        {
            commandInput.interactable = false;
            commandInput.transform.localPosition = new Vector3(commandInput.transform.localPosition.x, 500f);
        }

        menuState = MenuState.Main;
        mainCanvas.SetActive(true);
        characterCanvas.SetActive(false);
        partyCanvas.SetActive(false);
        equipCanvas.SetActive(false);
        mapCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        saveLoadCanvas.SetActive(false);
        startScreenCanvas.SetActive(false);

        actualCanvas.enabled = false;
        actualCanvas.enabled = true;

        for (int n = 0; n < partyIcons.Length; n++)
        {
            if (n < LoadInfo.Instance.partyMembers.Count)
            {
                GameObject obj = LoadInfo.Instance.partyMembers[n];
                CharacterScript objScript = LoadInfo.Instance.scriptDict[LoadInfo.Instance.partyMembers[n]];

                if (objScript.portrait != null)
                {
                    partyIcons[n].GetComponent<Image>().sprite = objScript.portrait;
                }
                else
                {
                    partyIcons[n].GetComponent<Image>().sprite = Resources.Load<Sprite>("SpriteSheets/PlaceHolder Portrait");
                }
                partyIcons[n].GetComponentInChildren<Text>().text = (obj.name +
                    "\n\nHP: " + objScript.health + " / " + objScript.maxHealth +
                    "\nMP: " + objScript.magic + " / " + objScript.maxMagic +
                    "\nLVL: " + objScript.level);
            }
            else
            {
                partyIcons[n].GetComponent<Image>().sprite = Resources.Load<Sprite>("SpriteSheets/PlaceHolder Portrait");
                partyIcons[n].GetComponentInChildren<Text>().text = "Null...\n\nHP: # / #\nMP: # / #";
            }

        }

    }

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------------

    public void SetupParty()
    {
        menuState = MenuState.Party;
        mainCanvas.SetActive(false);
        characterCanvas.SetActive(false);
        partyCanvas.SetActive(true);
        equipCanvas.SetActive(false);
        mapCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        saveLoadCanvas.SetActive(false);
        startScreenCanvas.SetActive(false);

        actualCanvas.enabled = false;
        actualCanvas.enabled = true;

        inputList = new List<Button>();
        foreach (Button button in partyCanvas.GetComponentsInChildren<Button>())
        {
            if (button.name.Contains("Party Button "))
            {
                //int temp = Convert.ToInt32(button.name.Replace("Party Button ", ""));
                inputList.Add(button);
                Debug.Log("Adding " + button.name + " to inputList; Index: " + inputList.IndexOf(button));
            }
            else
            {
                Debug.LogWarning("Button has a weird name: " + button.name);
            }
        }

        scrollInt = 0;
        buttonNum = -1;
        UpdateParty();
    }

    public void UpdateParty()
    {
        Debug.Log("Updating party");

        for (int n = 0; n < inputList.Count; n++)
        {
            Text buttonText = inputList[n].GetComponentInChildren<Text>();
            Image buttonIcon = inputList[n].GetComponent<Image>();
            CharacterScript charScript = null;

            inputList[n].interactable = true;
            if (n < 4)
            {
                charScript = LoadInfo.Instance.scriptDict[LoadInfo.Instance.partyMembers[n]];
                buttonIcon.sprite = charScript.portrait ? charScript.portrait : Resources.Load<Sprite>("SpriteSheets/PlaceHolder Portrait");
                buttonText.text = (charScript.gameObject.name + "\nLVL: " + charScript.level);
                switch (charScript.basePosition)
                {
                    case CharacterScript.Position.Back:
                        inputList[n].transform.localPosition = new Vector3(inputList[n].transform.localPosition.x, -175);

                        break;

                    case CharacterScript.Position.Centered:
                        inputList[n].transform.localPosition = new Vector3(inputList[n].transform.localPosition.x, -125);

                        break;

                    case CharacterScript.Position.Front:
                        inputList[n].transform.localPosition = new Vector3(inputList[n].transform.localPosition.x, -75);

                        break;

                    default:
                        inputList[n].transform.localPosition = new Vector3(inputList[n].transform.localPosition.x, -125);

                        break;
                }
            }
            else if ((n == 5 && scrollInt > 0) || (n + scrollInt < LoadInfo.Instance.partyMembers.Count && n == inputList.Count - 1))
            {
                buttonIcon.sprite = null;
                buttonText.text = "(More)";
                Debug.Log("More...");
            }
            else if (n + scrollInt >= LoadInfo.Instance.partyMembers.Count)
            {
                buttonIcon.sprite = null;
                buttonText.text = "";
                inputList[n].interactable = false;

            }
            else
            {
                charScript = LoadInfo.Instance.scriptDict[LoadInfo.Instance.partyMembers[n + scrollInt]];
                buttonIcon.sprite = charScript.portrait;
                buttonText.text = (charScript.gameObject.name + "\nLVL: " + charScript.level);
            }
        }
    }

    public void ProcessPartyInput(int buttonNumber)
    {
        //CharacterScript charScript = null;

        if (buttonNumber < 4)
        {
            //charScript = LoadInfo.Instance.scriptDict[LoadInfo.Instance.partyMembers[buttonNumber]];
            //selectedCharacter = LoadInfo.Instance.partyMembers[buttonNumber];
            if (buttonNum == -1)
            {
                buttonNum = buttonNumber;
            }
            else if (buttonNum == buttonNumber)
            {
                CharacterScript charScript = LoadInfo.Instance.scriptDict[LoadInfo.Instance.partyMembers[buttonNumber]];
                switch (charScript.basePosition)
                {
                    case CharacterScript.Position.Back:
                        charScript.basePosition = CharacterScript.Position.Centered;

                        break;

                    case CharacterScript.Position.Centered:
                        charScript.basePosition = CharacterScript.Position.Front;

                        break;

                    case CharacterScript.Position.Front:
                        charScript.basePosition = CharacterScript.Position.Back;

                        break;

                    default:


                        break;
                }
                buttonNum = -1;
            }
            else
            {
                selectedCharacter = LoadInfo.Instance.partyMembers[buttonNumber];
                LoadInfo.Instance.partyMembers[buttonNumber] = LoadInfo.Instance.partyMembers[buttonNum];
                LoadInfo.Instance.partyMembers[buttonNum] = selectedCharacter;
                selectedCharacter = null;
                buttonNum = -1;
            }
        }
        else if (buttonNumber == 5 && scrollInt > 0)
        {
            scrollInt -= 1;
        }
        else if (buttonNumber + scrollInt < LoadInfo.Instance.partyMembers.Count && buttonNumber == inputList.Count - 1)
        {
            scrollInt += 1;
        }
        else if (buttonNumber + scrollInt > LoadInfo.Instance.partyMembers.Count)
        {
            Debug.LogWarning("Clicked on a button that shouldn't be interactable");
        }
        else
        {
            //charScript = LoadInfo.Instance.scriptDict[LoadInfo.Instance.partyMembers[buttonNumber + scrollInt]];
            //selectedCharacter = LoadInfo.Instance.partyMembers[buttonNumber + scrollInt];
            if (buttonNum == -1)
            {
                buttonNum = buttonNumber + scrollInt;
            }
            else if (buttonNum == buttonNumber + scrollInt)
            {
                CharacterScript charScript = LoadInfo.Instance.scriptDict[LoadInfo.Instance.partyMembers[buttonNumber + scrollInt]];
                switch (charScript.basePosition)
                {
                    case CharacterScript.Position.Back:
                        charScript.basePosition = CharacterScript.Position.Centered;

                        break;

                    case CharacterScript.Position.Centered:
                        charScript.basePosition = CharacterScript.Position.Front;

                        break;

                    case CharacterScript.Position.Front:
                        charScript.basePosition = CharacterScript.Position.Back;

                        break;

                    default:


                        break;
                }
                buttonNum = -1;
            }
            else
            {
                selectedCharacter = LoadInfo.Instance.partyMembers[buttonNumber + scrollInt];
                LoadInfo.Instance.partyMembers[buttonNumber + scrollInt] = LoadInfo.Instance.partyMembers[buttonNum];
                LoadInfo.Instance.partyMembers[buttonNum] = selectedCharacter;
                selectedCharacter = null;
                buttonNum = -1;
            }
        }
        UpdateParty();
    }

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------------

    public void SetupSaveLoad(bool saving)
    {
        Debug.LogWarning("Save and load screen not yet implimented. Defaulting to quick save/load (slot 0)");

        /*if (saving)
        {
            LoadInfo.Instance.SaveGame(0);
        }
        else
        {
            LoadInfo.Instance.LoadGame(0);
        }*/

        menuState = MenuState.Party;
        mainCanvas.SetActive(false);
        characterCanvas.SetActive(false);
        partyCanvas.SetActive(false);
        equipCanvas.SetActive(false);
        mapCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        saveLoadCanvas.SetActive(true);
        startScreenCanvas.SetActive(false);
    }

    public void SelectSlot(int slot)
    {
        Text slotText = GameObject.Find("SlotText").GetComponent<Text>();
        List<Image> imageList = new List<Image>();
        buttonNum = slot;

        foreach (Image img in saveLoadCanvas.GetComponentsInChildren<Image>())
        {
            if (img.tag == "WorldPlayer")
            {
                imageList.Add(img);
            }
        }

        if (LoadInfo.Instance.gameState == LoadInfo.GameState.MainMenu)
        {
            saveLoadCanvas.transform.Find("Save").GetComponent<Button>().interactable = false;
        }
        else
        {
            saveLoadCanvas.transform.Find("Save").GetComponent<Button>().interactable = true;
        }

        if (File.Exists(Application.dataPath + "/saves/save" + slot + ".save"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(Application.dataPath + "/saves/save" + slot + ".save", FileMode.Open);

            SaveData saveData = bf.Deserialize(stream) as SaveData;

            stream.Close();

            slotText.text = "Play time: " + saveData.playTime.ToString();

            for (int n = 0; n < 4; n++)
            {
                Text playerText = imageList[n].GetComponentInChildren<Text>();
                CharacterScript charScript = null;

                if (saveData.partyMembersData[n] != null)
                {
                    charScript = LoadInfo.Instance.scriptDict[LoadInfo.Instance.partyMembers[n]];
                    imageList[n].sprite = charScript.portrait ? charScript.portrait : Resources.Load<Sprite>("SpriteSheets/PlaceHolder Portrait");
                    playerText.text = (charScript.gameObject.name + "\nLVL: " + charScript.level);

                    switch (charScript.basePosition)
                    {
                        case CharacterScript.Position.Back:
                            imageList[n].transform.localPosition = new Vector3(imageList[n].transform.localPosition.x, -175);

                            break;

                        case CharacterScript.Position.Centered:
                            imageList[n].transform.localPosition = new Vector3(imageList[n].transform.localPosition.x, -125);

                            break;

                        case CharacterScript.Position.Front:
                            imageList[n].transform.localPosition = new Vector3(imageList[n].transform.localPosition.x, -75);

                            break;

                        default:
                            imageList[n].transform.localPosition = new Vector3(imageList[n].transform.localPosition.x, -125);

                            break;
                    }
                }
                else
                {
                    charScript = LoadInfo.Instance.scriptDict[LoadInfo.Instance.partyMembers[n]];
                    imageList[n].sprite = charScript.portrait ? charScript.portrait : Resources.Load<Sprite>("SpriteSheets/PlaceHolder Portrait");
                    playerText.text = (charScript.gameObject.name + "\nLVL: " + charScript.level);

                    imageList[n].transform.localPosition = new Vector3(imageList[n].transform.localPosition.x, -125);
                }
            }
        }
        else
        {
            slotText.text = "No save data. Click 'load' to start a new game";
        }
    }

    public void SaveLoad (bool saving)
    {
        if (saving)
        {
            LoadInfo.Instance.SaveGame(buttonNum);
        }
        else
        {
            LoadInfo.Instance.LoadGame(buttonNum);
        }
    }

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------------

    public void SetupSettings(bool mainMenu)
    {
        Debug.LogWarning("Settings not yet implimented");
    }

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------------

    public void SetupItems(int slot) //List<ItemType> types)
    {
        inputList = new List<Button>();
        foreach (Button button in equipCanvas.GetComponentsInChildren<Button>())
        {
            inputList.Add(button);
        }

        //menuState = MenuState.Character;
        equipCanvas.SetActive(true);
        inputState = InputState.Item;
        scrollInt = 0;
        actualCanvas.enabled = false;
        actualCanvas.enabled = true;
        switch (slot)
        {
            default:
            case 0:

                itemsList = InventoryScript.Instance.GetListType(new List<Item.ItemType>() {
                    Item.ItemType.Accessory,
                    Item.ItemType.Armour,
                    Item.ItemType.Consumable,
                    Item.ItemType.Key,
                    Item.ItemType.TwoHandedWeapon,
                    Item.ItemType.Weapon });
                menuState = MenuState.Inventory;
                break;

            case 1: // Right weapon
                equipNum = 0;
                itemsList = InventoryScript.Instance.GetListType(new List<Item.ItemType>() {
                    Item.ItemType.Weapon,
                    Item.ItemType.TwoHandedWeapon });
                break;

            case 2: // Left weapon
                equipNum = 1;
                itemsList = InventoryScript.Instance.GetListType(new List<Item.ItemType>() {
                    Item.ItemType.Weapon });
                break;

            case 3: // Armour
                equipNum = 2;
                itemsList = InventoryScript.Instance.GetListType(new List<Item.ItemType>() {
                    Item.ItemType.Armour });
                break;

            case 4: // Accessory
                equipNum = 3;
                itemsList = InventoryScript.Instance.GetListType(new List<Item.ItemType>() {
                    Item.ItemType.Accessory });
                break;

            case 5:

                itemsList = InventoryScript.Instance.GetListType(new List<Item.ItemType>() {
                    Item.ItemType.Key });
                break;
        }

        // Display items on buttons

        UpdateEquipInput();
    }

    public void SetupAttacks(int slot) // Slot is unused
    {
        inputList = new List<Button>();
        foreach (Button button in equipCanvas.GetComponentsInChildren<Button>())
        {
            inputList.Add(button);
        }

        //menuState = MenuState.Character;
        equipCanvas.SetActive(true);
        inputState = InputState.Attack;
        CharacterScript charScript = selectedCharacter.GetComponent<CharacterScript>();

        actualCanvas.enabled = false;
        actualCanvas.enabled = true;

        traitsList = new List<Trait>();
        foreach (Trait trait in charScript.unlockedTraits)
        {
            traitsList.Add(trait);
        }
        /*foreach (Item item in new List<Item>() { charScript.weaponR, charScript.weaponL, charScript.armour, charScript.accesory })
        {
            if (item != null)
            {
                foreach (Attack atk in item.attacksList)
                {
                    if (traitsList.Contains(atk))
                    {
                        // To do: decide what to do if you've got multiple of the same attack equiped. Make it nore powerful maybe? Or do nothing? Probably nothing.
                    }
                    else
                    {
                        traitsList.Add(atk);
                    }
                }
            }

        }*/

        // Display items on buttons
        UpdateEquipInput();
    }

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ProcessEquipInput(int buttonNumber)
    {
        CharacterScript charScript = null;
        if (menuState != MenuState.Shop && menuState != MenuState.Inventory)
        {
            charScript = LoadInfo.Instance.scriptDict[selectedCharacter];
        }
        else
        {

        }
        buttonNum = buttonNumber;

        switch (inputState)
        {
            case InputState.Item:


                if (buttonNumber == 0 && scrollInt > 0)
                {
                    scrollInt -= 1;
                    UpdateEquipInput();
                }

                else if (buttonNumber == inputList.Count - 1 && buttonNumber < itemsList.Count - 1)
                {
                    scrollInt += 1;
                    UpdateEquipInput();
                }
                else if (buttonNumber + scrollInt == itemsList.Count && menuState != MenuState.Shop && menuState != MenuState.Inventory)
                {
                    //equipCanvas.SetActive(false);
                    switch (equipNum)
                    {
                        case 0:
                            if (charScript.weaponR != null)
                            {
                                InventoryScript.Instance.ModifyInventory(1, charScript.weaponR);
                            }

                            charScript.weaponR = null;
                            break;
                        case 1:
                            if (charScript.weaponL != null)
                            {
                                InventoryScript.Instance.ModifyInventory(1, charScript.weaponL);
                            }

                            charScript.weaponL = null;
                            break;
                        case 2:
                            if (charScript.armour != null)
                            {
                                InventoryScript.Instance.ModifyInventory(1, charScript.armour);
                            }

                            charScript.armour = null;
                            break;
                        case 3:
                            if (charScript.accesory != null)
                            {
                                InventoryScript.Instance.ModifyInventory(1, charScript.accesory);
                            }

                            charScript.accesory = null;
                            break;
                    }

                    SetupCharacterCanvas(LoadInfo.Instance.partyMembers.IndexOf(selectedCharacter));

                }
                else if (itemsList[buttonNumber].Type == Item.ItemType.Key)
                {
                    // Might change this
                }
                else if (menuState == MenuState.Character || menuState == MenuState.Shop || menuState == MenuState.Inventory)
                {
                    // Display description and confirm/deny buttons
                    SetupConfirmation(itemsList[buttonNumber + scrollInt].name + "\n\n" +
                        itemsList[buttonNumber + scrollInt].Description);

                }

                break;

            case InputState.Attack:

                if (buttonNumber == 0 && scrollInt > 0)
                {
                    scrollInt -= 1;
                    UpdateEquipInput();
                }

                else if (buttonNumber == inputList.Count - 1 && buttonNumber + scrollInt < itemsList.Count - 1)
                {
                    scrollInt += 1;
                    UpdateEquipInput();
                }
                /*else if (buttonNumber + scrollInt == itemsList.Count)
                {


                    SetupCharacterCanvas(LoadInfo.Instance.partyMembers.IndexOf(selectedCharacter));

                }*/
                else if (menuState == MenuState.Character)
                {
                    // Display description and confirm/deny buttons
                    SetupConfirmation(traitsList[buttonNumber + scrollInt].name + "\n\n" +
                        traitsList[buttonNumber + scrollInt].description);
                    // "\nMP required: " + traitsList[buttonNumber + scrollInt].magicRequired + 
                }
                break;
        }
    }

    public void UpdateEquipInput()
    {
        // Set inputList
        inputList = new List<Button>();
        foreach (Button button in equipCanvas.GetComponentsInChildren<Button>())
        {
            /*if (button.name == ("Button "))
            {
                
            }
            if (button.name == "Confirm") //|| button.name == "Deny")
            {
                button.interactable = false;
            }
            else
            {
                inputList.Add(button);
            }*/
            inputList.Add(button);
        }

        foreach (Text txt in equipCanvas.GetComponentsInChildren<Text>())
        {
            if (txt.name == "BigText")
            {
                txt.text = "";
            }
        }

        for (int n = 0; n < inputList.Count; n++)
        {
            Text buttonText = inputList[n].GetComponentInChildren<Text>();
            Image buttonIcon = null;
            foreach (Image img in inputList[n].GetComponentsInChildren<Image>())
            {
                if (img.tag == "Icon")
                {
                    buttonIcon = img;
                    break;
                }
            }

            if (inputList[n].name == "Confirm")
            {
                buttonText.text = "";
                inputList[n].interactable = false;
                continue;
            }
            else if (inputList[n].name == "Deny")
            {
                buttonText.text = "Cancel";
                continue;
            }

            buttonText.text = "";

            switch (inputState)
            {
                case InputState.Item:

                    inputList[n].interactable = true;
                    Debug.Log("N: " + n);
                    buttonIcon.color = new Color(255, 255, 255, 255);
                    Debug.Log("Step 1");
                    if ((n == 0 && scrollInt > 0) || (n + scrollInt == inputList.Count - 1 && n + scrollInt < itemsList.Count - 1))
                    {
                        buttonText.text = "More... ";
                        buttonIcon.sprite = LoadInfo.Instance.scrollIcon; //iconsSpriteSheet[57];
                    }
                    else if (n + scrollInt > itemsList.Count)
                    {
                        inputList[n + scrollInt].interactable = false;
                        buttonText.text = "";
                        buttonIcon.color = new Color(255, 255, 255, 0);
                        buttonIcon.sprite = LoadInfo.Instance.unavailableIcon; //iconsSpriteSheet[59];
                        // To do: make sprite transparent (EZ)
                    }
                    else if (n + scrollInt == itemsList.Count)
                    {
                        buttonText.text = "None";
                        buttonIcon.sprite = LoadInfo.Instance.unavailableIcon; //iconsSpriteSheet[59];
                    }
                    else //if ((n == 0 && scrollInt <= 0) && (n + scrollInt != inputList.Count - 1 && n + scrollInt < itemsList.Count - 1))
                    {
                        Debug.Log("Step 2");
                        if (menuState == MenuState.Shop)
                        {
                            buttonText.text = (itemsList[n + scrollInt].name + " : " + ((float)itemsList[n + scrollInt].Value * shopPriceModifier / 100f));
                            buttonIcon.sprite = itemsList[n + scrollInt].Icon;
                            Debug.Log("Setting up shop with " + itemsList.Count + " items.");
                        }
                        else
                        {
                            buttonText.text = (itemsList[n + scrollInt].name + " : " + InventoryScript.Instance.inventory[itemsList[n + scrollInt]]);
                            buttonIcon.sprite = itemsList[n + scrollInt].Icon;
                        }

                    }
                    break;

                case InputState.Attack:
                    inputList[n].interactable = true;
                    buttonIcon.color = new Color(255, 255, 255, 255);

                    if ((n == 0 && scrollInt > 0) || (n + scrollInt == inputList.Count - 1 && n + scrollInt < traitsList.Count - 1))
                    {
                        buttonText.text = "More... ";
                        buttonIcon.sprite = LoadInfo.Instance.scrollIcon; //iconsSpriteSheet[57];
                    }
                    else if (n + scrollInt >= traitsList.Count)
                    {
                        inputList[n + scrollInt].interactable = false;
                        buttonText.text = "";
                        buttonIcon.color = new Color(255, 255, 255, 0);
                        buttonIcon.sprite = LoadInfo.Instance.unavailableIcon; //iconsSpriteSheet[59];
                    }
                    /*else if (n + scrollInt == attackList.Count)
                    {
                        buttonText.text = "None";
                        buttonIcon.sprite = LoadInfo.Instance.iconsSpriteSheet[59];
                    }*/
                    else //if ((n == 0 && scrollInt <= 0) && (n + scrollInt != inputList.Count - 1 && n + scrollInt < attackList.Count - 1))
                    {
                        buttonText.text = (traitsList[n + scrollInt].name + " : " + traitsList[n + scrollInt].equipPoints);
                        buttonIcon.sprite = traitsList[n + scrollInt].icon;
                        if (selectedCharacter.GetComponent<CharacterScript>().equippedTraits.Contains(traitsList[n + scrollInt]))
                        {
                            buttonText.text += " \n(Equipped)";
                        }
                        else if ((selectedCharacter.GetComponent<CharacterScript>().equipPointsMax - selectedCharacter.GetComponent<CharacterScript>().equipPointsUsed) < traitsList[n + scrollInt].equipPoints)
                        {
                            inputList[n].interactable = false;
                        }
                    }
                    break;
            }
        }
    }

    public void SetupConfirmation(string text)
    {
        foreach (Text txt in equipCanvas.GetComponentsInChildren<Text>())
        {
            if (txt.name == "BigText")
            {
                txt.text = text;
            }
        }
        foreach (Button button in equipCanvas.GetComponentsInChildren<Button>())
        {
            if (button.name == "Confirm") //|| button.name == "Deny")
            {
                button.interactable = true;

                switch (inputState)
                {
                    default:

                        button.GetComponentInChildren<Text>().text = "Equip";
                        break;
                    case InputState.Attack:

                        CharacterScript charScript = selectedCharacter.GetComponent<CharacterScript>();
                        /*if (!charScript.availableAttacks.Contains(traitsList[buttonNum + scrollInt]))
                        {
                            // Can't unequip attacks from items. To do: make it impossible to unequip item attacks, even if they're unlocked in tech tree
                            button.interactable = false;
                            button.GetComponentInChildren<Text>().text = "(item)";
                            //Debug.Log("Attack added by item... " + attacksList[buttonNum + scrollInt].name);
                        }
                        else */if (!charScript.equippedTraits.Contains(traitsList[buttonNum + scrollInt]) && charScript.equipPointsMax - charScript.equipPointsUsed < traitsList[buttonNum + scrollInt].equipPoints)
                        {
                            button.interactable = false;
                            button.GetComponentInChildren<Text>().text = "(not enough equip points)";
                        }
                        else if (!charScript.equippedTraits.Contains(traitsList[buttonNum + scrollInt]))
                        {
                            button.GetComponentInChildren<Text>().text = "Equip";
                            //Debug.Log("Attack is not equipped... " + attacksList[buttonNum + scrollInt].name);

                        }
                        else if (charScript.equippedTraits.Contains(traitsList[buttonNum + scrollInt]))
                        {
                            button.GetComponentInChildren<Text>().text = "Unequip";
                            //Debug.Log("Attack is equipped... " + attacksList[buttonNum + scrollInt].name);

                        }
                        break;

                    case InputState.Item:

                        switch (menuState)
                        {
                            case MenuState.Character:


                                break;

                            case MenuState.Shop:

                                if (InventoryScript.Instance.Krn >= (int)(itemsList[buttonNum + scrollInt].Value * shopPriceModifier / 100f))
                                {
                                    button.GetComponentInChildren<Text>().text = "Buy";
                                }
                                else
                                {
                                    button.interactable = false;
                                    button.GetComponentInChildren<Text>().text = "Can't afford";
                                }
                                break;

                            case MenuState.Inventory:

                                button.GetComponentInChildren<Text>().text = "Use";
                                break;
                        }
                        break;
                }
            }
        }


    }

    public void ConfirmEquipInput()
    {
        CharacterScript charScript = null;

        if (selectedCharacter != null)
        {
            charScript = LoadInfo.Instance.scriptDict[selectedCharacter];
        }

        foreach (Button button in equipCanvas.GetComponentsInChildren<Button>())
        {
            if (button.name == "Confirm") //|| button.name == "Deny")
            {
                button.interactable = false;
            }
        }
        switch (inputState)
        {
            case InputState.Item:


                if (buttonNum == 0 && scrollInt > 0)
                {
                    scrollInt -= 1;
                    UpdateEquipInput();
                }

                else if (buttonNum == inputList.Count - 1 && buttonNum < itemsList.Count - 1)
                {
                    scrollInt += 1;
                    UpdateEquipInput();
                }
                else if (buttonNum + scrollInt == itemsList.Count && menuState != MenuState.Shop && menuState != MenuState.Inventory)
                {
                    //equipCanvas.SetActive(false);
                    switch (equipNum)
                    {
                        case 0:
                            if (charScript.weaponR != null)
                            {
                                InventoryScript.Instance.ModifyInventory(1, charScript.weaponR);
                            }

                            charScript.weaponR = null;
                            break;
                        case 1:
                            if (charScript.weaponL != null)
                            {
                                InventoryScript.Instance.ModifyInventory(1, charScript.weaponL);
                            }

                            charScript.weaponL = null;
                            break;
                        case 2:
                            if (charScript.armour != null)
                            {
                                InventoryScript.Instance.ModifyInventory(1, charScript.armour);
                            }

                            charScript.armour = null;
                            break;
                        case 3:
                            if (charScript.accesory != null)
                            {
                                InventoryScript.Instance.ModifyInventory(1, charScript.accesory);
                            }

                            charScript.accesory = null;
                            break;
                    }

                    SetupCharacterCanvas(LoadInfo.Instance.partyMembers.IndexOf(selectedCharacter));

                }
                else if (itemsList[buttonNum].Type == Item.ItemType.Key)
                {
                    // Might change this
                }
                else if (menuState == MenuState.Character)
                {
                    // Equip item
                    switch (equipNum)
                    {
                        case 0:
                            if (charScript.weaponR != null)
                            {
                                InventoryScript.Instance.ModifyInventory(1, charScript.weaponR);
                            }

                            if (itemsList[buttonNum + scrollInt].Type == Item.ItemType.TwoHandedWeapon)
                            {
                                if (charScript.weaponL != null)
                                {
                                    InventoryScript.Instance.ModifyInventory(1, charScript.weaponL);
                                }

                                charScript.weaponL = null;
                            }

                            charScript.weaponR = itemsList[buttonNum + scrollInt];
                            InventoryScript.Instance.ModifyInventory(-1, charScript.weaponR);
                            break;

                        case 1:
                            if (charScript.weaponL != null)
                            {
                                InventoryScript.Instance.ModifyInventory(1, charScript.weaponL);
                            }

                            charScript.weaponL = itemsList[buttonNum + scrollInt];
                            InventoryScript.Instance.ModifyInventory(-1, charScript.weaponL);
                            break;

                        case 2:

                            if (charScript.armour != null)
                            {
                                InventoryScript.Instance.ModifyInventory(1, charScript.armour);

                                /*foreach (Attack attack in charScript.armour.attacksList)
                                {
                                    if (charScript.equippedAttacks.Contains(attack) && !charScript.availableAttacks.Contains(attack) && !charScript.accesory.attacksList.Contains(attack))
                                    {
                                        charScript.equippedAttacks.Remove(attack);
                                    }
                                }*/
                            }

                            /*foreach (Attack attack in itemsList[buttonNum + scrollInt].attacksList)
                            {
                                if (!charScript.equippedAttacks.Contains(attack))
                                {
                                    charScript.equippedAttacks.Add(attack);
                                }
                            }*/

                            charScript.armour = itemsList[buttonNum + scrollInt];
                            InventoryScript.Instance.ModifyInventory(-1, charScript.armour);
                            break;

                        case 3:
                            if (charScript.accesory != null)
                            {
                                InventoryScript.Instance.ModifyInventory(1, charScript.accesory);

                                /*foreach (Attack attack in charScript.accesory.attacksList)
                                {
                                    if (charScript.equippedAttacks.Contains(attack) && !charScript.availableAttacks.Contains(attack) && !charScript.accesory.attacksList.Contains(attack))
                                    {
                                        charScript.equippedAttacks.Remove(attack);
                                    }
                                }*/
                            }

                            /*foreach (Attack attack in itemsList[buttonNum + scrollInt].attacksList)
                            {
                                if (!charScript.equippedAttacks.Contains(attack))
                                {
                                    charScript.equippedAttacks.Add(attack);
                                }
                            }*/

                            charScript.accesory = itemsList[buttonNum + scrollInt];
                            InventoryScript.Instance.ModifyInventory(-1, charScript.accesory);
                            break;
                    }

                    SetupCharacterCanvas(LoadInfo.Instance.partyMembers.IndexOf(selectedCharacter));
                    inputState = InputState.Category;


                }
                else if (menuState == MenuState.Shop)
                {
                    //InventoryScript.Instance.ModifyInventory((int)(itemsList[buttonNum + scrollInt].Value * shopPriceModifier / -100f), InventoryScript.Instance.currency);
                    //InventoryScript.Instance.ModifyInventory(1, itemsList[buttonNum + scrollInt]);
                    SetupShopAmount();
                }
                else if (menuState == MenuState.Inventory)
                {
                    // Get list of party members
                }
                else
                {
                    Debug.LogWarning("Something went wrong");

                }

                break;

            case InputState.Attack:

                if (menuState == MenuState.Character)
                {
                    if (!charScript.equippedTraits.Contains(traitsList[buttonNum + scrollInt]))
                    {
                        // Equip
                        charScript.equipPointsUsed += traitsList[buttonNum + scrollInt].equipPoints;
                        charScript.equippedTraits.Add(traitsList[buttonNum + scrollInt]);
                    }
                    else //if (charScript.equippedTraits.Contains(traitsList[buttonNum + scrollInt]))
                    {
                        // Unequip
                        charScript.equipPointsUsed -= traitsList[buttonNum + scrollInt].equipPoints;
                        charScript.equippedTraits.Remove(traitsList[buttonNum + scrollInt]);
                    }

                }
                UpdateEquipInput();
                SetupConfirmation(traitsList[buttonNum + scrollInt].name + "\n\n" +
                        traitsList[buttonNum + scrollInt].description);
                // + "\nMP required: " + traitsList[buttonNum + scrollInt].magicRequired 
                break;
        }
    }

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void CloseEquips() // For the cancel button
    {
        Debug.Log("Trying to close equips screen...");

        switch (menuState)
        {
            case MenuState.Shop:

                ClosePauseMenu();
                break;

            case MenuState.Character:
            default:

                equipCanvas.SetActive(false);
                techTreeCanvas.SetActive(false);
                /*if (selectedCharacter != null)
                {
                    SetupCharacterCanvas(LoadInfo.Instance.partyMembers.IndexOf(selectedCharacter));
                }
                else
                {
                    Debug.LogWarning("Trying to return to character canvas with no selected character! Trying method two...");
                    equipCanvas.SetActive(false);
                    techTreeCanvas.SetActive(false);
                }*/
                break;
        }

    }

    public void SetupCharacterCanvas(int c)
    {
        selectedCharacter = LoadInfo.Instance.partyMembers[c];

        menuState = MenuState.Character;
        mainCanvas.SetActive(true);
        characterCanvas.SetActive(true);
        techTreeCanvas.SetActive(false);
        partyCanvas.SetActive(false);
        equipCanvas.SetActive(false);
        mapCanvas.SetActive(false);
        settingsCanvas.SetActive(false);

        actualCanvas.enabled = false;
        actualCanvas.enabled = true;

        CharacterScript charScript = LoadInfo.Instance.scriptDict[selectedCharacter];

        if (charScript.portrait != null)
        {
            GameObject.Find("Portrait").GetComponent<Image>().sprite = charScript.portrait;
        }
        else
        {
            GameObject.Find("Portrait").GetComponent<Image>().sprite = Resources.Load<Sprite>("SpriteSheets/PlaceHolder Portrait");
        }

        GameObject.Find("Name").GetComponent<Text>().text = selectedCharacter.name;
        GameObject.Find("MainStats").GetComponent<Text>().text = (
                    "HP: " + charScript.health + " / " + charScript.maxHealth +
                    "\nMP: " + charScript.magic + " / " + charScript.maxMagic +
                    "\nLVL: " + charScript.level);

        GameObject.Find("Stats").GetComponent<Text>().text = (
            "Strength: " + charScript.strength + " / " + charScript.baseStrength +
            "\nConstitution: " + charScript.resistance + " / " + charScript.resistance +

            "\nIntelligence: " + charScript.intelligence + " / " + charScript.baseIntelligence +
            "\nConstitution: " + charScript.spirit + " / " + charScript.baseSpirit +

            "\nPersistance: " + charScript.persistance + " / " + charScript.basePersistance +
            "\nImmunity: " + charScript.immunity + " / " + charScript.baseImmunity +

            "\nSpeed: " + charScript.speed + " / " + charScript.baseSpeed +
            "\nLuck: " + charScript.luck + " / " + charScript.baseLuck);

        // To do: show status effects, or replace status textbox
        foreach (Image obj in GameObject.Find("R weapon").GetComponentsInChildren<Image>())
        {
            if (obj.tag == "Item" && charScript.weaponR != null)
            {
                obj.sprite = charScript.weaponR.Icon;
                GameObject.Find("R weapon").GetComponentInChildren<Text>().text = (charScript.weaponR.name + "\nLVL: " + charScript.weaponR.Level);
            }
            else if (obj.tag == "Item")
            {
                obj.sprite = LoadInfo.Instance.nullIcon; //iconsSpriteSheet[58];
                GameObject.Find("R weapon").GetComponentInChildren<Text>().text = ("No right weapon equipped");
            }
        }

        foreach (Image obj in GameObject.Find("L weapon").GetComponentsInChildren<Image>())
        {
            if (obj.tag == "Item" && charScript.weaponL != null)
            {
                obj.sprite = charScript.weaponL.Icon;
                GameObject.Find("L weapon").GetComponentInChildren<Text>().text = (charScript.weaponL.name + "\nLVL: " + charScript.weaponL.Level);
            }
            else if (obj.tag == "Item")
            {
                obj.sprite = LoadInfo.Instance.nullIcon; //iconsSpriteSheet[58];
                GameObject.Find("L weapon").GetComponentInChildren<Text>().text = ("No left weapon equipped");
            }

            if (charScript.weaponR != null)
            {
                if (charScript.weaponR.Type == Item.ItemType.TwoHandedWeapon)
                {
                    GameObject.Find("L weapon").GetComponent<Button>().interactable = false;
                    GameObject.Find("L weapon").GetComponentInChildren<Text>().text = ("(Two handed weapon)");
                }
                else
                {
                    GameObject.Find("L weapon").GetComponent<Button>().interactable = true;
                }
            }
            else
            {
                GameObject.Find("L weapon").GetComponent<Button>().interactable = true;
            }

        }

        foreach (Image obj in GameObject.Find("Armour").GetComponentsInChildren<Image>())
        {
            if (obj.tag == "Item" && charScript.armour != null)
            {
                obj.sprite = charScript.armour.Icon;
                GameObject.Find("Armour").GetComponentInChildren<Text>().text = (charScript.armour.name + "\nLVL: " + charScript.armour.Level);
            }
            else if (obj.tag == "Item")
            {
                obj.sprite = LoadInfo.Instance.nullIcon; //iconsSpriteSheet[58];
                GameObject.Find("Armour").GetComponentInChildren<Text>().text = ("No armour equipped");
            }
        }

        foreach (Image obj in GameObject.Find("Accessory").GetComponentsInChildren<Image>())
        {
            if (obj.tag == "Item" && charScript.accesory != null)
            {
                obj.sprite = charScript.accesory.Icon;
                GameObject.Find("Accessory").GetComponentInChildren<Text>().text = (charScript.accesory.name + "\nLVL: " + charScript.accesory.Level);
            }
            else if (obj.tag == "Item")
            {
                obj.sprite = LoadInfo.Instance.nullIcon; //iconsSpriteSheet[58];
                GameObject.Find("Accessory").GetComponentInChildren<Text>().text = ("No accessory equipped");
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------------

    public void SetupShop(NpcWorldScript shopNPC)
    {
        inputList = new List<Button>();
        foreach (Button button in equipCanvas.GetComponentsInChildren<Button>())
        {
            inputList.Add(button);
        }

        Debug.Log("Opening shop!");
        menuState = MenuState.Shop;
        inputState = InputState.Item;
        mainCanvas.SetActive(false);
        characterCanvas.SetActive(false);
        techTreeCanvas.SetActive(false);
        partyCanvas.SetActive(false);
        equipCanvas.SetActive(true);
        mapCanvas.SetActive(false);
        settingsCanvas.SetActive(false);

        LoadInfo.Instance.pause = true;
        LoadInfo.Instance.gameState = LoadInfo.GameState.Pause;

        actualCanvas.enabled = false;
        actualCanvas.enabled = true;

        scrollInt = 0;
        shopPriceModifier = shopNPC.pricePercent;
        itemsList = shopNPC.shopInventory;

        UpdateEquipInput();
    }

    public void BuyItem()
    {
        if ((int)(buyNum * itemsList[buttonNum + scrollInt].Value * shopPriceModifier / 100f) <= InventoryScript.Instance.Krn)
        {
            InventoryScript.Instance.ModifyInventory((int)(buyNum * itemsList[buttonNum + scrollInt].Value * shopPriceModifier / -100f), InventoryScript.Instance.currency);
            InventoryScript.Instance.ModifyInventory(buyNum, itemsList[buttonNum + scrollInt]);
            CloseNotification();
            Debug.Log("Enough money! " + (int)(buyNum * itemsList[buttonNum + scrollInt].Value * shopPriceModifier / 100f));
        }
        else
        {
            CloseNotification();
            Debug.Log("Not enough money! " + (int)(buyNum * itemsList[buttonNum + scrollInt].Value * shopPriceModifier / 100f));
        }
    }

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------------

    public void SetupNotification(Notification note)
    {
        LoadInfo.Instance.pause = true;
        notificationCanvas.SetActive(true);
        actualCanvas.enabled = false;
        actualCanvas.enabled = true;

        notificationCanvas.GetComponentInChildren<Text>().text = note.text;

        Button[] buttonList = notificationCanvas.GetComponentsInChildren<Button>();
        for (int n = 0; n < buttonList.Length; n++)
        {
            if (note.options.Length > n && note.text.Length > n)
            {
                buttonList[n].onClick.RemoveAllListeners();
                buttonList[n].GetComponentInChildren<Text>().text = note.options[n];
                buttonList[n].onClick.AddListener(note.actions[n]);
                buttonList[n].interactable = true;
            }
            else
            {
                buttonList[n].onClick.RemoveAllListeners();
                buttonList[n].GetComponentInChildren<Text>().text = "";
                buttonList[n].interactable = false;
            }

        }
    }

    public void SetupNotification(string text)
    {
        notificationCanvas.GetComponentInChildren<Text>().text = text;
    }

    public void CloseNotification()
    {
        if (LoadInfo.Instance.gameState == LoadInfo.GameState.World)
        {
            LoadInfo.Instance.pause = false;
        }
        notificationCanvas.SetActive(false);

        actualCanvas.enabled = false;
        actualCanvas.enabled = true;
    }

    public void SetupShopAmount()
    {
        buyNum = 1;
        SetupNotification(new Notification("Buy: " + buyNum + " " + itemsList[buttonNum].name + "(s) for " + (int)(buyNum * itemsList[buttonNum].Value * shopPriceModifier / 100f) + " / " + InventoryScript.Instance.Krn + " Krn. ",
            new string[5] { "-10", "-1", "Buy", "+1", "+10" },
            new UnityAction[5] { (() => ChangeBuyNum(-10)), (() => ChangeBuyNum(-1)), (() => BuyItem()), (() => ChangeBuyNum(1)), (() => ChangeBuyNum(10)), }));
    }

    public void ChangeBuyNum(int num)
    {
        buyNum += num;
        if (buyNum < 0)
        {
            buyNum = 0;
        }
        SetupNotification("Buy: " + buyNum + " " + itemsList[buttonNum].name + "(s) for " + (int)(buyNum * itemsList[buttonNum].Value * shopPriceModifier / 100f) + " / " + InventoryScript.Instance.Krn + " Krn. ");
    }

    public void ProcessNotification(int num)
    {

    }

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------------

    public void SetupDialogue(Dialogue temp) // Opens the dialogue box with a set of dialogue
    {
        foreach (Image img in actualCanvas.GetComponentsInChildren<Image>())
        {
            if (img.tag == "CanvasBackground")
            {
                img.sprite = backgroundSprite;
                img.type = Image.Type.Tiled;
            }
            else if (img.tag == "InputButton")
            {
                img.sprite = buttonSprite;
                img.type = Image.Type.Tiled;
            }
        }

        inputList = new List<Button>();
        foreach (Button button in dialogueCanvas.GetComponentsInChildren<Button>())
        {
            inputList.Add(button);
        }
        dialogueCanvas.SetActive(true);
        dialogue = temp.dialogueList;

        LoadInfo.Instance.pause = true;
        currentDialogue = 0;

        actualCanvas.enabled = false;
        actualCanvas.enabled = true;

        StartCoroutine(UpdateDialogue(true));
    }

    public void ProcessDialogue(int num) // Changes the dialogue based on which option the player clicked
    {
        if (dialogue[currentDialogue].options[num].action != null)
        {
            dialogue[currentDialogue].options[num].action.Invoke();
        }

        if (dialogueCanvas.activeSelf)
        {
            currentDialogue = dialogue[currentDialogue].options[num].destination;
            StartCoroutine(UpdateDialogue(true));
        }

    }

    public void CloseDialogue() // Closes the dialogue box
    {
        Debug.Log("Closing dialogue");
        dialogueCanvas.SetActive(false);
        dialogue = null;
        LoadInfo.Instance.pause = false;
        currentDialogue = 0;
    }

    public IEnumerator UpdateDialogue(bool auto) // Updates the display for the dialogue box
    {
        Debug.Log(dialogue);

        skipText = false;
        displayPortrait.sprite = dialogue[currentDialogue].portrait;
        displayText.text = "";

        Debug.Log("Typing...");

        for (int n = 0; n < dialogue[currentDialogue].text.Length; n++)
        {
            if (skipText)
            {
                displayText.text = dialogue[currentDialogue].text;
                break;
            }
            displayText.text += dialogue[currentDialogue].text[n];

            yield return new WaitForSeconds(dialogue[currentDialogue].textDelay);
            if (dialogue[currentDialogue].text[n] == '.' || dialogue[currentDialogue].text[n] == ',' || dialogue[currentDialogue].text[n] == ';' || dialogue[currentDialogue].text[n] == ':')
            {
                yield return new WaitForSeconds(dialogue[currentDialogue].textDelay * 4);
            }
        }

        Debug.Log("Done typing!");

        for (int n = 0; n < inputList.Count; n++)
        {
            yield return new WaitForSeconds(0.05f);

            Text buttonText = inputList[n].transform.Find("Text").GetComponent<Text>();
            Image buttonIcon = inputList[n].transform.Find("Icon").GetComponent<Image>();
            bool available = true;

            if (n < dialogue[currentDialogue].options.Count)
            {
                buttonText.text = dialogue[currentDialogue].options[n].text;
                // buttonIcon.sprite = dialogue[currentDialogue].options[n].
                available = true;
                // Test requirements
            }
            else
            {
                available = false;
                buttonText.text = "";
            }

            inputList[n].interactable = available;
        }
    }

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------------

    public void SetupTechTree()
    {
        techTreeCanvas.SetActive(true);
        CharacterScript charScript = selectedCharacter.GetComponent<CharacterScript>();

        foreach (Button button in techTreeCanvas.GetComponentsInChildren<Button>())
        {
            Text buttonText = button.GetComponentInChildren<Text>();
            Image buttonIcon = button.transform.Find("Icon").GetComponent<Image>();

            TechTreeInput traitInput = button.GetComponent<TechTreeInput>();
            Trait trait = null;

            for (int n = 0; n < traitInput.traits.Count; n++)
            {
                if (!charScript.unlockedTraits.Contains(traitInput.traits[n]))
                {
                    trait = traitInput.traits[n];
                    break;
                }
            }

            if (charScript.unlockPoints >= trait.unlockPoints && trait != null)
            {
                button.interactable = true;
                buttonText.text = trait.name;
                buttonIcon.sprite = trait.icon;
            }
            else if (trait != null)
            {
                button.interactable = false;
                buttonText.text = trait.name;
                buttonIcon.sprite = trait.icon;
            }
            else
            {
                button.interactable = false;
                buttonText.text = traitInput.traits[0].name;
                buttonIcon.sprite = traitInput.traits[0].icon;
            }
        }
    }

    public void SetupTraitConfirmation (TechTreeInput tech)
    {

    }

    public void ConfirmTechTreeInput(TechTreeInput tech)
    {
        //CharacterScript charScript = selectedCharacter.GetComponent<CharacterScript>();

        Debug.Log("Adding " + tech.name);
        if (tech != null)
        {
            tech.Unlock();
        }
    }

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------------

    public void ToggleCommand()
    {
        if (commandInput.interactable)
        {
            if (LoadInfo.Instance.gameState == LoadInfo.GameState.World)
            {
                LoadInfo.Instance.pause = false;
            }

            commandInput.interactable = false;
            commandInput.transform.localPosition = new Vector3(commandInput.transform.localPosition.x, 500f);
        }
        else
        {
            LoadInfo.Instance.pause = true;
            commandInput.interactable = true;
            commandInput.transform.localPosition = new Vector3(commandInput.transform.localPosition.x, 296f);
            commandInput.ActivateInputField();
        }
    }

    public void ProcessCommandInput(string text)
    {
        CharacterScript charScript = null;
        commandInput.text = "";
        ToggleCommand();
        string[] t = text.Split(' '); //text.Contains(" ") ? text.Substring(0, text.IndexOf(" ")).ToLower() : text.ToLower();
        Debug.Log("t.length == " + t.Length);

        switch (t[0].ToLower())
        {
            case "test":

                Debug.Log("Yep, it's working");
                break;

            case "give":
            case "giveitem":
            case "get":
            case "getitem":

                if (Char.IsNumber(t[1][0]))
                {
                    for (int n = 3; n < t.Length; n++)
                    {
                        t[2] += " ";
                        t[2] += t[n];
                    }
                    InventoryScript.Instance.ModifyInventory(Convert.ToInt32(t[1]), t[2]);
                }
                else if (Char.IsNumber(t[t.Length - 1][0]))
                {
                    for (int n = 3; n < t.Length - 1; n++)
                    {
                        t[1] += " ";
                        t[1] += t[n];
                    }
                    InventoryScript.Instance.ModifyInventory(Convert.ToInt32(t[t.Length - 1]), t[1]);
                }
                else
                {
                    for (int n = 2; n < t.Length; n++)
                    {
                        t[1] += " ";
                        t[1] += t[n];
                    }
                    InventoryScript.Instance.ModifyInventory(1, t[1]);
                }
                break;

            case "rest":
            case "healall":

                foreach (GameObject obj in LoadInfo.Instance.partyMembers)
                {
                    charScript = obj.GetComponent<CharacterScript>();

                    if (charScript)
                    {
                        charScript.ModifyHealth(charScript.maxHealth);
                        charScript.ModifyMagic(charScript.maxMagic);
                    }
                }
                break;

            case "loadscene":
            case "loadlevel":

                SceneManager.LoadScene(t[1]);
                break;

            case "xp":
            case "exp":
            case "experience":

                charScript = GameObject.Find(t[1]).GetComponent<CharacterScript>();
                if (charScript)
                {
                    charScript.ModifyExperience(Convert.ToInt32(t[2]));
                }
                break;

            case "invincibility":
            case "invalnurability":

                LoadInfo.Instance.worldPlayer.GetComponent<PlayerMove>().invalnurability = Convert.ToInt32(t[1]);
                Debug.Log("Adding invalnurability seconds");
                break;

            case "states":
            case "state":

                Debug.Log("Game state: " + LoadInfo.Instance.gameState + "\nMenu state: " + menuState + "\nInput state: " + inputState);
                break;

            case "database":

                string temp = "";
                temp += ("<b>Showing item database</b>\n");
                foreach (KeyValuePair<string, Item> pair in InventoryScript.Database)
                {
                    temp +=("\n'<b>" + pair.Key + "</b>'");
                }
                Debug.Log(temp);
                break;

            default:

                Debug.LogWarning("Unknown command");
                break;
        }
    }
}
