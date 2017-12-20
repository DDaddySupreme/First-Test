using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackData", menuName = "RPG/Attack", order = 1)]
public class Attack : ScriptableObject
{
    public enum DamageType
    {
        Slash,
        Blunt,
        Fire,
        Ice,
        Shock,
        Water,
        Earth,
        Wind,
        Bio,
        Chem, // Might change to Rad
        Demonic,
        Angelic,
        Generic,
        Leave, // Leaves battle without killing target. Mostly for running from battle, but this way opens up oppurtunity for other spells that bahish an enemy. Damage type mostly for AI and resistances
    }
    public enum TargetType
    {
        Normal,
        Self,
        Group,
        Row,
        All,
        Area
    }

    [Multiline]
    public string description;

    public int damageModifier = 100;
    public int hitChance;
    public int critChance;
    public int magicRequired;
    public bool positive; // Just for the AI. To do: make AI know the difference between healing and buffs (?)
    public bool magical;
    public bool ranged;
    //public bool TEST; // Not a used value. Just for making Unity compile properly

    [Space]
    public DamageType damageType;
    public TargetType targetType = TargetType.Normal;
    public Sprite sprite;

    public List<StatusEffect> statusEffects;

    [Space]
    public int equipPoints;
    public int level;

    [Space]
    public List<SubAttack> delegates;

    public Attack(DamageType dmgType, int dmg, int chance, int crit, int mp, TargetType TrgtType, bool self)
    {
        damageType = dmgType;
        damageModifier = dmg;
        hitChance = chance;
        critChance = crit;
        magicRequired = mp;
        targetType = TrgtType;
    }

    public Attack(DamageType dmgType, int dmg, int chance, int crit, int mp, TargetType TrgtType, bool self, List<StatusEffect> efcts)
    {
        damageType = dmgType;
        damageModifier = dmg;
        hitChance = chance;
        critChance = crit;
        magicRequired = mp;
        targetType = TrgtType;

        statusEffects = efcts;
    }
}

[System.Serializable]
public class SubAttack
{
    public List<string> conditions; // To do: impliment conditions for things like map type
    public string delegateName;
    public string miscText;
    public int miscNum;

    public SubAttack ()
    {
        delegateName = "StandardAttack";
        miscText = "";
        miscNum = 100;
    }

    public SubAttack (string name, string text, int num, List<string> cond)
    {
        conditions = cond;
        delegateName = name;
        miscText = text;
        miscNum = num;
    }

    public SubAttack(string name, string text, int num)
    {
        conditions = new List<string>();
        delegateName = name;
        miscText = text;
        miscNum = num;
    }
}