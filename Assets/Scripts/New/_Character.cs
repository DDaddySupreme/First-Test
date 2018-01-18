using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Character : MonoBehaviour
{
    public List<_Stat> stats; // I would prefer a dictionary, but tose don't display in te Unity editor
    public Sprite portrait;

    void Start()
    {

    }

    void Update()
    {

    }

    /// <summary>
    /// Modify a stat on this character
    /// </summary>
    /// <param name="name">Name of stat to be modified</param>
    /// <param name="amount">How much it will be modified</param>
    /// <param name="percent">Is 'amount' a percentage?</param>
    /// <param name="permanent">If true, effect maximum. This is used for things like leveling up</param>
    /// <param name="tag">What caused the modification. Examples: spell, melee, enviroment, reaction, self, status</param>
    public void ModifyStat (string name, int amount, bool percent = false, bool permanent = false, string tag = "") 
    {
        // To do: add delegates to detect when a certain stat is modified, and by how much. Also, delegate should be able to modify how much it's modified
        // To do: find way to increase max hp in combat, without it being permanent
        _Stat stat = null;
        foreach (_Stat temp in stats)
        {
            if (stat.name.ToLower().Replace(" ", "") == name.ToLower().Replace(" ", ""))
            {
                stat = temp;
            }
        }

        if (stat == null)
        {
            stat = new _Stat(name);
            stats.Add(stat);
        }

        if (percent)
        {
            stat.current = (int)(stat.current * (amount / 100f));
        }
        else
        {
            stat.current += amount;
        }
    }

    public float FindStat(string name, bool max = false)
    {
        foreach (_Stat stat in stats)
        {
            if (stat.name.ToLower().Replace(" ", "") == name.ToLower().Replace(" ", ""))
            {
                return !max ? stat.current: stat.max;
            }
        }
        return _Stat.defaultNum;
    }
}

[System.Serializable]
public class _Stat
{
    public static int defaultNum = 100; // If there's a 'movement' stat, this isn't gonna work very well. That stat would probably have a different default than others
    public string name;
    public float current;
    public float max;
    public bool wholeNumOnly;

    public _Stat(string name)
    {
        this.name = name;
        current = defaultNum;
        max = defaultNum;
    }

    public _Stat (string name, float current, float max)
    {
        this.name = name;
        this.current = current;
        this.max = max;
    }
}