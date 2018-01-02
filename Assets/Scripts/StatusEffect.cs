using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatusEffect
{
    [System.Serializable, System.Obsolete]
    public enum StatusType 
    {
        HealthOverTime,

        MagicOverTime,

        ChangeMaxHealth,

        ChangeMaxMagic,

        ChangeStrength,

        ChangeMagicPower,

        ChangeResistance,

        ChangeMagicResistance,

        ChangeLuck,

        ChangeSpeed,

        ChangeAiState,
            /// ChangeAiState is state based, not power based.
            /// 1: Swap player and enemy controll
            /// 2: Confusion
            /// 3: Berserk
            /// 4: Caution (Not sure if I'll need this)
            /// 5: Panic (Not sure if I'll let the enemies use this. Could accidentally make the whole party leave)
            /// Anything else is ignored
            /// Adding any new AiState will remove all others (?)

        AddOffenseDelegate,

        AddDefenseDelegate,

        Revive
            /// Duration acts like a delay. Reviving/instant death only happens once per status.
            /// Removing revive (only if power is positive) will clear all status effects
    }

    public string name;             // Just used for displaying. If blank, will not be displayed, because it's probably meant to go with another effect which does have a display name.
    public string removeID;         // If remove effect ID is equal to another effects ID, it will remove that effect. Also used for adding delegates

    [Space]
    public Attack.DamageType damageType;
    //public StatusType statusType;
    public CharacterScript caster;
    public string statusDelegateName;
    public string statusMiscText;       // For specifying things like which stat to change
    public int statusPower;             // statusPower changes whether status is buff or debuff. negative d.healthOverTime is poison, 0 changeMagicPower is silence, 0.5 changeSpeed is slowness, etc.
    public int statusDuration;
    public int chance;                  // Sometimes an attack will hit, but the status won't
    public int delay;
    [System.Obsolete]
    public bool remove = false;     
    public bool positive;               // Helps AI
    public bool persistant;             // Does the effect stay after the battle ends?

    public StatusEffect (StatusEffect original, CharacterScript causedBy)
    {
        name = original.name;
        removeID = original.removeID;

        caster = causedBy;
        statusDelegateName = original.statusDelegateName;
        statusMiscText = original.statusMiscText;

        damageType = original.damageType;
        //statusType = original.statusType;

        statusPower = original.statusPower;
        statusDuration = original.statusDuration;
        chance = original.chance;
        delay = original.delay;

        //remove = original.remove;
        positive = original.positive;
        persistant = original.persistant;
    }

    [System.Obsolete]
    public StatusEffect (StatusType type, int dur, int pwr)
    {
        //statusType = type;
        statusDuration = dur;
        statusPower = pwr;
    }
}
    
