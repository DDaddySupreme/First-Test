using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleController : MonoBehaviour
{

    public static GameObject turn;
    public static List<GameObject> charList;
    public static Dictionary<GameObject, CharacterScript> scriptDict;

    //public InputField inputField;
    [Space]
    public GameObject floatingText;
    public Text actionTextBox;
    public Text statsTextBox;
    public InventoryScript invScript;
    public LoadInfo loadInfo;
    public Canvas battleCanvas;
    public Button[] buttonArray;
    public Image[] iconArray;
    public InputState inputState;
    public WeaponState currentWeapon = WeaponState.Choosing;

    private bool canUseItem = true;
    private int scrollInt = 0;
    [System.Obsolete("Use TotalTime instead")]
    private int totalTime;
    private List<GameObject> waitingList;
    private GameObject selectedTarget = null;
    private Attack selectedAttack;
    private Item selectedItem;

    private List<Item> Consumables
    {
        get { return invScript.GetListType(new List<Item.ItemType>() { Item.ItemType.Consumable }); }
    }

    private int TotalTime
    {
        get
        {
            int temp = 0;
            foreach (GameObject obj in charList)
            {
                temp += scriptDict[obj].speed;
            }
            return temp;
        }
    }

    public enum InputState
    {
        Category,
        Action,
        Spell,
        Item,
        Target,
        Wait
    }

    public enum WeaponState
    {
        Choosing,
        Right,
        Left,
        Consumable,
        None
    }

    private void Awake()
    {
        loadInfo = LoadInfo.Instance; //GameObject.Find("InfoLoader").GetComponent<LoadInfo>();
        loadInfo.gameState = LoadInfo.GameState.Battle;
        //inputField = FindObjectOfType<InputField>();
        buttonArray = FindObjectsOfType<Button>();
        iconArray = FindObjectsOfType<Image>();

        Text[] textArray = FindObjectsOfType<Text>();
        foreach (Text text in textArray)
        {
            if (text.name.Replace(" ", "") == "ActionText")
            {
                actionTextBox = text;
            }
            else if (text.name.Replace(" ", "") == "StatsText")
            {
                statsTextBox = text;
            }
        }
        Spawn();
        /*foreach (GameObject obj in charList)
        {
            Debug.Log(obj.name + " : " + scriptDict[obj].health);
        }*/
    }

    void Start()
    {
        turn = FindTurn();
        invScript = InventoryScript.Instance; //loadInfo.GetComponent<InventoryScript>();

        /*DisplayText("it is <color=cyan>" + turn.name + "</color>'s turn");
        if (turn.tag == "Player")
        {
            inputState = InputState.Category;
            DisplayText("Choose an attack!");
        }
        else
        {
            inputState = InputState.Wait;
        }*/

        //inputField.onEndEdit.AddListener(ProcessPlayerText);

        /*UpdateStatsText();
        if (TurnEnded != null)
        {
            Debug.Log("Turn ended");
            TurnEnded();
        }*/
        UpdateInputArea();

        NextTurn();
    }


    void Update()
    {

    }

    // Puts all the characters and their scripts containing stats in one place to minimize use of GetComponent
    void Spawn()
    {
        SpecialAttackFunctions.instance.battleController = this;

        string endText = "";
        int count = 0;
        charList = new List<GameObject>();
        scriptDict = new Dictionary<GameObject, CharacterScript>();
        waitingList = new List<GameObject>();
        //totalTime = 0;
        /*foreach (GameObject obj in loadInfo.partyMembers)
        {           
            Instantiate(obj, transform);
        }*/

        if (loadInfo.worldEnemy.enemies.Count <= 0)
        {
            Debug.LogError("Trying to enter battle with no enemies");
            EndBattle(false);
            return;
        }

        foreach (GameObject enemy in loadInfo.worldEnemy.enemies)
        {
            GameObject obj = Instantiate(enemy, gameObject.transform);
            obj.transform.position = new Vector3(2 * count - (2 + loadInfo.worldEnemy.enemies.Count), 2);

            obj.name = obj.name.Replace("(Clone)", "");
            CharacterScript charScript = (obj.GetComponent<CharacterScript>());
            charScript.battleController = this;
            scriptDict.Add(obj, charScript);
            charList.Add(obj);

            charScript.EnterBattle();
            charScript.DefaultValues();
            //totalTime += charScript.speed;
            endText += (obj.name + " has been loaded as an enemy and has " + charScript.speed + " speed\n");
            count++;
        }

        for (int n = 0; n < 4; n++)
        {
            if (loadInfo.partyMembers[n]) //(n < loadInfo.partyMembers.Count)
            {
                loadInfo.partyMembers[n].SetActive(true);
                loadInfo.partyMembers[n].transform.position = new Vector3(2 * n - (2 + 4), -3);

                charList.Add(loadInfo.partyMembers[n]);
                CharacterScript charScript = loadInfo.partyMembers[n].GetComponent<CharacterScript>();
                charScript.battleController = this;
                charScript.DefaultValues();
                scriptDict.Add(loadInfo.partyMembers[n], charScript);

                charScript.EnterBattle();
                //totalTime += charScript.speed;
                endText += (loadInfo.partyMembers[n].name + " has been loaded as a player and has " + charScript.speed + " speed\n");
            }
        }

        //Debug.Log("Total time is " + totalTime);
        DisplayText(endText);
    }

    public void Spawn(GameObject toSpawn, int num = 1, string tag = "")
    {
        for (int n = 0; n < num; n++)
        {
            GameObject obj = Instantiate(toSpawn, gameObject.transform);
            string endText = "";

            if (tag != "")
            {
                obj.tag = tag;
            }

            obj.name = obj.name.Replace("(Clone)", "");
            CharacterScript charScript = (obj.GetComponent<CharacterScript>());
            charScript.battleController = this;
            scriptDict.Add(obj, charScript);
            charList.Add(obj);

            charScript.EnterBattle();
            charScript.DefaultValues();
            //totalTime += charScript.speed;
            endText += (obj.name + " has been loaded as '" + obj.tag + "' and has " + charScript.speed + " speed\n");

            DisplayText(endText);
        }
    }

    // Sort by speed
    [System.Obsolete("Switch to FindTurn")]
    private int SortBySpeed(GameObject char1, GameObject char2) // char1 and char2 are reversed, so a higher speed is lower on the ist, and chosen first
    {
        return (scriptDict[char2].speed + UnityEngine.Random.Range(-5, 6)).CompareTo(scriptDict[char1].speed);

    }

    private GameObject FindTurn()
    {
        Debug.Log(TotalTime);
        GameObject temp = null;
        int count = 0;

        if (turn != null)
        {
            scriptDict[turn].turnNum = 0;
        }

        while (waitingList.Count == 0)
        {
            foreach (GameObject obj in charList)
            {
                scriptDict[obj].turnNum += scriptDict[obj].speed + UnityEngine.Random.Range(-scriptDict[obj].speed / 4, scriptDict[obj].speed / 4);
                if (scriptDict[obj].turnNum >= TotalTime && scriptDict[obj].position != CharacterScript.Position.Removed)
                {
                    waitingList.Add(obj);
                    Debug.Log(obj.name + " is ready to attack!");
                }
            }

            count++;
            if (count >= 500)
            {
                Debug.LogError("Infinity error! (probably) \nNo characters are ready to act after 500 tries! (BattleController/private GameObject FindTurn()) \nAre all enemies asleep? Adding all characters to waitingList");
                Debug.Break();
                foreach (GameObject obj in charList)
                {
                    waitingList.Add(obj);
                }
                break;
            }
        }

        foreach (GameObject obj in waitingList)
        {
            //Debug.Log(obj.name + " : " + scriptDict[obj].turnNum);
            if (scriptDict[obj].position == CharacterScript.Position.Removed)
            {
                waitingList.Remove(obj);
                continue;
            }

            if (temp == null)
            {
                temp = obj;
            }
            else if (scriptDict[temp].turnNum < scriptDict[obj].turnNum)
            {
                temp = obj;
            }
        }

        waitingList.Remove(temp);
        return temp;
    }

    // Player input -------------------------------------------------------------------------------------------------------------------------------
    public void ProcessPlayerInput(int buttonNumber)
    {
        if (buttonNumber > -1)
        {
            switch (inputState)
            {
                case InputState.Category:

                    if (buttonNumber == 0)
                    {
                        inputState = InputState.Action;
                    }
                    else if (buttonNumber == 1)
                    {
                        inputState = InputState.Spell;
                    }
                    else if (buttonNumber == 2)
                    {
                        inputState = InputState.Item;
                    }
                    scrollInt = 0;
                    break;

                case InputState.Action:
                    if (buttonNumber == 0 && scrollInt > 0)
                    {
                        scrollInt -= 1;
                    }
                    else if (buttonNumber == 4 && buttonNumber + scrollInt < scriptDict[turn].GetAttackList(false, currentWeapon).Count - 1)
                    {
                        scrollInt += 1;
                    }
                    else if (buttonNumber + scrollInt < scriptDict[turn].GetAttackList(false, currentWeapon).Count)
                    {
                        selectedAttack = scriptDict[turn].GetAttackList(false, currentWeapon)[buttonNumber + scrollInt];
                        inputState = InputState.Target;
                        scrollInt = 0;
                    }
                    break;

                case InputState.Spell:
                    if (buttonNumber == 0 && scrollInt > 0)
                    {
                        scrollInt -= 1;
                    }
                    else if (buttonNumber == 4 && buttonNumber + scrollInt < scriptDict[turn].GetAttackList(true, currentWeapon).Count - 1)
                    {
                        scrollInt += 1;
                    }
                    else if (buttonNumber + scrollInt < scriptDict[turn].GetAttackList(true, currentWeapon).Count)
                    {
                        selectedAttack = scriptDict[turn].GetAttackList(true, currentWeapon)[buttonNumber + scrollInt];
                        inputState = InputState.Target;
                        scrollInt = 0;
                    }
                    break;

                case InputState.Item:
                    if (buttonNumber == 0 && scrollInt > 0)
                    {
                        scrollInt -= 1;
                    }
                    else if (buttonNumber == 4 && buttonNumber + scrollInt < Consumables.Count - 1)
                    {
                        scrollInt += 1;
                    }
                    else if (buttonNumber + scrollInt < Consumables.Count)
                    {
                        if (Consumables[buttonNumber + scrollInt].attacksList.Count > 0)
                        {
                            selectedItem = Consumables[buttonNumber + scrollInt];
                            selectedAttack = Consumables[buttonNumber + scrollInt].attacksList[0];
                            currentWeapon = WeaponState.Consumable;
                            inputState = InputState.Target;
                            scrollInt = 0;
                        }
                        else
                        {
                            Debug.LogError("Something went wrong with the consumable!");
                            RestartTurn();
                        }

                    }
                    break;

                case InputState.Target:
                    if (buttonNumber == 0 && scrollInt > 0)
                    {
                        scrollInt -= 1;
                    }
                    else if (buttonNumber == 4 && buttonNumber + scrollInt < charList.Count - 1)
                    {
                        scrollInt += 1;
                    }
                    else if (buttonNumber + scrollInt < charList.Count)
                    {
                        selectedTarget = charList[buttonNumber + scrollInt];

                        if (selectedAttack != null)
                        {
                            switch (selectedAttack.targetType)
                            {
                                case Attack.TargetType.Self:
                                    selectedTarget = turn;
                                    break;
                            }
                            StartCoroutine(DoTurn(selectedAttack, selectedTarget));
                            inputState = InputState.Wait;
                        }
                        else if (selectedItem != null)
                        {
                            //StartCoroutine(UseConsumable(selectedItem, selectedTarget));
                            currentWeapon = WeaponState.None;
                            inputState = InputState.Wait;
                        }
                        else
                        {
                            Debug.LogError("Something went wrong! \nSelected target and selected attack are null!");
                            break;
                        }
                        UpdateInputArea();
                        selectedTarget = null;
                        selectedAttack = null;
                        //selectedItem = null;
                        scrollInt = 0;
                    }
                    break;

                case InputState.Wait:

                    DisplayText("State is currently set to wait, so be patient!");
                    break;
            }
        }
        else if (inputState == InputState.Category)
        {
            DisplayText("Skipping turn...");
            inputState = InputState.Wait;
            //scriptDict[turn].CalculateStatusEffects();
            NextTurn();
        }
        else
        {
            inputState = InputState.Category;
            scrollInt = 0;
        }
        UpdateInputArea();
    }

    public void DoTurnHolder(Attack attack, GameObject target, bool advanceTurn = true) // 'Cause you can't start coroutines from a non-monobehaviour, but you also can't save a monobehaviour
    {
        StartCoroutine(DoTurn(attack, target, advanceTurn));
    }

    // This makes the turn happen -----------------------------------------------------------------------------------------------------------------
    public IEnumerator DoTurn(Attack attack, GameObject target, bool advanceTurn = true)
    {
        List<GameObject> targets = new List<GameObject>(); // { target }; ;
        List<int> startingHealth = new List<int>();
        Item weapon = null;
        string temp = "";
        bool miss = false;

        switch (attack.targetType)
        {
            default:
            case Attack.TargetType.Self:
            case Attack.TargetType.Normal:

                targets = new List<GameObject>() { target };
                break;

            case Attack.TargetType.All:

                foreach (GameObject obj in charList)
                {
                    targets.Add(obj);
                }
                break;

            case Attack.TargetType.Group:

                foreach (GameObject obj in charList)
                {
                    if (obj.tag == target.tag)
                    {
                        targets.Add(obj);
                    }
                }
                break;

            case Attack.TargetType.Row:

                foreach (GameObject obj in charList)
                {
                    if (obj.tag == target.tag && scriptDict[obj].position == scriptDict[target].position)
                    {
                        targets.Add(obj);
                    }
                }
                break;

            case Attack.TargetType.Area:

                foreach (GameObject obj in charList)
                {
                    if (obj.tag == target.tag && Mathf.Abs(charList.IndexOf(target) - charList.IndexOf(obj)) <= 1) // To do: maybe make the range variable
                    {
                        targets.Add(obj);
                    }
                }
                break;
        }

        foreach (GameObject trgt in targets)
        {
            startingHealth.Add(scriptDict[trgt].health);
        }

        yield return new WaitForSeconds(0.25f);

        // check magic
        if (scriptDict[turn].magic >= attack.magicRequired)
        {
            scriptDict[turn].magic -= attack.magicRequired;

            foreach (GameObject trgt in targets)
            {
                if (targets.Last() != trgt)
                {
                    temp += trgt.name + ", ";
                }
                else
                {
                    temp += trgt.name;
                }
            }
            //scriptDict[turn].CalculateStatusEffects();
            if (selectedItem == null)
            {
                DisplayText("\n<color=cyan><b>" + turn.name + " ---> " + attack.name + " ---> " + temp + "</b></color>");
                DisplayText("Lost " + attack.magicRequired + " magic");
            }
            else
            {
                DisplayText("\n<color=cyan><b>" + turn.name + " ---> " + selectedItem.name + " ---> " + temp + "</b></color>");
                DisplayText("Lost " + attack.magicRequired + " magic");
            }

        }
        else
        {
            DisplayText("Not enough magic! Try another attack!");
            yield return new WaitForSeconds(0.25f);
            RestartTurn();
            yield break;
        }
        yield return new WaitForSeconds(0.5f);


        switch (currentWeapon)
        {
            case WeaponState.Right:
                weapon = scriptDict[turn].weaponR;
                break;

            case WeaponState.Left:
                weapon = scriptDict[turn].weaponL;
                break;

            case WeaponState.Consumable:
                weapon = selectedItem;
                break;

            case WeaponState.None:
                weapon = invScript.defaultWeapon;
                Debug.LogWarning(turn.name + " does not have a weapon equipped!");
                break;
        }

        SpecialAttackFunctions.instance.totalDamage = 0;
        foreach (StatusEffect effect in scriptDict[turn].offendDelegateStatuses) // Might find a way to add crits, but for now, this is fine
        {
            if (SpecialAttackFunctions.instance.offendDelegates.ContainsKey(effect.removeID))
            {
                SpecialAttackFunctions.instance.offendDelegates[effect.removeID](scriptDict[turn], scriptDict[target], attack, weapon, effect);
            }
            else
            {
                Debug.LogError("Offend delegate: '" + effect.removeID + "' not found! Current turn: " + turn.name + " using " + effect.name);
            }
        }

        for (int n = 0; n < targets.Count; n++)
        {
            if (!attack.positive && scriptDict[targets[n]].reflect && attack.magical)
            {
                targets[n] = turn;
            }
            else if (!attack.positive && scriptDict[targets[n]].defender)
            {
                targets[n] = scriptDict[targets[n]].defender;
            }

            // Calculate crit/miss and do attack
            int chance = UnityEngine.Random.Range(1, 100);
            //chance += (scriptDict[turn].luck - scriptDict[targets[n]].speed);
            Debug.Log(targets[n].name + " chance: " + chance);//+ " ( +" + (scriptDict[turn].luck - scriptDict[targets[n]].speed) + " )");

            if (chance <= (100 - attack.hitChance)) // Miss
            {
                DisplayText("Missed " + targets[n].name + "!");
                miss = true;
            }
            else if (chance > (100 - attack.critChance)) // Crit
            {
                SpecialAttackFunctions.instance.totalDamage = 0;
                if (attack.delegates.Count <= 0)
                {
                    SpecialAttackFunctions.instance.attackDelegates["StandardAttack"](scriptDict[turn], scriptDict[targets[n]], attack, weapon, new SubAttack(), true);
                    Debug.LogWarning("Attack: " + attack.name + " does not have any attack delegates! Using StandardAttack.");
                }
                else
                {
                    for (int p = 0; p < attack.delegates.Count; p++) //(string name in attack.delegates)
                    {
                        if (SpecialAttackFunctions.instance.attackDelegates.ContainsKey(attack.delegates[p].delegateName) && p <= attack.delegates.Count)
                        {
                            SpecialAttackFunctions.instance.attackDelegates[attack.delegates[p].delegateName](scriptDict[turn], scriptDict[targets[n]], attack, weapon, attack.delegates[p], true);
                        }
                        else
                        {
                            Debug.LogError("Attack delegate: '" + name + "' not found! Current turn: " + turn.name + " using " + attack.name);
                        }
                    }
                }

                SpecialAttackFunctions.instance.totalDamage = 0;
                foreach (StatusEffect effect in scriptDict[targets[n]].defendDelegateStatuses) // Defensive delegate for target
                {
                    if (SpecialAttackFunctions.instance.defendDelegates.ContainsKey(effect.removeID))
                    {
                        SpecialAttackFunctions.instance.defendDelegates[effect.removeID](scriptDict[targets[n]], scriptDict[turn], attack, weapon, effect);
                    }
                    else
                    {
                        Debug.LogError("Defend delegate: '" + effect.removeID + "' not found! Current turn: " + targets[n].name + " using " + effect.name);
                    }
                }

                miss = false;
            }
            else // Normal
            {
                SpecialAttackFunctions.instance.totalDamage = 0;
                if (attack.delegates.Count <= 0)
                {
                    SpecialAttackFunctions.instance.attackDelegates["StandardAttack"](scriptDict[turn], scriptDict[targets[n]], attack, weapon, new SubAttack(), false);
                }
                else
                {
                    for (int p = 0; p < attack.delegates.Count; p++) //(string name in attack.delegates)
                    {
                        if (SpecialAttackFunctions.instance.attackDelegates.ContainsKey(attack.delegates[p].delegateName) && p <= attack.delegates.Count && p <= attack.delegates.Count)
                        {
                            Debug.Log("P: " + p);
                            SpecialAttackFunctions.instance.attackDelegates[attack.delegates[p].delegateName](scriptDict[turn], scriptDict[targets[n]], attack, weapon, attack.delegates[p], false);
                        }
                        else
                        {
                            Debug.LogError("Attack delegate: '" + attack.delegates[p] + "' not found! Current turn: " + turn.name + " using " + attack.name);
                        }
                    }
                }

                SpecialAttackFunctions.instance.totalDamage = 0;
                foreach (StatusEffect effect in scriptDict[targets[n]].defendDelegateStatuses) // Defensive delegate for target
                {
                    if (SpecialAttackFunctions.instance.defendDelegates.ContainsKey(effect.removeID))
                    {
                        SpecialAttackFunctions.instance.defendDelegates[effect.removeID](scriptDict[targets[n]], scriptDict[turn], attack, weapon, effect);
                    }
                    else
                    {
                        Debug.LogError("Defend delegate: '" + effect.removeID + "' not found! Current turn: " + targets[n].name + " using " + effect.name);
                    }
                }

                miss = false;
            }

            /*if (attack.magicRequired <= 0) // Physical attack
            {
                finalDamage.Insert(n, (int)
                    (physDamage * 
                    damageMultiplier[n] * 
                    ((float)attack.damageModifier / 100) * 
                    ((float)scriptDict[turn].strength / scriptDict[targets[n]].resistance)));
            }
            else // Magic attack
            {
                finalDamage.Insert(n, (int)
                    (magDamage * 
                    damageMultiplier[n] * 
                    ((float)attack.damageModifier / 100) * 
                    ((float)scriptDict[turn].intelligence / scriptDict[targets[n]].spirit)));

            }

            if (attack.damageModifier >= 0) // Dealing damage
            {
                DisplayText("<color=red>Dealt " + (finalDamage[n]) + " damage!</color>"); // To do: impliment resistance to certain types of damage

                if (loadInfo.difficultyLevel > 0)
                {
                    scriptDict[turn].threat += Mathf.Abs(finalDamage[n]) / (loadInfo.difficultyLevel);
                }
            }
            else if (scriptDict[targets[n]].health > 0) // Healing
            {
                DisplayText("<color=yellow>Healed " + (finalDamage[n] * -1) + " health!</color>");
                scriptDict[turn].threat += Mathf.Abs(finalDamage[n]) * (loadInfo.difficultyLevel); // To do: make threat work with poison
            }
            else // Healing, target is dead
            {
                DisplayText(targets[n].name + " is dead, and was therefor not healed");
            }
            
            if (attack.damageType == Attack.DamageType.Steal)
            {                
                if (targets[n].tag != turn.tag)
                {
                    // Calculate chance to steal
                }
                else
                {
                    DisplayText("Can't steal fron your team mate!");
                }
            }
            else if (attack.damageType == Attack.DamageType.Leave)
            {
                if (targets[n].tag != turn.tag)
                {
                    // Calculate chance to leave
                }
                else
                {
                    // just use chance
                }
            }
            else if (scriptDict[targets[n]].damageTypeModifier != null)
            {
                if (scriptDict[targets[n]].damageTypeModifier.ContainsKey(attack.damageType))
                {
                    finalDamage[n] = (int)(finalDamage[n] * scriptDict[targets[n]].damageTypeModifier[attack.damageType]);
                }
            }
            else
            {
                scriptDict[targets[n]].damageTypeModifier = new Dictionary<Attack.DamageType, float>();
            }
            
            finalDamage[n] += UnityEngine.Random.Range((finalDamage[n] / -10), (finalDamage[n] / 10));

            scriptDict[targets[n]].health -= finalDamage[n];
            
            UpdateStatsText();

            // Leach health. leach of 0 will do nothing
            yield return new WaitForSeconds(0.5f);
            if (attack.leachHpPercent > 0 && Mathf.Abs(finalDamage[n] * attack.leachHpPercent / 100f) > startingHealth[n])
            {
                scriptDict[turn].health += startingHealth[n];
                DisplayText("<color=yellow>Stole <b>" + startingHealth[n] + "</b> health!</color>");
            }
            else if (attack.leachHpPercent > 0 && finalDamage[n] != 0)
            {
                scriptDict[turn].health += (int)(finalDamage[n] * attack.leachHpPercent / 100f);
                DisplayText("<color=yellow>Stole <b>" + finalDamage[n] + "</b> health!</color>");
            }
            */
            yield return new WaitForSeconds(1f);

            // Status effects 
            if (attack.statusEffects.Count > 0 && !miss)
            {
                yield return new WaitForSeconds(0.5f);
                foreach (StatusEffect effect in attack.statusEffects)
                {
                    scriptDict[targets[n]].AddStatusEffect(effect, scriptDict[turn]);
                    /*if (effect.name != "") // Sometimes two effects will be combined in gameplay, like with GlassHammer (more damage, less resistance)
                    {
                        DisplayText(targets[n].name + " has gotten status effect: <b>" + effect.name + "</b> for " + effect.statusDuration + " turns.");
                    }*/
                }
            }
            if (selectedItem != null)
            {
                foreach (StatusEffect effect in selectedItem.statusEffects)
                {
                    scriptDict[targets[n]].AddStatusEffect(effect, scriptDict[turn]);
                }
            }
        }

        ////scriptDict[turn].CalculateStatusEffects();

        yield return new WaitForSeconds(1f);
        UpdateStatsText();

        // Next turn. Bool is for OnOffense and OnDefense delegates
        if (advanceTurn)
        {
            NextTurn();
        }

    }

    // Event for when turn is done
    public delegate void DoneWithTurn();
    public static event DoneWithTurn TurnEnded;

    // Displays text to screen
    public void DisplayText(string text, GameObject obj = null)
    {
        //actionTextBox.text += text + "\n";
        FloatingText ft = Instantiate(floatingText, battleCanvas.transform).GetComponentInChildren<FloatingText>();
        ft.Set(text);
        if (obj != null)
        {
            Debug.Log("<b>SPAWING TEXT POPUP AT " + obj.transform.position + "</b>");
            ft.transform.position = Camera.main.WorldToScreenPoint(obj.transform.position);
        }
        else
        {
            ft.transform.localPosition = new Vector3(0, 0);
        }

        Debug.Log(text);
        /*if (actionTextBox.text.Length > 800)
        {
            actionTextBox.text = actionTextBox.text.Remove(0, 100);
            int temp1 = actionTextBox.text.IndexOf("<");
            int temp2 = actionTextBox.text.IndexOf("/");
            while (actionTextBox.text.Remove(14).Contains(">") || temp1 == temp2 - 1)
            {
                actionTextBox.text = actionTextBox.text.Remove(0, 1);
                temp1 = actionTextBox.text.IndexOf("<");
                temp2 = actionTextBox.text.IndexOf("/");
            }
        }*/
    }

    public void UpdateStatsText()
    {
        string endText = "";
        string[] color = new string[3];

        foreach (GameObject obj in charList)
        {
            if (obj.tag == "Player")
            {
                if (turn == obj)
                {
                    color[0] = "<color=lightblue>";
                }
                else
                {
                    color[0] = "<color=white>";
                }

                if (scriptDict[obj].health > scriptDict[obj].maxHealth / 2f)
                {
                    color[1] = "<color=green>";
                }
                else if (scriptDict[obj].health > scriptDict[obj].maxHealth / 4f)
                {
                    color[1] = "<color=yellow>";
                }
                else
                {
                    color[1] = "<color=red>";
                }

                if (scriptDict[obj].magic > scriptDict[obj].maxMagic / 4f)
                {
                    color[2] = "<color=cyan>";
                }
                else
                {
                    color[2] = "<color=darkblue>";
                }


                endText += (color[0] + obj.name + "</color> - \n"
                    + color[1] + scriptDict[obj].health + "/" + scriptDict[obj].maxHealth + " HP</color>,\n " 
                    + color[2] + scriptDict[obj].magic + "/" + scriptDict[obj].maxMagic + " MP</color>\n");
            }
            /*else
            {
                color = "<color=red>";
            }*/
        }

        /*endText += ("\nCurrent Turn: " + turn.name + "\n");
        Resources.Load("/Scripts/Attacks");

        foreach (Attack attack in scriptDict[turn].attacksList) {

            if (attack.damageType == Attack.DamageType.healing)
            {
                color = "<color=green>";
            }
            else
            {
                color = "<color=red>";
            }
            endText += (attack.name + " : <color=cyan>" + attack.magicRequired + "</color> Mp\n");
        }*/
        statsTextBox.text = endText;
    }

    public void UpdateInputArea()
    {
        foreach (Button button in buttonArray)
        {
            int buttonNumber = Convert.ToInt32(button.name.Replace("Button ", "")); // could probably use array index, but I don't know if it's sorted properly
            Text buttonText = button.GetComponentInChildren<Text>();
            Image buttonIcon = null;
            string color = "<color=white>";

            foreach (Image image in iconArray)
            {
                if (image.name.Contains("Image "))
                {
                    if (Convert.ToInt32(image.name.Replace("Image ", "")) == buttonNumber)
                    {
                        buttonIcon = image.GetComponent<Image>();
                    }
                }
            }

            button.interactable = true;
            if (buttonNumber > -1)
            {
                switch (inputState) // I'm sure there's a way I can use a second function to shorten this
                {
                    case InputState.Category:
                        if (buttonNumber == 0)
                        {
                            buttonText.text = "Actions";
                            buttonIcon.sprite = loadInfo.iconsSpriteSheet[16];
                            buttonIcon.color = new Color(255, 255, 255, 255);
                        }
                        else if (buttonNumber == 1)
                        {
                            buttonText.text = "Spells";
                            buttonIcon.sprite = loadInfo.iconsSpriteSheet[8];
                            buttonIcon.color = new Color(255, 255, 255, 255);
                        }
                        else if (buttonNumber == 2 && canUseItem == true)
                        {
                            buttonText.text = "Items";
                            buttonIcon.sprite = loadInfo.iconsSpriteSheet[51];
                            buttonIcon.color = new Color(255, 255, 255, 255);

                        }
                        else
                        {
                            buttonText.text = "";
                            buttonIcon.sprite = null;
                            buttonIcon.color = new Color(255, 255, 255, 0);
                            button.interactable = false;
                        }
                        break;

                    case InputState.Action:
                        if ((buttonNumber == 0 && scrollInt > 0) || (buttonNumber == 4 && buttonNumber + scrollInt < scriptDict[turn].GetAttackList(false, currentWeapon).Count - 1))
                        {
                            buttonText.text = "More...";
                            buttonIcon.sprite = loadInfo.scrollIcon; //iconsSpriteSheet[57];
                            buttonIcon.color = new Color(255, 255, 255, 255);

                        }
                        else if (buttonNumber + scrollInt < scriptDict[turn].GetAttackList(false, currentWeapon).Count)
                        {
                            if (!scriptDict[turn].GetAttackList(false, currentWeapon)[buttonNumber + scrollInt].positive)
                            {
                                color = "<color=red>";
                            }
                            else
                            {
                                color = "<color=green>";
                            }
                            buttonText.text = (color + scriptDict[turn].GetAttackList(false, currentWeapon)[buttonNumber + scrollInt].name + "</color>");
                            buttonIcon.sprite = scriptDict[turn].GetAttackList(false, currentWeapon)[buttonNumber + scrollInt].sprite;
                            buttonIcon.color = new Color(255, 255, 255, 255);
                        }
                        else
                        {
                            buttonText.text = "";
                            buttonIcon.sprite = null;
                            buttonIcon.color = new Color(255, 255, 255, 0);
                            button.interactable = false;
                        }
                        break;

                    case InputState.Spell:
                        if ((buttonNumber == 0 && scrollInt > 0) || (buttonNumber == 4 && buttonNumber + scrollInt < scriptDict[turn].GetAttackList(true, currentWeapon).Count - 1))
                        {
                            buttonText.text = "More...";
                            buttonIcon.sprite = loadInfo.scrollIcon; //iconsSpriteSheet[57];
                            buttonIcon.color = new Color(255, 255, 255, 255);
                        }
                        else if (buttonNumber + scrollInt < scriptDict[turn].GetAttackList(true, currentWeapon).Count)
                        {
                            if (!scriptDict[turn].GetAttackList(true, currentWeapon)[buttonNumber + scrollInt].positive)
                            {
                                color = "<color=red>";
                            }
                            else
                            {
                                color = "<color=green>";
                            }
                            buttonText.text = (color + scriptDict[turn].GetAttackList(true, currentWeapon)[buttonNumber + scrollInt].name + "</color> : <color=cyan>" +
                                scriptDict[turn].GetAttackList(true, currentWeapon)[buttonNumber + scrollInt].magicRequired + " MP</color>");
                            buttonIcon.sprite = scriptDict[turn].GetAttackList(true, currentWeapon)[buttonNumber + scrollInt].sprite;
                            buttonIcon.color = new Color(255, 255, 255, 255);
                        }
                        else
                        {
                            buttonText.text = "";
                            buttonIcon.sprite = null;
                            buttonIcon.color = new Color(255, 255, 255, 0);
                            button.interactable = false;
                        }
                        break;

                    case InputState.Item:
                        if ((buttonNumber == 0 && scrollInt > 0) || (buttonNumber == 4 && buttonNumber + scrollInt < Consumables.Count - 1))
                        {
                            buttonText.text = "More...";
                            buttonIcon.sprite = loadInfo.scrollIcon; //iconsSpriteSheet[57];
                            buttonIcon.color = new Color(255, 255, 255, 255);
                        }
                        else if (buttonNumber + scrollInt < Consumables.Count)
                        {
                            buttonText.text = (Consumables[buttonNumber + scrollInt].name + " : " + invScript.inventory[Consumables[buttonNumber + scrollInt]]);
                            buttonIcon.sprite = Consumables[buttonNumber + scrollInt].Icon;
                            buttonIcon.color = new Color(255, 255, 255, 255);
                        }
                        else
                        {
                            buttonText.text = "";
                            buttonIcon.sprite = null;
                            buttonIcon.color = new Color(255, 255, 255, 0);
                            button.interactable = false;
                        }
                        break;

                    case InputState.Target:
                        if ((buttonNumber == 0 && scrollInt > 0) || (buttonNumber == 4 && buttonNumber + scrollInt < charList.Count - 1))
                        {
                            buttonText.text = "More...";
                            buttonIcon.sprite = loadInfo.scrollIcon; //iconsSpriteSheet[57];
                            buttonIcon.color = new Color(255, 255, 255, 255);
                        }
                        else if (buttonNumber + scrollInt < charList.Count)
                        {
                            if (charList[buttonNumber + scrollInt].tag == "Player")
                            {
                                color = "<color=green>";
                            }
                            else
                            {
                                color = "<color=red>";
                            }
                            buttonText.text = (color + charList[buttonNumber + scrollInt].name + "</color>");
                            buttonIcon.sprite = null;
                            buttonIcon.color = new Color(255, 255, 255, 0);

                            if (selectedAttack != null)
                            {
                                if (charList[buttonNumber + scrollInt] != turn && selectedAttack.targetType == Attack.TargetType.Self)
                                {
                                    button.interactable = false;
                                }
                                break;
                            }
                        }
                        else
                        {
                            buttonText.text = "";
                            button.interactable = false;
                        }
                        break;

                    case InputState.Wait:
                        buttonText.text = "";
                        buttonIcon.sprite = null;
                        buttonIcon.color = new Color(255, 255, 255, 0);
                        button.interactable = false;
                        break;
                }
            }
            else if (inputState == InputState.Category)
            {
                buttonText.text = "Skip";
            }
            else if (inputState == InputState.Wait)
            {
                buttonText.text = "";
                button.interactable = false;
            }
            else
            {
                buttonText.text = "Cancel";
            }
        }
    }

    /*public void ShowItems ()
    {
        string endText = "";
        foreach (KeyValuePair<Item, int> obj in invScript.inventory)
        {
            endText += (obj.Key.name + "(" + obj.Value + ") : " + obj.Key.Description + "\n");
        }
        statsTextBox.text = endText;
    }*/

    public void RestartTurn()
    {
        TurnEnded();
        if (turn.tag == "Player")
        {
            inputState = InputState.Category;
            UpdateInputArea();
        }
    }

    // Next turn 
    public void NextTurn()
    {
        Debug.Log("Starting next turn...");
        selectedItem = null;
        int count = 0;
        int charactersAlive = 0;
        //int enemiesAlive = 0;
        int playersAlive = 0;

        Debug.Log(charList.Count);
        foreach (GameObject obj in charList)
        {
            if (scriptDict[obj].health > 0)
            {
                charactersAlive += 1;

                if (obj.tag == "Player" && scriptDict[obj].position != CharacterScript.Position.Removed)
                {
                    playersAlive += 1;
                }
                else if (obj.tag == "Enemy")
                {
                    //enemiesAlive += 1;
                }
            }
        }

        if (charactersAlive != playersAlive && charactersAlive != 0) //enemiesAlive)
        {
            bool available = true;
            count = 0;
            do
            {
                /*if (count < charList.Count - 1)
                {
                    count += 1;
                    turn = (charList[count]);
                }
                else
                {
                    count = 0;
                    turn = (charList[count]);
                }*/
                count++;
                available = true;

                if (currentWeapon == WeaponState.Choosing)
                {
                    if (scriptDict[turn].weaponR != null)
                    {
                        currentWeapon = WeaponState.Right;
                    }
                    else if (scriptDict[turn].weaponL != null)
                    {
                        currentWeapon = WeaponState.Left;
                    }
                    else
                    {
                        currentWeapon = WeaponState.None;
                    }
                }
                else //if (currentWeapon != WeaponState.Choosing)
                {
                    if (currentWeapon == WeaponState.Right && scriptDict[turn].weaponL != null)
                    {
                        currentWeapon = WeaponState.Left;
                        canUseItem = false;
                    }
                    else
                    {
                        currentWeapon = WeaponState.Choosing;
                        ////scriptDict[turn].CalculateStatusEffects();
                        canUseItem = true;
                        turn = FindTurn();
                        available = false;
                    }
                }

                /*if (scriptDict[turn].health <= 0)
                {
                    currentWeapon = WeaponState.Choosing;
                    //scriptDict[turn].CalculateStatusEffects();
                    turn = FindTurn();
                    available = false;
                }*/

            }
            while (available == false && count < 100);
            Debug.Log("Current weapon: " + currentWeapon);

            if (count >= 100)
            {
                Debug.LogError("Infinity error! BattleController.cs - NextTurn(). Weapon state: " + currentWeapon);
            }

            DisplayText("\nIt is now <color=cyan>" + turn.name + "</color>'s turn.");
            scriptDict[turn].CalculateStatusEffects();

            /*if (turn.tag == "Player")
            {
                inputState = InputState.Category;
                UpdateInputArea();
                DisplayText("Choose an attack!");
            }*/

            UpdateStatsText();

            if (TurnEnded != null)
            {
                TurnEnded();
            }
        }
        else if (charactersAlive == playersAlive)
        {
            DisplayText("\nYou win!");
            StartCoroutine(EndBattle(true));
        }
        else
        {
            //Debug.Log("Players alive: " + playersAlive + ", Enemies alive: " + enemiesAlive);
            DisplayText("\nLoser!");
        }
    }

    public IEnumerator EndBattle(bool destroy)
    {
        int exp = 0;
        List<CharacterScript> party = new List<CharacterScript>();

        yield return new WaitForSeconds(0.5f);

        if (destroy)
        {
            Destroy(loadInfo.worldEnemy.gameObject);
        }

        foreach (GameObject obj in charList)
        {
            CharacterScript charScript = scriptDict[obj];

            if (loadInfo.partyMembers.Contains(obj))
            {
                party.Add(charScript);
            }
            else if (charScript.health <= 0 && obj.tag != "Player")
            {
                exp += charScript.experience;
            }
        }

        exp /= party.Count;

        foreach (CharacterScript charScript in party)
        {
            List<StatusEffect> removeList = new List<StatusEffect>();
            foreach (StatusEffect effect in charScript.statusEffects)
            {
                if (!effect.persistant)
                {
                    removeList.Add(effect);
                }
            }

            foreach (StatusEffect effect in charScript.statusEffects)
            {
                if (charScript.statusEffects.Contains(effect))
                {
                    charScript.statusEffects.Remove(effect);
                }
            }

            if (charScript.health > 0)
            {
                charScript.ModifyExperience(exp);
                DisplayText(charScript.name + " gained " + exp);
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                charScript.ModifyExperience(exp / 3);
                DisplayText(charScript.name + " gained " + exp / 3);
                yield return new WaitForSeconds(0.5f);
            }
        }

        loadInfo.worldPlayer.GetComponent<PlayerMove>().invalnurability = 4;
        loadInfo.gameState = LoadInfo.GameState.World;
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("Battle"));
        loadInfo.pause = false;
    }
}
