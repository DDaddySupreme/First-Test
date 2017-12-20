using System.Collections;
using System.Collections.Generic;
//using UnityEditor;
using UnityEngine;

[DisallowMultipleComponent, System.Serializable]
public class CharacterScript : MonoBehaviour
{
    public Sprite portrait;
    public int level;
    public int experience; // Resets each level

    [Space, Header("Stats:")] // Base stats are without any status effects

    public int health;
    public int baseMaxHealth;
    [HideInInspector]
    public int maxHealth;

    [Space]
    public int magic;
    public int baseMaxMagic;
    [HideInInspector]
    public int maxMagic;

    [Space]
    public int baseStrength; // Physical power
    [HideInInspector]
    public int strength;

    public int baseResistance; // Physical resistance
    [HideInInspector]
    public int resistance;

    [Space]
    public int baseIntelligence; // Magical power
    [HideInInspector]
    public int intelligence;

    public int baseSpirit; // Magical resistance
    [HideInInspector]
    public int spirit;

    [Space]
    public int basePersistance; // Status effect power
    [HideInInspector]
    public int persistance;

    public int baseImmunity; // Status effect resistance
    [HideInInspector]
    public int immunity;


    [Space]
    public int baseLuck; // Chance to hit/crit, and other luck based things
    [HideInInspector]
    public int luck;

    public int baseSpeed; // Chance to dodge, frequency of turns
    [HideInInspector]
    public int speed;

    [Space]
    public int inspiration; // Not completely sure what this will do

    [HideInInspector]
    public int turnNum;

    [System.Obsolete, Space]
    public bool frontRow;
    public Position basePosition;
    public Position position;

    [Space]
    public Item weaponR;
    public Item weaponL;
    public Item armour;
    public Item accesory;


    //[HideInInspector]
    public int threat; // Threat increases with damage or healing and helps determine who an AI will attack
    //[HideInInspector]
    public AiState aiState;

    [Space]
    public List<StatusEffect> statusEffects = new List<StatusEffect>();

    // These delegates will always be added by status effects, though some may be on equipped items.
    public List<StatusEffect> offendDelegateStatuses;
    public List<StatusEffect> defendDelegateStatuses;
    public GameObject defender;
    public bool reflect; // To do: I really want to get rid of this and include it in some other function somewhere so it's more modular

    [Space]
    public List<Trait> unlockedTraits = new List<Trait>();
    public List<Trait> equippedTraits = new List<Trait>();

    public int unlockPoints;
    public int equipPointsMax;
    //[HideInInspector]
    public int equipPointsUsed;

    [Space, Header("Damage Type Modifier:")]
    [Tooltip("Keys and values MUST have the same length")]
    public List<Attack.DamageType> valnurabilityTypes;
    [Tooltip("Keys and values MUST have the same length")]
    public List<int> valnurabilityPercentages; // Keys and values have to be the same length!

    [Space, Header("Enemies only")]
    [Tooltip("Enemies only. Specify what items can be dropped/stolen by their name")]
    public List<Item> dropList;
    [Tooltip("Enemies only. Specify how many of a corrosponding item can be dropped/stolen")] public List<int> dropMax;

    [Tooltip("Enemies only. What percentage does their health have to be at or lower than for them to start using caution?")] public int cautionAt;
    [Tooltip("Enemies only. What percentage does their health have to be at or lower than for them to start panicing?")] public int panicAt;

    [HideInInspector]
    public BattleController battleController;

    public int TotalExp
    {
        get { return (experience + ((int)Mathf.Pow(level, 2) * 50 + level * -50)); }
    }

    List<Item> EquipList
    {
        get { return new List<Item>() { weaponR, weaponL, armour, accesory }; }
    }

    /*public enum AiType // The main AI mode that dictates what an enemy will do
    {
        Player, // Disable AI and give controll to the player, even if this is an enemy

        SimpleEnemy, // For when a player is under enemy control

        Simple, // Mainly for when 

        Brute, // Tries to do the most damage to the enemy with the most max health

        Tactical, // Singles out most powerful targets, like mages or healers

        Threatened, // 

        Cuatious, // 

        Selfish, // Prioritizes healing self, and will run away

        Stun, // Automatically skips turn (can't just turn speed to 0 because that breaks things)

        Confused, // Enables AI and atacks completely random target with completely random attack (or heal). Chance to do something good depends on luck(?)

        Berserk, // Enables AI and attacks the member of the oposite team with the highest threat using the attack with the most physical damage, ignoring all other stats

    }*/

    public enum AiState // Mostly for status effects or death
    {
        Player, // Disable AI and give controll to the player, even if this is an enemy

        Enemy, // Enables AI and attacks players, even if this is a player

        Stun, // Automatically skips turn (can't just turn speed to 0 because that breaks things)

        Confused, // Enables AI and atacks completely random target with completely random attack (or heal). Chance to do something good depends on luck(?)

        Berserk, // Enables AI and attacks the member of the oposite team with the highest threat using the attack with the most physical damage, ignoring all other stats

        Caution, // Enables AI and prioritizes healing and attacks with immediate effects, not damage over time. Also cares less about mana costs

        Panic // Enables AI and prioritizes healing or running, ignoring other teammates completely, and using attacks with highest damage on completely random target

    }

    public enum Position
    {
        Centered,

        Front,

        Back,

        Flying,

        Removed
    }

    public void ImportStats(CharacterData data)
    {
        portrait = Resources.Load<Sprite>(data.portraitPath);
        name = data.name;
        level = data.level;
        experience = data.exp;

        health = data.health;
        Debug.Log(name + ": " + data.health + ", " + health);
        baseMaxHealth = data.baseMaxHealth;
        magic = data.magic;
        baseMaxMagic = data.baseMaxMagic;

        baseStrength = data.baseStrength;
        baseResistance = data.baseResistance;
        baseIntelligence = data.baseIntelligence;
        baseSpirit = data.baseSpirit;
        basePersistance = data.basePersistance;
        baseImmunity = data.baseImmunity;
        baseLuck = data.baseLuck;
        baseSpeed = data.baseSpeed;
        inspiration = data.inspiration;

        position = data.position;

        weaponR = data.weaponRName != "" ? InventoryScript.Database[data.weaponRName] : null;
        weaponL = data.weaponLName != "" ? InventoryScript.Database[data.weaponLName] : null;
        armour = data.armourName != "" ? InventoryScript.Database[data.armourName] : null;
        accesory = data.accesoryName != "" ? InventoryScript.Database[data.accesoryName] : null;

        aiState = data.aiState;
        statusEffects = data.statusEffects;

        equippedTraits = new List<Trait>(); //LoadInfo.Instance.attacksDatabase[data.equippedAttacksNames];
        unlockedTraits = new List<Trait>(); //LoadInfo.Instance.attacksDatabase[data.availableAttacksNames];

        foreach (string n in data.equippedTraitNames)
        {
            equippedTraits.Add(LoadInfo.Instance.traitsDatabase[n]);
        }

        foreach (string n in data.unlockedTraitNames)
        {
            unlockedTraits.Add(LoadInfo.Instance.traitsDatabase[n]);
        }

        unlockPoints = data.unlockPoints;
        equipPointsMax = data.equipPointsMax;
        equipPointsUsed = data.equipPointsUsed;

        valnurabilityTypes = data.valnurabilityTypes;
        valnurabilityPercentages = data.valnurabilityPercentages;

        // Default values?
    }

