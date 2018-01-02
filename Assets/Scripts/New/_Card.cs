using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Card : ScriptableObject
{
    public _Attack[] attacks;

    public int hitChance;
    public int critChance;
    public int magic;
    public float range;

}

public class _Attack : ScriptableObject
{
    public int damage; // Make negative for healing
    public string damageType; // To restore/remove mp, use "magic"
    public string attackDelegate;

    public _Status[] status;
}

public class _Status : ScriptableObject
{
    public int duration;
    public float power;
    public string statusDelegate;
}