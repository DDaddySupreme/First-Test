using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TraitData", menuName = "RPG/Trait", order = 1)]
//[System.Serializable]
public class Trait : ScriptableObject // Also known as Unlock
{
    public enum Stat
    {
        Health,
        Magic,
        Strength,
        intelligence,
        Resistance,
        Spirit,
        Persistance,
        Immunity,
        Speed,
        Luck,
        Inspiration
    }

    //public string name;
    public string description;

    public List<Attack> attacks;
    public List<StatusEffect> effects;

    public Stat stat;
    public int statNum;
    public Sprite icon;

    [Space]
    public int level;
    public int unlockPoints;
    public int equipPoints;
}