    private void OnEnable()
    {
        BattleController.TurnEnded += AI;
    }

    private void OnDisable()
    {
        BattleController.TurnEnded -= AI;
    }

    public void EnterBattle()
    {
        //battleController = GetComponentInParent<BattleController>();

        DefaultValues();
    }

    public List<Attack> GetAttackList(bool spell, BattleController.WeaponState weaponState = BattleController.WeaponState.Choosing, Item weapon = null)
    {
        List<Attack> temp = new List<Attack>();

        if (weapon != null)
        {
            foreach (Attack attack in weapon.attacksList)
            {
                if (!spell && !attack.magical && !temp.Contains(attack))
                {
                    temp.Add(attack);
                    Debug.Log("Adding " + attack.name + " to list. Spell bool is " + spell);
                }
                else if (spell && attack.magical && !temp.Contains(attack))
                {
                    temp.Add(attack);
                    Debug.Log("Adding " + attack.name + " to list. Spell bool is " + spell);
                }
            }
        }
        else if (weaponState != BattleController.WeaponState.Choosing)
        {
            switch (weaponState)
            {
                default:

                    break;

                case BattleController.WeaponState.Right:

                    foreach (Attack attack in weaponR.attacksList)
                    {
                        if (!spell && !attack.magical && !temp.Contains(attack))
                        {
                            temp.Add(attack);
                        }
                        else if (spell && attack.magical && !temp.Contains(attack))
                        {
                            temp.Add(attack);
                        }
                    }
                    break;

                case BattleController.WeaponState.Left:

                    foreach (Attack attack in weaponL.attacksList)
                    {
                        if (!spell && !attack.magical && !temp.Contains(attack))
                        {
                            temp.Add(attack);
                        }
                        else if (spell && attack.magical && !temp.Contains(attack))
                        {
                            temp.Add(attack);
                        }
                    }
                    break;

                case BattleController.WeaponState.None:

                    foreach (Attack attack in InventoryScript.Instance.defaultWeapon.attacksList)
                    {
                        if (!spell && !attack.magical && !temp.Contains(attack))
                        {
                            temp.Add(attack);
                        }
                        else if (spell && attack.magical && !temp.Contains(attack))
                        {
                            temp.Add(attack);
                        }
                    }
                    break;
            }
        }
        else
        {
            Debug.LogError("No weapon or weapon state specified in 'GetAttacksList'! List will be incomplete!");
        }

        foreach (Trait trait in equippedTraits)
        {
            foreach (Attack attack in trait.attacks)
            {
                if (!spell && !attack.magical && !temp.Contains(attack))
                {
                    temp.Add(attack);
                }
                else if (spell && attack.magical && !temp.Contains(attack))
                {
                    temp.Add(attack);
                }
            }
        }

        return temp;
    }

    public void DefaultValues()
    {
        offendDelegateStatuses = new List<StatusEffect>();
        defendDelegateStatuses = new List<StatusEffect>();

        /*for (int n = 0; n < damageTypes.Count && n < damageTypePercentages.Count; n++)
        {
            damageTypeModifier.Add(damageTypes[n], damageTypePercentages[n]);
        }*/

        /*int strenCount = 1;
        int magCount = 1;
        int resCount = 1;
        int magresCount = 1;
        int spdCount = 1;*/

        luck = baseLuck;
        speed = baseSpeed;
        maxHealth = baseMaxHealth;
        maxMagic = baseMaxMagic;
        strength = baseStrength;
        intelligence = baseIntelligence;
        resistance = baseResistance;
        spirit = baseSpirit;
        persistance = basePersistance;
        immunity = baseImmunity;
        defender = null;
        reflect = false;

        foreach (Item equip in EquipList)
        {
            if (equip != null)
            {

                if (equip.Health > 0 && (equip.Type == Item.ItemType.Weapon || equip.Type == Item.ItemType.TwoHandedWeapon))
                {
                    //strength += strength * (equip.Health / 100);
                    //strength *= equip.Health;
                    //strenCount++;
                }
                else if (equip.Health > 0)
                {
                    maxHealth += (baseMaxHealth * equip.Health / 100);
                }
                if (equip.Magic > 0 && (equip.Type == Item.ItemType.Weapon || equip.Type == Item.ItemType.TwoHandedWeapon))
                {
                    //intelligence += intelligence * (equip.Magic / 100);
                    //intelligence *= equip.Magic;
                    //magCount++;
                }
                else if (equip.Magic > 0)
                {
                    maxMagic += (baseMaxMagic * equip.Magic / 100);
                }
                if (equip.Resistance > 0)
                {
                    resistance += resistance * (equip.Resistance / 100);
                    //resistance *= equip.Resistance;
                    //resCount++;
                }
                if (equip.MagicResistance > 0)
                {
                    spirit += spirit * (equip.MagicResistance / 100);
                    //spirit *= equip.MagicResistance;
                    //magresCount++;
                }
                if (Mathf.Abs(equip.Speed) > 0)
                {
                    speed += baseSpeed * (equip.Speed / 100);
                    //speed *= equip.Speed;
                    //spdCount++;
                }

                /*if (equip.Effects.Count > 0)
                {
                    foreach (StatusEffect effect in equip.Effects)
                    {
                        AddStatusEffect(effect, this);
                    }
                }*/
            }
        }

        /*strength /= strenCount;
        intelligence /= magCount;
        resistance /= resCount;
        spirit /= magresCount;
        speed /= spdCount;*/

        if (health <= 0)
        {
            Debug.Log(gameObject.name + " is at " + (float)health / maxHealth * 100 + "% health. ( " + health + "/" + maxHealth + " ) Dead.");
            aiState = AiState.Stun;
        }
        else if (((float)health / maxHealth * 100) <= panicAt && gameObject.tag == "Enemy")
        {
            Debug.Log(gameObject.name + " is at " + (float)health / maxHealth * 100 + "% health. ( " + health + "/" + maxHealth + " ) Starting to panic!");
            aiState = AiState.Panic;
        }
        else if (((float)health / maxHealth * 100) <= cautionAt && gameObject.tag == "Enemy")
        {
            Debug.Log(gameObject.name + " is at " + (float)health / maxHealth * 100 + "% health. ( " + health + "/" + maxHealth + " ) Starting to be cautious...");
            aiState = AiState.Caution;
        }
        else if (gameObject.tag == "Enemy")
        {
            Debug.Log(gameObject.name + " is at " + (float)health / maxHealth * 100 + "% health. ( " + health + "/" + maxHealth + " ) Using standard AI.");
            aiState = AiState.Enemy;
        }

    }

