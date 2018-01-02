using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechTreeInput : MonoBehaviour {

    public List<Trait> traits;
    public List<Trait> prerequisites;
    public bool repeatable;

    public void Unlock ()
    {
        PauseMenuScript pauseController = LoadInfo.Instance.pauseController;
        CharacterScript charScript = pauseController.selectedCharacter.GetComponent<CharacterScript>();
        Trait trait = null;

        for (int n = 0; n < traits.Count; n++)
        {
            if (!charScript.unlockedTraits.Contains(traits[n]))
            {
                trait = traits[n];
                Debug.Log("Chose trait!");
                break;
            }
            else
            {
                Debug.Log("Not choosing " + traits[n].name);
            }
        }

        if (trait == null)
        {
            Debug.LogError("Can't unlock");
            return;
        }

        if (charScript.unlockPoints >= trait.unlockPoints)
        {
            charScript.unlockedTraits.Add(trait);
            charScript.unlockPoints -= trait.unlockPoints;
            Debug.Log("Unlocking " + trait.name);

            if (trait.statNum != 0)
            {
                switch (trait.stat)
                {
                    case Trait.Stat.Health:

                        charScript.baseMaxHealth += trait.statNum;
                        break;

                    case Trait.Stat.Magic:

                        charScript.baseMaxMagic += trait.statNum;
                        break;

                    case Trait.Stat.Strength:

                        charScript.baseStrength += trait.statNum;
                        break;

                    case Trait.Stat.intelligence:

                        charScript.baseIntelligence += trait.statNum;
                        break;

                    case Trait.Stat.Resistance:

                        charScript.baseResistance += trait.statNum;
                        break;

                    case Trait.Stat.Spirit:

                        charScript.baseSpirit += trait.statNum;
                        break;

                    case Trait.Stat.Persistance:

                        charScript.basePersistance += trait.statNum;
                        break;

                    case Trait.Stat.Immunity:

                        charScript.baseImmunity += trait.statNum;
                        break;

                    case Trait.Stat.Speed:

                        charScript.baseSpeed += trait.statNum;
                        break;

                    case Trait.Stat.Luck:

                        charScript.baseImmunity += trait.statNum;
                        break;
                }
            }
        }
        else
        {
            Debug.LogError("Not enough points to unlock!");
        }
    }
}
