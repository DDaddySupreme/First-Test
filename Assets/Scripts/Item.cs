using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultItem", menuName = "RPG/Item", order = 1)]
public class Item : ScriptableObject
{
    public enum ItemType
    {
        Key,
        Consumable,
        Scroll,
        Weapon,
        TwoHandedWeapon,
        Armour,
        Accessory
    }

    //public string name;
    [Multiline]
    public string Description;
    [Space]
    [Tooltip("Consumables: health restored or damage dealt. \nWeapons: phys damage. \nArmour: max health modifier.")]
    public int Health = 0;

    [Tooltip("Consumables: magic restored or removed. \nWeapons: mag damage. \nArmour: max magic modifier.")]
    public int Magic = 0;

    [Tooltip("Weapons/armour: Resistance stat modifier")]
    public int Resistance = 0;

    [Tooltip("Weapons/armour: Magic resistance stat modifier")]
    public int MagicResistance = 0;

    [Tooltip("Consumables: chance to hit. \nWeapons/armour: speed skill modifier; the percentage of a characters speed that's added. " +
        "This will be negative on most weapons, and won't change too much with levels")]
    public int Speed = 0; // Changes chance to hit on consumables

    [Space]
    public int Value = 0;
    public int Level = 0; // To do : give items levels
                          // Items can't crit. That's one downside to usieng them
    [Space]
    [Tooltip("Are health and magic percentages or static values?")]
    public bool PercentBased = false; // Consumable only

    public ItemType Type = ItemType.Consumable;

    public Attack.DamageType DamageType = Attack.DamageType.Blunt;
    public Attack.TargetType targetType = Attack.TargetType.Normal;
    public Sprite Icon = null;

    [Space]
    public List<StatusEffect> statusEffects = null;
    [Tooltip("Consumables: uses first attack in list. \nWeapons: can use any atacks in list while using said weapon. \nArmour: (currently does nothing)")]
    public List<Attack> attacksList = new List<Attack>();
}