    private void Update()
    {
        // normalize HP and MP
        /*if (health > maxHealth)
        {
            health = maxHealth;
        }
        else if (health < 0)
        {
            health = 0;
            Debug.Log("dead");
        }
        if (magic > maxMagic)
        {
            magic = maxMagic;
        }
        else if (magic < 0)
        {
            magic = 0;
        }*/
    }

    public void ModifyHealth(int hp, bool displayMessage = true)
    {
        health += hp;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        else if (health < 0)
        {
            health = 0;
            Debug.Log("Dead");
        }

        switch (LoadInfo.Instance.gameState)
        {
            case LoadInfo.GameState.Battle:
                if (hp > 0 && displayMessage)
                {
                    //battleController.DisplayText("<color=yellow>" + gameObject.name + " gained " + hp + " health!</color>");
                    battleController.DisplayText("<color=yellow>" + hp + " HP</color>", gameObject);
                }
                else if (displayMessage)
                {
                    //battleController.DisplayText("<color=red>" + gameObject.name + " lost " + -hp + " health!</color>");
                    battleController.DisplayText("<color=red>" + hp + " HP</color>", gameObject);
                }

                if (health <= 0)
                {
                    //battleController.DisplayText("\n<color=red>" + gameObject.name + " has died!</color>");
                    battleController.DisplayText("<color=red>Dead\n</color>", gameObject);
                    aiState = AiState.Stun;

                    if (gameObject.tag == "Enemy")
                    {
                        //Destroy(gameObject);
                        //turnNum = 0;

                    }
                }
                //battleController.UpdateStatsText();
                break;
        }
    }

    public void ModifyMagic(int mp, bool displayMessage = true)
    {
        magic += mp;
        if (magic > maxMagic)
        {
            magic = maxMagic;
        }
        else if (magic < 0)
        {
            magic = 0;
            Debug.LogWarning("Magic fell below 0!");
        }

        switch (LoadInfo.Instance.gameState)
        {
            case LoadInfo.GameState.Battle:
                if (mp > 0)
                {
                    //battleController.DisplayText("<color=cyan>" + gameObject.name + " gained " + mp + " magic!</color>");
                    battleController.DisplayText("<color=cyan>" + mp + " MP</color>", gameObject);
                }
                else
                {
                    //battleController.DisplayText("<color=navy>" + gameObject.name + " lost " + mp * -1 + " magic!</color>");
                    battleController.DisplayText("<color=navy>" + mp + " MP</color>", gameObject);
                }
                battleController.UpdateStatsText();
                break;
        }
    }

    public void ModifyExperience(int xp)
    {
        int count = 0;
        experience += xp;

        while (experience >= level * 100 || count > 100)
        {
            count++;
            experience -= level * 100;
            LevelUp();
            if (count > 100)
            {
                Debug.LogError("Possible infinity error; character: " + name + " has gained more than 100 levels in one operation.\nCurrent level: " + level + ". Local exp leftover: " + experience);
                Debug.Break();
            }
        }

        if (experience < 0)
        {
            experience = 0;
            // Just in case anything removes xp, which nothing probably will
        }
    }

    public void LevelUp()
    {
        if (level < 1)
        {
            level = 1;
            // Level 0 isn't a thing here
        }

        Debug.Log(name + " is leveling up!");
        level += 1;

        // To do: also increase equip points and unlock points
    }

    // dealing with status effects
    public void AddStatusEffect(StatusEffect statusEffect, CharacterScript caster)
    {
        StatusEffect temp = new StatusEffect(statusEffect, caster);

        if (temp.positive)
        {
            switch (temp.statusDelegateName) // To do: maybe change power and duration for both
            {
                default:

                    temp.statusPower += (int)((float)temp.statusPower * caster.persistance / 100f);
                    break;

                case "ModifyStat":

                    temp.statusPower += (int)((float)temp.statusPower * caster.persistance / 100f);
                    break;

                case "ChangeAi":
                case "AddDelegate":
                case "RemoveStatus":

                    temp.statusDuration += (int)((float)temp.statusDuration * caster.persistance / 100f);
                    break;
            }
        }
        else
        {
            switch (temp.statusDelegateName) // To do: maybe change power and duration for both
            {
                default:

                    temp.statusPower = (int)((float)temp.statusPower * ((((caster.persistance - immunity) / 100f) + 1) + ((float)caster.persistance / immunity)) / 2f);
                    break;

                case "ModifyStat":

                    temp.statusPower = (int)((float)temp.statusPower / ((((caster.persistance - immunity) / 100f) + 1) + ((float)caster.persistance / immunity)) / 2f);
                    break;

                case "ChangeAi":
                case "AddDelegate":
                case "RemoveStatus":

                    //Debug.Log(temp.statusDuration + " * (" + (((caster.persistance - immunity) / 100f) + 1) + " + " + ((float)caster.persistance / immunity) + ") / 2");
                    temp.statusDuration = (int)((float)temp.statusDuration * ((((caster.persistance - immunity) / 100f) + 1) + ((float)caster.persistance / immunity)) / 2f);
                    break;
            }
        }

        if (((Random.Range(1, 100) + caster.luck - luck) <= temp.chance && !temp.positive) || ((Random.Range(1, 100)) <= temp.chance && temp.positive))
        {
            if (temp.statusDelegateName == "revive")
            {
                SpecialAttackFunctions.instance.statusDelegates["revive"](this, temp.caster, temp);
            }
            else if (SpecialAttackFunctions.instance.statusDelegates.ContainsKey(temp.statusDelegateName))
            {
                statusEffects.Add(temp);
                Debug.Log("Adding status effect. Remove ID: " + temp.removeID);
                if (temp.name != "")
                {
                    battleController.DisplayText(gameObject.name + " has gotten the status effect: " + temp.name + " for " + temp.statusDuration + " turns from " + caster.gameObject.name + ".");
                }
            }
            else
            {
                Debug.LogWarning("Unknown status delegate: '" + temp.statusDelegateName + ".' Remove ID: " + temp.removeID);
            }
        }
        else if (temp.name != "")
        {
            battleController.DisplayText("The status effect: " + temp.name + " wasn't applied!");
        }
        /*if (temp.delay <= 0)
        {
            CalculateStatusEffects(temp);
        }
        else
        {
            temp.delay -= 1;
        }*/
        temp.delay -= 1;
    }

