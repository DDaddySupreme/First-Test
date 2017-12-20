using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpecialAttackFunctions : MonoBehaviour
{
    public static SpecialAttackFunctions instance;
    public BattleController battleController;
    public int totalDamage; // For connecting multiple delegates

    public delegate void OnAttackDelegate(CharacterScript char1, CharacterScript char2, Attack atk, Item weapon, SubAttack holder, bool crit); // Called when an attack is used to determine how the attack happens

    public delegate void OnOffendDelegate(CharacterScript char1, CharacterScript char2, Attack atk, Item weapon, StatusEffect effect); // Called when a character attacks another character

    public delegate void OnDefendDelegate(CharacterScript char1, CharacterScript char2, Attack atk, Item weapon, StatusEffect effect); // Called when a character is attacked

    public delegate void OnStatusDelegate(CharacterScript char1, CharacterScript char2, StatusEffect effect); // Called when a status effect is calculated. char2 is the person who caused the status

    public Dictionary<string, OnAttackDelegate> attackDelegates;
    public Dictionary<string, OnOffendDelegate> offendDelegates;
    public Dictionary<string, OnDefendDelegate> defendDelegates;
    public Dictionary<string, OnStatusDelegate> statusDelegates;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        attackDelegates = new Dictionary<string, OnAttackDelegate>()
        {
            { "StandardAttack", StandardAttack },
            { "PotionsAttack", PotionsAttack },
            { "NearDeathExperience", NearDeathExperience },
            { "Steal", Steal },
            { "LifeSteal", LifeSteal },
            { "SwitchPosition", SwitchPosition },
            { "Flee", Flee },
            { "SpawnCharacter" , SpawnCharacter }
        };

        offendDelegates = new Dictionary<string, OnOffendDelegate>()
        {
            { "Thorns", Thorns },
            { "ManaBurn", ManaBurn }
        };

        defendDelegates = new Dictionary<string, OnDefendDelegate>()
        {
            { "Retaliate", Retaliate },
        };

        statusDelegates = new Dictionary<string, OnStatusDelegate>()
        {
            { "ModifyStatValue", ModifyStatValue },
            { "ModifyStatPercent", ModifyStatPercent },
            { "DamageOverTime", DamageOverTime },
            { "Revive", Revive },
            { "AddDelegate", AddDelegate },
            { "ChangeAi", ChangeAi },
            { "RemoveStatus", RemoveStatus },
        };
    }
    // Char1 is attacker, char2 is target


    // OnAttack
    public void StandardAttack(CharacterScript char1, CharacterScript char2, Attack atk, Item weapon, SubAttack holder, bool crit)
    {
        int finalDamage = 0;

        if (atk.positive)
        {
            finalDamage = atk.magical ?
            (int)((float)weapon.Magic * atk.damageModifier / 100f * ((char1.intelligence / 100f) + 1)) :
            (int)((float)weapon.Health * atk.damageModifier / 100f * ((char1.strength / 100f) + 1));
        }
        else
        {
            finalDamage = atk.magical ?
            (int)((float)weapon.Magic * atk.damageModifier / 100f * ((((char1.intelligence - char2.spirit) / 100f) + 1) + ((float)char1.intelligence / char2.spirit) / 2f)) :
            (int)((float)weapon.Health * atk.damageModifier / 100f * ((((char1.strength - char2.resistance) / 100f) + 1) + ((float)char1.strength / char2.resistance) / 2f));
        }

        float positionModifier = 1;

        Debug.Log("Initial damage: " + finalDamage);

        if (crit)
        {
            finalDamage *= 2;
            battleController.DisplayText("<b><color=red>Critical hit!</color></b>");
        }

        switch (char2.position)
        {
            case CharacterScript.Position.Back:

                positionModifier += atk.ranged ? 0 : -0.25f;
                break;

            case CharacterScript.Position.Centered:


                break;

            case CharacterScript.Position.Front:

                positionModifier += atk.ranged ? 0 : 0.25f;
                break;

            case CharacterScript.Position.Flying:

                positionModifier += atk.ranged ? 0 : -0.25f;
                break;

            case CharacterScript.Position.Removed:

                positionModifier = atk.ranged ? 0 : 0f;
                break;
        }

        switch (char2.position)
        {
            case CharacterScript.Position.Back:

                positionModifier += atk.ranged ? 0 : -0.25f;
                break;

            case CharacterScript.Position.Centered:


                break;

            case CharacterScript.Position.Front:

                positionModifier += atk.ranged ? 0 : 0.25f;
                break;

            case CharacterScript.Position.Flying:

                if (atk.ranged)
                {
                    switch (char1.position)
                    {
                        case CharacterScript.Position.Back:

                            positionModifier = 0.75f;
                            break;

                        case CharacterScript.Position.Centered:
                        case CharacterScript.Position.Flying:

                            positionModifier = 1f;
                            break;

                        case CharacterScript.Position.Front:

                            positionModifier = 1.25f;
                            break;
                    }
                }
                else
                {
                    positionModifier = 0;
                }
                break;

            case CharacterScript.Position.Removed:

                positionModifier = atk.ranged ? 0 : 0f;
                break;
        }

        Debug.Log(char1.position + "->" + char2.position + " : " + positionModifier);
        finalDamage = (int)(finalDamage * positionModifier);

        /*if (char2.damageTypeModifier.ContainsKey(atk.damageType))
        {
            finalDamage = (int)((float)finalDamage * char2.damageTypeModifier[atk.damageType] / 100f);
        }*/

        for (int n = 0; n < char2.valnurabilityTypes.Count; n++)
        {
            if (char2.valnurabilityTypes[n] == atk.damageType && char2.valnurabilityPercentages.Count >= n)
            {
                finalDamage = (int)((float)finalDamage * char2.valnurabilityPercentages[n] / 100f);
            }
        }

        char2.ModifyHealth(-finalDamage);
        totalDamage += finalDamage;
        char1.threat += finalDamage;
    }

    public void PotionsAttack(CharacterScript char1, CharacterScript char2, Attack atk, Item weapon, SubAttack holder, bool crit)
    {
        int finalDamage = (int)(weapon.Health * atk.damageModifier / 100f);
        int finalMagic = (int)(weapon.Magic * atk.damageModifier / 100f); ;

        if (Mathf.Abs(finalDamage) > 0)
        {
            char2.ModifyHealth(-finalDamage);
        }
        if (Mathf.Abs(finalMagic) > 0)
        {
            char2.ModifyMagic(-finalMagic);
        }
    }

    public void Flee(CharacterScript char1, CharacterScript char2, Attack atk, Item weapon, SubAttack holder, bool crit)
    {
        // To do: add extra chance. Also add the thing itself
        char2.position = CharacterScript.Position.Removed;
        char2.aiState = CharacterScript.AiState.Stun;

    }

    public void SwitchPosition(CharacterScript char1, CharacterScript char2, Attack atk, Item weapon, SubAttack holder, bool crit)
    {
        switch (char2.position)
        {
            case CharacterScript.Position.Back:

                if ((holder.miscNum) > 0)
                {
                    char2.position = CharacterScript.Position.Centered;
                }
                else if ((holder.miscNum) < 0)
                {
                    char2.position = CharacterScript.Position.Back;
                }
                break;

            case CharacterScript.Position.Centered:

                if (holder.miscNum > 0)
                {
                    char2.position = CharacterScript.Position.Front;
                }
                else if (holder.miscNum < 0)
                {
                    char2.position = CharacterScript.Position.Back;
                }
                break;

            case CharacterScript.Position.Front:

                if (holder.miscNum > 0)
                {
                    char2.position = CharacterScript.Position.Front;
                }
                else if (holder.miscNum < 0)
                {
                    char2.position = CharacterScript.Position.Centered;
                }
                break;

            case CharacterScript.Position.Flying:

                if (holder.miscNum > 0)
                {
                    char2.position = CharacterScript.Position.Front;
                }
                else if (holder.miscNum < 0)
                {
                    char2.position = CharacterScript.Position.Back;
                }
                break;

            case CharacterScript.Position.Removed:

                Debug.Log("Trying to move a removed enemy");
                break;
        }

    }

    public void Steal(CharacterScript char1, CharacterScript char2, Attack atk, Item weapon, SubAttack holder, bool crit) // To do: impliment better chance mechanic (?) Also, reduce number of drops left in enemy inv
    {
        int index = 0;
        int amount = crit ? 2 : 1;
        List<Item> items;

        if (char2.gameObject.tag == "Enemy")
        {
            items = char2.dropList;
            if (items.Count <= 0)
            {
                battleController.DisplayText("Nothing to steal!");
                return;
            }
            index = UnityEngine.Random.Range(0, items.Count - 1);
            amount *= items[index].name == "Krn" ? 100 : 1;
            amount += items[index].name == "Krn" ? UnityEngine.Random.Range(-50, 50) : 0;

            if (char2.dropMax[index] >= amount)
            {
                InventoryScript.Instance.ModifyInventory(amount, items[index]);
                battleController.DisplayText("Stole " + amount + " " + items[index].name + "(s)");
            }
            else if (char2.dropMax[index] > 0)
            {
                InventoryScript.Instance.ModifyInventory(char2.dropMax[index], items[index]);
                battleController.DisplayText("Stole " + char2.dropMax[index] + " " + items[index].name + "(s)");
            }
            else
            {
                Debug.LogWarning("Trying to steal item from empty inventory! (contains no " + items[index].name + ")");
            }
        }
        else
        {
            items = InventoryScript.Instance.GetListType(new List<Item.ItemType>() { Item.ItemType.Consumable });
            if (items.Count <= 0)
            {
                battleController.DisplayText("Nothing to steal!");
                return;
            }
            index = UnityEngine.Random.Range(0, items.Count - 1);
            amount *= items[index].name == "Krn" ? 100 : 1;
            amount += items[index].name == "Krn" ? UnityEngine.Random.Range(-50, 50) : 0;

            if (InventoryScript.Instance.inventory[items[index]] >= amount)
            {
                InventoryScript.Instance.ModifyInventory(-amount, items[index]);
                battleController.DisplayText("Stole " + amount + " " + items[index].name + "(s)");
            }
            else if (InventoryScript.Instance.inventory[items[index]] > 0)
            {
                InventoryScript.Instance.ModifyInventory(InventoryScript.Instance.inventory[items[index]], items[index]);
                battleController.DisplayText("Stole " + InventoryScript.Instance.inventory[items[index]] + " " + items[index].name + "(s)");
            }
            else
            {
                Debug.LogWarning("Trying to steal item from empty inventory! (contains no " + items[index].name + ")");
            }
        }
    }

    public void LifeSteal(CharacterScript char1, CharacterScript char2, Attack atk, Item weapon, SubAttack holder, bool crit) // Has to be last in delegate list
    {
        int leach = (int)(totalDamage * holder.miscNum / 100f);
        char1.ModifyHealth(leach);
    }

    public void NearDeathExperience(CharacterScript char1, CharacterScript char2, Attack atk, Item weapon, SubAttack holder, bool crit)
    {
        int finalDamage = char1.maxHealth - char1.health;
        char2.ModifyHealth(-finalDamage);
        totalDamage += finalDamage;
    }

    public void SpawnCharacter(CharacterScript char1, CharacterScript char2, Attack atk, Item weapon, SubAttack holder, bool crit)
    {
        Debug.Log("Attempting to spawn 'Character Prefabs/Spawnables/" + holder.miscText + "'");
        GameObject obj = Resources.Load<GameObject>("Character Prefabs/Spawnables/" + holder.miscText);

        battleController.Spawn(obj, holder.miscNum, char1.tag);
    }

    // OnOffend
    public void Thorns(CharacterScript char1, CharacterScript char2, Attack atk, Item weapon, StatusEffect effect)
    {
        // Not like thorns in minecraft. Being covered in thorns is a bad thing, and moving around could be painful dangerous

        int finalDamage = effect.statusPower;

        finalDamage = (int)((float)finalDamage * char1.strength / char1.resistance);
        if (!atk.magical)
        {
            char1.ModifyHealth(-finalDamage);
            totalDamage += finalDamage;
        }
    }

    public void ManaBurn(CharacterScript char1, CharacterScript char2, Attack atk, Item weapon, StatusEffect effect)
    {
        // Using magic results in pain and possibly death

        int finalDamage = effect.statusPower;

        finalDamage = (int)((float)finalDamage * char1.intelligence / char1.spirit);
        if (atk.magical)
        {
            char1.ModifyHealth(-finalDamage);
            totalDamage += finalDamage;
        }
    }


    // OnDefend
    public void Retaliate(CharacterScript char1, CharacterScript char2, Attack atk, Item weapon, StatusEffect effect)
    {
        int finalDamage = 0;

        if (char1.weaponR != null)
        {
            if (!char1.weaponR.attacksList[0].magical)
            {
                finalDamage += (int)((float)char1.weaponR.Health * char1.weaponR.attacksList[0].damageModifier / 100f * char1.strength / char2.resistance);
            }
            else
            {
                finalDamage += (int)((float)char1.weaponR.Magic * char1.weaponR.attacksList[0].damageModifier / 100f * char1.intelligence / char2.spirit);
            }

        }

        if (char1.weaponL != null)
        {
            if (!char1.weaponL.attacksList[0].magical)
            {
                finalDamage += (int)((float)char1.weaponL.Health * char1.weaponL.attacksList[0].damageModifier / 100f * char1.strength / char2.resistance);
            }
            else
            {
                finalDamage += (int)((float)char1.weaponL.Magic * char1.weaponL.attacksList[0].damageModifier / 100f * char1.intelligence / char2.spirit);
            }
        }

        if (char1.weaponR == null && char1.weaponL == null)
        {
            if (!InventoryScript.Instance.defaultWeapon.attacksList[0].magical)
            {
                finalDamage += (int)((float)InventoryScript.Instance.defaultWeapon.Health * InventoryScript.Instance.defaultWeapon.attacksList[0].damageModifier / 100f * char1.strength / char2.resistance);
            }
            else
            {
                finalDamage += (int)((float)InventoryScript.Instance.defaultWeapon.Magic * InventoryScript.Instance.defaultWeapon.attacksList[0].damageModifier / 100f * char1.intelligence / char2.spirit);
            }
        }

        if (UnityEngine.Random.Range(1, 100) >= 100 - effect.statusPower + char1.luck - char2.speed)
        {
            battleController.DisplayText(char1.gameObject.name + "is counter-attacking!");
            char2.ModifyHealth(finalDamage);
            totalDamage += finalDamage;
        }
    }

    // OnStatus
    public void ModifyStatValue(CharacterScript char1, CharacterScript char2, StatusEffect effect) // Persistance and imunity are used in AddStatus, not here (I guess)
    {
        string txt = effect.statusMiscText;
        txt = txt.ToLower();
        txt.Replace(" ", "");
        txt.Replace("change", "");

        switch (txt) // With synonyms for convienence
        {
            case "maxhealth":
            case "health":
            case "maxhp":
            case "hp":

                char1.maxHealth += effect.statusPower;
                break;

            case "maxmagic":
            case "magic":
            case "maxmp":
            case "mp":

                char1.maxMagic += effect.statusPower;
                break;

            case "might":
            case "strength":

                char1.strength += effect.statusPower;
                break;

            case "constitution":
            case "resistance":

                char1.resistance += effect.statusPower;
                break;

            case "intelligence":
            case "magicpower":

                char1.intelligence += effect.statusPower;
                break;

            case "spirit":
            case "magicresistance":

                char1.spirit += effect.statusPower;
                break;

            case "persistance":

                char1.persistance += effect.statusPower;
                break;

            case "immunity":

                char1.immunity += effect.statusPower;
                break;

            case "luck":

                char1.luck += effect.statusPower;
                break;

            case "speed":

                char1.speed += effect.statusPower;
                break;

            case "inspiration":
            case "morale":

                // To do: make this stat do something
                break;

            default:

                Debug.LogWarning("Unknown stat in 'ModifyStat' status delegate. Status removeID: " + effect.removeID);
                break;
        }
    }

    public void ModifyStatPercent(CharacterScript char1, CharacterScript char2, StatusEffect effect) // Persistance and imunity are used in AddStatus, not here (I guess)
    {
        string txt = effect.statusMiscText;
        txt = txt.ToLower();
        txt.Replace(" ", "");
        txt.Replace("change", "");

        switch (txt) // With synonyms for convienence
        {
            case "maxhealth":
            case "health":
            case "maxhp":
            case "hp":

                char1.maxHealth += (int)((float)char1.baseMaxHealth * effect.statusPower / 100f);
                break;

            case "maxmagic":
            case "magic":
            case "maxmp":
            case "mp":

                char1.maxMagic += (int)((float)char1.baseMaxMagic * effect.statusPower / 100f);
                break;

            case "might":
            case "strength":

                char1.strength += (int)((float)char1.baseStrength * effect.statusPower / 100f);
                break;

            case "constitution":
            case "resistance":

                char1.resistance += (int)((float)char1.baseResistance * effect.statusPower / 100f);
                break;

            case "intelligence":
            case "magicpower":

                char1.intelligence += (int)((float)char1.baseIntelligence * effect.statusPower / 100f);
                break;

            case "spirit":
            case "magicresistance":

                char1.spirit += (int)((float)char1.baseSpirit * effect.statusPower / 100f);
                break;

            case "persistance":

                char1.persistance += (int)((float)char1.basePersistance * effect.statusPower / 100f);
                break;

            case "immunity":

                char1.immunity += (int)((float)char1.baseImmunity * effect.statusPower / 100f);
                break;

            case "luck":

                char1.luck += (int)((float)char1.baseLuck * effect.statusPower / 100f);
                break;

            case "speed":

                char1.speed += (int)((float)char1.baseSpeed * effect.statusPower / 100f);
                break;

            case "inspiration":
            case "morale":

                // To do: make this stat do something
                break;

            default:

                Debug.LogWarning("Unknown stat in 'ModifyStat' status delegate. Status removeID: " + effect.removeID + ", Misc text: " + effect.statusMiscText);
                break;
        }
    }

    public void DamageOverTime(CharacterScript char1, CharacterScript char2, StatusEffect effect)
    {
        float modifier = 1;
        for (int n = 0; n < char1.valnurabilityTypes.Count; n++)
        {
            if (char1.valnurabilityTypes[n] == effect.damageType && n < char1.valnurabilityPercentages.Count)
            {
                modifier += (char1.valnurabilityPercentages[n] / 100f) - 1;
            }
        }

        switch (effect.statusMiscText.ToLower().Replace(" ", ""))
        {
            case "health":
            case "poison":
            case "bleed":
            case "regen":
            case "regeneration":

                char1.ModifyHealth((int)(-effect.statusPower * modifier));
                break;

            case "magic":

                char1.ModifyMagic((int)(-effect.statusPower * modifier));
                break;

            case "percenthealth":
            case "healthpercent":
            case "regenpercent":

                char1.ModifyHealth((int)(char1.maxHealth * effect.statusPower / -100f * modifier));
                break;

            case "percentmagic":
            case "magicpercent":

                char1.ModifyMagic((int)(char1.maxHealth * effect.statusPower / -100f * modifier));
                break;

            case "exponentialhealth":
            case "healthexponential":

                char1.ModifyHealth((int)(-effect.statusPower * modifier));
                effect.statusPower *= 2;
                break;

            case "exponentialmagic":
            case "magicxponential":

                char1.ModifyMagic((int)(-effect.statusPower * modifier));
                effect.statusPower *= 2;
                break;
        }
    }

    public void RemoveStatus(CharacterScript char1, CharacterScript char2, StatusEffect effect)
    {
        List<StatusEffect> removeList = new List<StatusEffect>();

        foreach (StatusEffect efct in char1.statusEffects)
        {
            if (efct.removeID == effect.removeID || efct.removeID == effect.statusMiscText)
            {
                removeList.Add(efct);
            }
        }

        Debug.Log("Removing " + removeList.Count + " status effects with ID: " + effect.removeID);

        foreach (StatusEffect efct in removeList)
        {
            if (char1.statusEffects.Contains(efct))
            {
                char1.statusEffects.Remove(efct);
            }
        }
    }

    public void Revive(CharacterScript char1, CharacterScript char2, StatusEffect effect) // Negative revive is instant death
    {
        int hpFinal = (int)((float)char1.maxHealth * effect.statusPower / 100f);

        if ((char1.health <= 0 && effect.positive) || (!effect.positive))
        {
            char1.ModifyHealth(hpFinal);
            if (char1.gameObject.tag == "Player" && char1.aiState == CharacterScript.AiState.Stun)
            {
                char1.aiState = CharacterScript.AiState.Player;
            }
            else if (char1.gameObject.tag == "Enemy" && char1.aiState == CharacterScript.AiState.Stun)
            {
                char1.aiState = CharacterScript.AiState.Enemy;
            }
        }
        else
        {
            battleController.DisplayText("Can't revive a living target!");
        }

    }

    public void ChangeAi(CharacterScript char1, CharacterScript char2, StatusEffect effect)
    {
        if (char1.aiState != CharacterScript.AiState.Stun)
        {

            switch (effect.statusMiscText.ToLower().Replace(" ", ""))
            {
                case "seduce":
                case "opposite":

                    char1.aiState = char2.aiState;
                    break;

                case "confuse":

                    char1.aiState = CharacterScript.AiState.Confused;
                    break;

                case "berserk":
                case "rage":

                    char1.aiState = CharacterScript.AiState.Berserk;
                    break;

                case "caution":

                    char1.aiState = CharacterScript.AiState.Caution;
                    break;

                case "panic":

                    char1.aiState = CharacterScript.AiState.Panic;
                    break;

                case "stun":
                case "sleep":
                case "stop":
                case "none":
                case "death":

                    char1.aiState = CharacterScript.AiState.Stun;
                    break;
            }
        }
    }

    public void AddDelegate(CharacterScript char1, CharacterScript char2, StatusEffect effect)
    {
        switch (effect.statusMiscText.ToLower().Replace(" ", ""))
        {
            case "offend":

                char1.offendDelegateStatuses.Add(effect);
                break;

            case "defend":

                char1.defendDelegateStatuses.Add(effect);
                break;

            case "killed":

                // To do: make an 'OnKilledDelegate'
                break;
        }
    }

    public void AddDefender(CharacterScript char1, CharacterScript char2, StatusEffect effect)
    {
        switch (effect.statusMiscText.ToLower())
        {
            default:
            case "defend":
            case "cover":

                char1.defender = char2.gameObject;
                break;

            case "reflect":

                char1.reflect = true;
                break;
        }

    }
}