    public void CalculateStatusEffects()
    {
        List<StatusEffect> deleteList = new List<StatusEffect>();

        DefaultValues(); // Reset values so the effect isn't magnified every turn
                         //Debug.Log(statusEffects.Count);
        foreach (Trait trait in equippedTraits)
        {
            if (trait.effects.Count > 0)
            {
                foreach (StatusEffect effect in trait.effects)
                {
                    //AddStatusEffect(effect, this);
                    if (SpecialAttackFunctions.instance.statusDelegates.ContainsKey(effect.statusDelegateName) && effect.delay <= 0)
                    {
                        SpecialAttackFunctions.instance.statusDelegates[effect.statusDelegateName](this, effect.caster, effect);
                    }
                    else if (effect.delay <= 0)
                    {
                        Debug.LogWarning("Status delegate: " + effect.statusDelegateName + " not found! Remove ID: " + effect.removeID);
                    }
                }
            }
        }

        foreach (Item equip in EquipList)
        {
            if (equip != null)
            {
                if (equip.statusEffects.Count > 0)
                {
                    foreach (StatusEffect effect in equip.statusEffects)
                    {
                        //AddStatusEffect(effect, this);
                        if (SpecialAttackFunctions.instance.statusDelegates.ContainsKey(effect.statusDelegateName) && effect.delay <= 0)
                        {
                            SpecialAttackFunctions.instance.statusDelegates[effect.statusDelegateName](this, effect.caster, effect);
                        }
                        else if (effect.delay <= 0)
                        {
                            Debug.LogWarning("Status delegate: " + effect.statusDelegateName + " not found! Remove ID: " + effect.removeID);
                        }
                    }
                }
            }
        }

        foreach (StatusEffect effect in statusEffects)
        {

            if (SpecialAttackFunctions.instance.statusDelegates.ContainsKey(effect.statusDelegateName) && effect.delay <= 0)
            {
                SpecialAttackFunctions.instance.statusDelegates[effect.statusDelegateName](this, effect.caster, effect);
                effect.statusDuration -= 1;
            }
            else if (effect.delay <= 0)
            {
                Debug.LogWarning("Status delegate: " + effect.statusDelegateName + " not found! Remove ID: " + effect.removeID);
                effect.statusDuration -= 1;
            }

            effect.delay -= 1;

            if (effect.name != "" && effect.delay <= 0)
            {
                battleController.DisplayText("The status effect: " + effect.name + " has " + (effect.statusDuration) + " turns left");
            }
            else if (effect.name != "")
            {
                battleController.DisplayText("The status effect: " + effect.name + " won't go away on its own!");
            }

            if (effect.statusDuration == 0 && effect.delay <= 0)
            {
                deleteList.Add(effect);
            }
        }


        foreach (StatusEffect effect in deleteList)
        {
            if (statusEffects.Contains(effect))
            {
                statusEffects.Remove(effect);
            }

        }
    }

    // AI: controlls the enemies
    void AI()
    {

        if (BattleController.turn == gameObject)
        {

            List<Attack> attacksList = new List<Attack>();
            Attack mostFitAtk = null;
            GameObject mostFitObj = null; ;
            /*int mostFitY = 0;
            int mostFitZ = 0;
            bool w = false; // Initial check for positive attacks
            int x = 0; // Initial check for minimum magic required to cast a spell
            int y = 0; // Temporary character score
            int z = 0; // Temporary attack score*/
            bool terrible = false;
            Item weapon = null;
            int mostFitScore = 0;
            int objScore = 0;
            int atkScore = 0;
            int finalScore = 0;

            /*foreach (Attack attack in GetAttackList(battleController.currentWeapon, false))
            {
                attacksList.Add(attack);
                if (attack.positive)
                {
                    w = true;
                }
            }
            foreach (Attack attack in GetAttackList(battleController.currentWeapon, true))
            {
                attacksList.Add(attack);
                if (attack.positive)
                {
                    w = true;
                }
                if (attack.magicRequired < x)
                {
                    x = attack.magicRequired;
                }
            }*/
            if (aiState != AiState.Player)
            {
                switch (battleController.currentWeapon)
                {
                    case BattleController.WeaponState.Right:
                        weapon = weaponR;
                        break;

                    case BattleController.WeaponState.Left:
                        weapon = weaponL;
                        break;

                    case BattleController.WeaponState.None:
                        weapon = InventoryScript.Instance.defaultWeapon;
                        //Debug.LogError("Trying to run AI with no weapon equipped!");
                        break;
                }

                foreach (Attack attack in GetAttackList(false, battleController.currentWeapon))
                {
                    attacksList.Add(attack);
                }
                foreach (Attack attack in GetAttackList(true, battleController.currentWeapon))
                {
                    attacksList.Add(attack);
                }

                Debug.Log("It's my turn! -" + gameObject.name + " : " + aiState);
            }

            switch (aiState)
            {
                case AiState.Player:

                    Debug.Log("Giving controll of " + gameObject.name + " to player");
                    battleController.inputState = BattleController.InputState.Category;
                    battleController.UpdateInputArea();
                    break;

                case AiState.Enemy:

                    foreach (GameObject obj in BattleController.charList)
                    {
                        foreach (Attack atk in attacksList)
                        {
                            CharacterScript charScript = BattleController.scriptDict[obj];
                            terrible = false;
                            objScore = 0;
                            atkScore = 0;
                            finalScore = 0;

                            if (LoadInfo.Instance.hardMode) // Hard mode
                            {
                            // Attacking teammate or healing enemy
                                if ((obj.tag == "Enemy" && !atk.positive) || (obj.tag != "Enemy" && atk.positive)) 
                                {
                                    terrible = true;
                                }
                            // Healing/buffing teammate
                                else if (obj.tag == "Enemy" && atk.positive)
                                {
                                    /*if (atk.damageModifier < 0 && atk.magical)
                                    {
                                        objScore += charScript.maxHealth - charScript.health;
                                        atkScore += weapon.Magic * intelligence / 100;
                                    }
                                    else if (atk.damageModifier < 0)
                                    {
                                        objScore += charScript.maxHealth - charScript.health;
                                        atkScore += weapon.Health * strength / 100;
                                    }*/

                                    if (atk.positive) // To do: impliment switch statement for attack delegates here too
                                    {
                                        finalScore -= atk.magical ?
                                        (int)((float)weapon.Magic * atk.damageModifier / 100f * ((intelligence / 100f) + 1)) :
                                        (int)((float)weapon.Health * atk.damageModifier / 100f * ((strength / 100f) + 1));
                                    }
                                    else
                                    {
                                        finalScore -= atk.magical ?
                                        (int)((float)weapon.Magic * atk.damageModifier / 100f * ((((intelligence - charScript.spirit) / 100f) + 1) + ((float)intelligence / charScript.spirit) / 2f)) :
                                        (int)((float)weapon.Health * atk.damageModifier / 100f * ((((strength - charScript.resistance) / 100f) + 1) + ((float)strength / charScript.resistance) / 2f));
                                    }

                                    foreach (StatusEffect effect in atk.statusEffects)
                                    {
                                        switch (effect.statusDelegateName)
                                        {
                                            case "DamageOverTime":


                                                break;
                                        }
                                    }

                                    objScore += charScript.maxHealth - charScript.health;
                                    objScore += charScript.threat;
                                    objScore += charScript.strength;
                                    objScore += (int)(charScript.intelligence * (float)charScript.magic / charScript.maxMagic);

                                    switch (charScript.aiState)
                                    {
                                        default:
                                        case AiState.Enemy:
                                        case AiState.Stun:

                                            break;

                                        case AiState.Caution:

                                            objScore = (int)(objScore * 1.5f);
                                            break;

                                        case AiState.Panic:

                                            objScore = (int)(objScore * 2f);
                                            break;

                                        case AiState.Confused:
                                        case AiState.Player:

                                            objScore = (int)(objScore * 0.5f);
                                            break;
                                    }
                                }
                            // Attacking enemy
                                else
                                {
                                    for (int n = 0; n < atk.delegates.Count; n++)
                                    {
                                        int temp = 0;
                                        switch (atk.delegates[n].delegateName)
                                        {
                                            case "Steal":

                                                foreach (KeyValuePair<Item, int> pair in InventoryScript.Instance.inventory)
                                                {
                                                    temp += pair.Key.Value;
                                                }
                                                temp /= InventoryScript.Instance.inventory.Count;
                                                break;

                                            case "NearDeathExperience":
                                            case "LifeSteal":

                                                temp = maxHealth - health;
                                                break;

                                            case "SwitchPosition":

                                                // To do: SwitchPosition and LifeSteal need some math
                                                break;

                                            default:
                                            case "StandardAttack":

                                                if (atk.positive)
                                                {
                                                    temp += atk.magical ?
                                                    (int)((float)weapon.Magic * atk.damageModifier / 100f * ((intelligence / 100f) + 1)) :
                                                    (int)((float)weapon.Health * atk.damageModifier / 100f * ((strength / 100f) + 1));
                                                }
                                                else
                                                {
                                                    temp += atk.magical ?
                                                    (int)((float)weapon.Magic * atk.damageModifier / 100f * ((((intelligence - charScript.spirit) / 100f) + 1) + ((float)intelligence / charScript.spirit) / 2f)) :
                                                    (int)((float)weapon.Health * atk.damageModifier / 100f * ((((strength - charScript.resistance) / 100f) + 1) + ((float)strength / charScript.resistance) / 2f));
                                                }
                                                temp += (int)((float)atkScore * (atk.critChance - 100 - atk.hitChance + luck - charScript.speed) / 100);

                                                if (temp >= charScript.health) // This is likely to kill them
                                                {
                                                    temp *= 3;
                                                }
                                                break;
                                        }

                                        atkScore += temp;
                                    }

                                    objScore += charScript.health;
                                    objScore += charScript.threat;
                                    objScore += charScript.strength;
                                    objScore += (int)(charScript.intelligence * (float)charScript.magic / charScript.maxMagic);
                                    if (charScript.aiState == AiState.Enemy)
                                    {
                                        objScore = (int)(objScore * 0.5f);
                                    }

                                    foreach (StatusEffect effect in atk.statusEffects)
                                    {

                                    }

                                    if (atk.magicRequired > magic)
                                    {
                                        terrible = true;
                                    }
                                    else if (atk.magicRequired > 0)
                                    {
                                        atkScore -= (int)((float)atkScore * atk.magicRequired / magic);
                                    }


                                }

                                finalScore = objScore + atkScore;
                                //Debug.Log("Obj: " + obj + "\nAtk: " + atk + "\nScore: " + finalScore + " (terrible = " + terrible + ")");

                                if (finalScore >= mostFitScore && !terrible)
                                {
                                    mostFitScore = finalScore;
                                    mostFitObj = obj;
                                    mostFitAtk = atk;
                                }
                            }

                            else // Normal mode
                            {
                                if (atk.magicRequired > magic || (atk.positive && obj.tag != gameObject.tag) || (!atk.positive && obj.tag == gameObject.tag) || charScript.health <= 0)
                                {
                                    terrible = true;
                                }

                                finalScore = Random.Range(1, 100);
                                Debug.Log("Obj: " + obj.name + "\nAtk: " + atk.name + "\nScore: " + finalScore + " (terrible = " + terrible + ")");

                                if (finalScore > mostFitScore && !terrible)
                                {
                                    mostFitScore = finalScore;
                                    mostFitObj = obj;
                                    mostFitAtk = atk;
                                }
                                else if (finalScore == mostFitScore && !terrible && Random.Range(0, 1) == 1)
                                {
                                    mostFitScore = finalScore;
                                    mostFitObj = obj;
                                    mostFitAtk = atk;
                                }
                            }
                        }
                    }

                    break;

                case AiState.Caution:

                    foreach (GameObject obj in BattleController.charList)
                    {
                        foreach (Attack atk in attacksList)
                        {
                            CharacterScript charScript = BattleController.scriptDict[obj];
                            terrible = false;
                            objScore = 0;
                            atkScore = 0;
                            finalScore = 0;

                            if (LoadInfo.Instance.hardMode) // Hard mode
                            {
                                // Attacking teammate or healing enemy
                                if ((obj.tag == tag && !atk.positive) || (obj.tag != tag && atk.positive))
                                {
                                    terrible = true;
                                }
                                // Healing/buffing teammate
                                else if (obj.tag == tag && atk.positive)
                                {
                                    /*if (atk.damageModifier < 0 && atk.magical)
                                    {
                                        objScore += charScript.maxHealth - charScript.health;
                                        atkScore += weapon.Magic * intelligence / 100;
                                    }
                                    else if (atk.damageModifier < 0)
                                    {
                                        objScore += charScript.maxHealth - charScript.health;
                                        atkScore += weapon.Health * strength / 100;
                                    }*/

                                    if (atk.positive) // To do: impliment switch statement for attack delegates here too
                                    {
                                        finalScore -= atk.magical ?
                                        (int)((float)weapon.Magic * atk.damageModifier / 100f * ((intelligence / 100f) + 1)) :
                                        (int)((float)weapon.Health * atk.damageModifier / 100f * ((strength / 100f) + 1));
                                    }
                                    else
                                    {
                                        finalScore -= atk.magical ?
                                        (int)((float)weapon.Magic * atk.damageModifier / 100f * ((((intelligence - charScript.spirit) / 100f) + 1) + ((float)intelligence / charScript.spirit) / 2f)) :
                                        (int)((float)weapon.Health * atk.damageModifier / 100f * ((((strength - charScript.resistance) / 100f) + 1) + ((float)strength / charScript.resistance) / 2f));
                                    }

                                    foreach (StatusEffect effect in atk.statusEffects)
                                    {
                                        switch (effect.statusDelegateName)
                                        {
                                            case "DamageOverTime":


                                                break;
                                        }
                                    }

                                    objScore += charScript.maxHealth - charScript.health;
                                    objScore += charScript.threat;
                                    objScore += charScript.strength;
                                    objScore += (int)(charScript.intelligence * (float)charScript.magic / charScript.maxMagic);

                                    switch (charScript.aiState)
                                    {
                                        default:
                                        case AiState.Enemy:
                                        case AiState.Stun:

                                            break;

                                        case AiState.Caution:

                                            objScore = (int)(objScore * 1.5f);
                                            break;

                                        case AiState.Panic:

                                            objScore = (int)(objScore * 2f);
                                            break;

                                        case AiState.Confused:
                                        case AiState.Player:

                                            objScore = (int)(objScore * 0.5f);
                                            break;
                                    }
                                }
                                // Attacking enemy
                                else
                                {
                                    for (int n = 0; n < atk.delegates.Count; n++)
                                    {
                                        int temp = 0;
                                        switch (atk.delegates[n].delegateName)
                                        {
                                            case "Steal":

                                                foreach (KeyValuePair<Item, int> pair in InventoryScript.Instance.inventory)
                                                {
                                                    temp += pair.Key.Value;
                                                }
                                                temp /= InventoryScript.Instance.inventory.Count;
                                                break;

                                            case "NearDeathExperience":
                                            case "LifeSteal":

                                                temp = maxHealth - health;
                                                break;

                                            case "SwitchPosition":

                                                // To do: SwitchPosition and LifeSteal need some math
                                                break;

                                            default:
                                            case "StandardAttack":

                                                if (atk.positive)
                                                {
                                                    temp += atk.magical ?
                                                    (int)((float)weapon.Magic * atk.damageModifier / 100f * ((intelligence / 100f) + 1)) :
                                                    (int)((float)weapon.Health * atk.damageModifier / 100f * ((strength / 100f) + 1));
                                                }
                                                else
                                                {
                                                    temp += atk.magical ?
                                                    (int)((float)weapon.Magic * atk.damageModifier / 100f * ((((intelligence - charScript.spirit) / 100f) + 1) + ((float)intelligence / charScript.spirit) / 2f)) :
                                                    (int)((float)weapon.Health * atk.damageModifier / 100f * ((((strength - charScript.resistance) / 100f) + 1) + ((float)strength / charScript.resistance) / 2f));
                                                }
                                                temp += (int)((float)atkScore * (atk.critChance - 100 - atk.hitChance + luck - charScript.speed) / 100);

                                                if (temp >= charScript.health) // This is likely to kill them
                                                {
                                                    temp *= 3;
                                                }
                                                break;
                                        }

                                        atkScore += temp;
                                    }

                                    objScore += charScript.health;
                                    objScore += charScript.threat;
                                    objScore += charScript.strength;
                                    objScore += (int)(charScript.intelligence * (float)charScript.magic / charScript.maxMagic);
                                    if (charScript.aiState == AiState.Enemy)
                                    {
                                        objScore = (int)(objScore * 0.5f);
                                    }

                                    foreach (StatusEffect effect in atk.statusEffects)
                                    {

                                    }

                                    if (atk.magicRequired > magic)
                                    {
                                        terrible = true;
                                    }
                                    else if (atk.magicRequired > 0)
                                    {
                                        atkScore -= (int)((float)atkScore * atk.magicRequired / magic);
                                    }


                                }

                                finalScore = objScore + atkScore;
                                //Debug.Log("Obj: " + obj + "\nAtk: " + atk + "\nScore: " + finalScore + " (terrible = " + terrible + ")");

                                if (finalScore >= mostFitScore && !terrible)
                                {
                                    mostFitScore = finalScore;
                                    mostFitObj = obj;
                                    mostFitAtk = atk;
                                }
                            }

                            else // Normal mode
                            {
                                if (atk.magicRequired > magic || (atk.positive && obj.tag != gameObject.tag) || (!atk.positive && obj.tag == gameObject.tag) || charScript.health <= 0)
                                {
                                    terrible = true;
                                }

                                finalScore = Random.Range(1, 100);
                                Debug.Log("Obj: " + obj.name + "\nAtk: " + atk.name + "\nScore: " + finalScore + " (terrible = " + terrible + ")");

                                if (finalScore > mostFitScore && !terrible)
                                {
                                    mostFitScore = finalScore;
                                    mostFitObj = obj;
                                    mostFitAtk = atk;
                                }
                                else if (finalScore == mostFitScore && !terrible && Random.Range(0, 1) == 1)
                                {
                                    mostFitScore = finalScore;
                                    mostFitObj = obj;
                                    mostFitAtk = atk;
                                }
                            }
                        }
                    }
                    break;

                case AiState.Panic:

                    foreach (GameObject obj in BattleController.charList)
                    {
                        foreach (Attack atk in attacksList)
                        {
                            CharacterScript charScript = BattleController.scriptDict[obj];
                            terrible = false;
                            objScore = 0;
                            atkScore = 0;
                            finalScore = 0;

                            if (LoadInfo.Instance.hardMode) // Hard mode
                            {
                                // Attacking teammate or healing enemy
                                if ((obj.tag == tag && !atk.positive) || (obj.tag != tag && atk.positive))
                                {
                                    terrible = true;
                                }
                                // Healing/buffing teammate
                                else if (obj.tag == tag && atk.positive)
                                {
                                    /*if (atk.damageModifier < 0 && atk.magical)
                                    {
                                        objScore += charScript.maxHealth - charScript.health;
                                        atkScore += weapon.Magic * intelligence / 100;
                                    }
                                    else if (atk.damageModifier < 0)
                                    {
                                        objScore += charScript.maxHealth - charScript.health;
                                        atkScore += weapon.Health * strength / 100;
                                    }*/

                                    if (atk.positive) // To do: impliment switch statement for attack delegates here too
                                    {
                                        finalScore -= atk.magical ?
                                        (int)((float)weapon.Magic * atk.damageModifier / 100f * ((intelligence / 100f) + 1)) :
                                        (int)((float)weapon.Health * atk.damageModifier / 100f * ((strength / 100f) + 1));
                                    }
                                    else
                                    {
                                        finalScore -= atk.magical ?
                                        (int)((float)weapon.Magic * atk.damageModifier / 100f * ((((intelligence - charScript.spirit) / 100f) + 1) + ((float)intelligence / charScript.spirit) / 2f)) :
                                        (int)((float)weapon.Health * atk.damageModifier / 100f * ((((strength - charScript.resistance) / 100f) + 1) + ((float)strength / charScript.resistance) / 2f));
                                    }

                                    foreach (StatusEffect effect in atk.statusEffects)
                                    {
                                        switch (effect.statusDelegateName)
                                        {
                                            case "DamageOverTime":


                                                break;
                                        }
                                    }

                                    objScore += charScript.maxHealth - charScript.health;
                                    objScore += charScript.threat;
                                    objScore += charScript.strength;
                                    objScore += (int)(charScript.intelligence * (float)charScript.magic / charScript.maxMagic);

                                    switch (charScript.aiState)
                                    {
                                        default:
                                        case AiState.Enemy:
                                        case AiState.Stun:

                                            break;

                                        case AiState.Caution:

                                            objScore = (int)(objScore * 1.5f);
                                            break;

                                        case AiState.Panic:

                                            objScore = (int)(objScore * 2f);
                                            break;

                                        case AiState.Confused:
                                        case AiState.Player:

                                            objScore = (int)(objScore * 0.5f);
                                            break;
                                    }
                                }
                                // Attacking enemy
                                else
                                {
                                    for (int n = 0; n < atk.delegates.Count; n++)
                                    {
                                        int temp = 0;
                                        switch (atk.delegates[n].delegateName)
                                        {
                                            case "Steal":

                                                foreach (KeyValuePair<Item, int> pair in InventoryScript.Instance.inventory)
                                                {
                                                    temp += pair.Key.Value;
                                                }
                                                temp /= InventoryScript.Instance.inventory.Count;
                                                break;

                                            case "NearDeathExperience":
                                            case "LifeSteal":

                                                temp = maxHealth - health;
                                                break;

                                            case "SwitchPosition":

                                                // To do: SwitchPosition and LifeSteal need some math
                                                break;

                                            default:
                                            case "StandardAttack":

                                                if (atk.positive)
                                                {
                                                    temp += atk.magical ?
                                                    (int)((float)weapon.Magic * atk.damageModifier / 100f * ((intelligence / 100f) + 1)) :
                                                    (int)((float)weapon.Health * atk.damageModifier / 100f * ((strength / 100f) + 1));
                                                }
                                                else
                                                {
                                                    temp += atk.magical ?
                                                    (int)((float)weapon.Magic * atk.damageModifier / 100f * ((((intelligence - charScript.spirit) / 100f) + 1) + ((float)intelligence / charScript.spirit) / 2f)) :
                                                    (int)((float)weapon.Health * atk.damageModifier / 100f * ((((strength - charScript.resistance) / 100f) + 1) + ((float)strength / charScript.resistance) / 2f));
                                                }
                                                temp += (int)((float)atkScore * (atk.critChance - 100 - atk.hitChance + luck - charScript.speed) / 100);

                                                if (temp >= charScript.health) // This is likely to kill them
                                                {
                                                    temp *= 3;
                                                }
                                                break;
                                        }

                                        atkScore += temp;
                                    }

                                    objScore += charScript.health;
                                    objScore += charScript.threat;
                                    objScore += charScript.strength;
                                    objScore += (int)(charScript.intelligence * (float)charScript.magic / charScript.maxMagic);
                                    if (charScript.aiState == AiState.Enemy)
                                    {
                                        objScore = (int)(objScore * 0.5f);
                                    }

                                    foreach (StatusEffect effect in atk.statusEffects)
                                    {

                                    }

                                    if (atk.magicRequired > magic)
                                    {
                                        terrible = true;
                                    }
                                    else if (atk.magicRequired > 0)
                                    {
                                        atkScore -= (int)((float)atkScore * atk.magicRequired / magic);
                                    }


                                }

                                finalScore = objScore + atkScore;
                                //Debug.Log("Obj: " + obj + "\nAtk: " + atk + "\nScore: " + finalScore + " (terrible = " + terrible + ")");

                                if (finalScore >= mostFitScore && !terrible)
                                {
                                    mostFitScore = finalScore;
                                    mostFitObj = obj;
                                    mostFitAtk = atk;
                                }
                            }

                            else // Normal mode
                            {
                                if (atk.magicRequired > magic || (atk.positive && obj.tag != gameObject.tag) || (!atk.positive && obj.tag == gameObject.tag) || charScript.health <= 0)
                                {
                                    terrible = true;
                                }

                                finalScore = Random.Range(1, 100);
                                Debug.Log("Obj: " + obj.name + "\nAtk: " + atk.name + "\nScore: " + finalScore + " (terrible = " + terrible + ")");

                                if (finalScore > mostFitScore && !terrible)
                                {
                                    mostFitScore = finalScore;
                                    mostFitObj = obj;
                                    mostFitAtk = atk;
                                }
                                else if (finalScore == mostFitScore && !terrible && Random.Range(0, 1) == 1)
                                {
                                    mostFitScore = finalScore;
                                    mostFitObj = obj;
                                    mostFitAtk = atk;
                                }
                            }
                        }
                    }
                    break;
                    
                case AiState.Confused:
                    int count = 0;
                    if (Random.Range(0, 100) < Random.Range(0, luck)) // A higher luck makes it more likely that you'll hit an enemy, not a teammate
                    {
                        Debug.Log("Got lucky! Attacking opponent");
                        do
                        {
                            mostFitAtk = GetAttackList(false, weapon: weapon)[Random.Range(0, GetAttackList(false, weapon: weapon).Count - 1)];
                            mostFitObj = BattleController.charList[Random.Range(0, BattleController.charList.Count - 1)];
                            count++;
                            if (count > 100)
                            {
                                Debug.LogError("Infinite loop in AI; Character is confused, and got lucky");
                                Debug.Break();
                                break;
                            }
                        }
                        while (((!mostFitAtk.positive && mostFitObj.tag != gameObject.tag) || (mostFitAtk.positive && mostFitObj.tag == gameObject.tag)) && mostFitAtk.magicRequired <= magic);
                    }
                    else
                    {
                        Debug.Log("Didn't get lucky! Attacking random");
                        /*do
                        {
                            mostFitAtk = GetAttackList(false)[Random.Range(0, attacksList.Count - 1)];
                            mostFitObj = BattleController.charList[Random.Range(0, BattleController.charList.Count - 1)];
                            count++;
                            if (count > 100)
                            {
                                Debug.LogError("Infinite loop in AI; Character is confused, and did not get lucky");
                                Debug.Break();
                                break;
                            }
                        }
                        while (mostFitAtk.magicRequired <= magic);*/

                        mostFitObj = BattleController.charList[Random.Range(0, BattleController.charList.Count - 1)];
                        mostFitAtk = weapon.attacksList[0]; // Auto attack. Might change back to making it random again.
                    }
                    break;

                case AiState.Berserk:

                    foreach (GameObject obj in BattleController.charList)
                    {
                        if (mostFitObj == null)
                        {
                            mostFitObj = obj;
                        }
                        else if (BattleController.scriptDict[obj].threat > BattleController.scriptDict[mostFitObj].threat)
                        {
                            mostFitObj = obj;
                        }
                    }

                    foreach (Attack atk in GetAttackList(false, weapon: weapon))
                    {
                        if (mostFitAtk == null)
                        {
                            mostFitAtk = atk;
                        }
                        else if (atk.damageModifier * (atk.critChance + luck) > mostFitAtk.damageModifier * (mostFitAtk.critChance + luck) && atk.magicRequired <= magic)
                        {
                            mostFitAtk = atk;
                        }

                    }
                    break;

                case AiState.Stun:

                    battleController.DisplayText(gameObject.name + " cannot move!");
                    battleController.NextTurn();
                    //CalculateStatusEffects();
                    break;
            }

            if (aiState != AiState.Player && aiState != AiState.Stun)
            {

                if (mostFitAtk != null && mostFitObj != null)
                {
                    battleController.DoTurnHolder(mostFitAtk, mostFitObj);
                    //Debug.Log("Attack: " + mostFitAtk.name);
                    //Debug.Log("Object: " + mostFitObj.name);
                }
                else
                {
                    Debug.LogError("AI couldn't find a good move!");
                    battleController.NextTurn();
                    //CalculateStatusEffects();
                    //Debug.Log("Attack: " + mostFitAtk);
                    //Debug.Log("Object: " + mostFitObj);
                }
            }
        }
    }
}

[System.Serializable]
public class CharacterData
{
    public string portraitPath;
    public string name;
    public int level;
    public int exp;

    public int health;
    public int baseMaxHealth;

    public int magic;
    public int baseMaxMagic;

    public int baseStrength; // Physical power

    public int baseResistance; // Physical resistance

    public int baseIntelligence; // Magical power

    public int baseSpirit; // Magical resistance

    public int basePersistance; // Status effect power

    public int baseImmunity; // Status effect resistance

    public int baseLuck; // Chance to hit/crit, and other luck based things

    public int baseSpeed; // Chance to dodge, frequency of turns

    public int inspiration; // Not completely sure what this will do

    public CharacterScript.Position position;

    public string weaponRName;
    public string weaponLName;
    public string armourName;
    public string accesoryName;

    public CharacterScript.AiState aiState;

    public List<StatusEffect> statusEffects = new List<StatusEffect>();

    public List<string> unlockedTraitNames = new List<string>();
    public List<string> equippedTraitNames = new List<string>();

    public List<string> equippedAttacksNames = new List<string>();
    public List<string> availableAttacksNames = new List<string>();

    public List<string> unlocks = new List<string>();
    public int unlockPoints;
    public int equipPointsMax;
    public int equipPointsUsed;

    public List<Attack.DamageType> valnurabilityTypes;
    public List<int> valnurabilityPercentages; // Keys and values have to be the same length!

    public CharacterData(CharacterScript charScript)
    {
        //portraitPath = AssetDatabase.GetAssetPath(charScript.portrait);
        portraitPath = "SpriteSheets/" + charScript.portrait.name;
        name = charScript.name;
        level = charScript.level;
        exp = charScript.experience;

        health = charScript.health;
        Debug.Log(charScript.name + "'s health: " + health);
        baseMaxHealth = charScript.baseMaxHealth;

        magic = charScript.magic;
        baseMaxMagic = charScript.baseMaxMagic;

        baseStrength = charScript.baseStrength;
        baseResistance = charScript.baseResistance;
        baseIntelligence = charScript.baseIntelligence;
        baseSpirit = charScript.baseSpirit;
        basePersistance = charScript.basePersistance;
        baseImmunity = charScript.baseImmunity;
        baseLuck = charScript.baseLuck;
        baseSpeed = charScript.baseSpeed;

        inspiration = charScript.inspiration;
        position = charScript.position;

        weaponRName = charScript.weaponR ? charScript.weaponR.name : "";
        weaponLName = charScript.weaponL ? charScript.weaponL.name : "";
        armourName = charScript.armour ? charScript.armour.name : "";
        accesoryName = charScript.accesory ? charScript.accesory.name : "";

        aiState = charScript.aiState;
        statusEffects = charScript.statusEffects;

        equippedTraitNames = new List<string>(); //charScript.equippedAttacks;
        unlockedTraitNames = new List<string>(); //charScript.availableAttacks;

        foreach (Trait trait in charScript.equippedTraits)
        {
            equippedTraitNames.Add(trait.name);
        }

        foreach (Trait trait in charScript.unlockedTraits)
        {
            availableAttacksNames.Add(trait.name);
        }

        unlockPoints = charScript.unlockPoints;
        equipPointsMax = charScript.equipPointsMax;
        equipPointsUsed = charScript.equipPointsUsed;

        valnurabilityTypes = charScript.valnurabilityTypes;
        valnurabilityPercentages = charScript.valnurabilityPercentages;

    }
}