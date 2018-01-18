using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New_CardData", menuName = "_New/Card", order = 1)]
public class _Card : ScriptableObject
{
    public _Attack[] attacks;

    public int hitChance;
    public int critChance;
    public int magic;
    //public float range;
    //public float damageMod;
    public string[] args; // Some attacks might effect the other attacks in the list
    public string[] argNames;
}

//[CreateAssetMenu(fileName = "New_AttackData", menuName = "_New/Attack", order = 1)]
[System.Serializable]
public class _Attack
{
    private _Card card;
    public string name; // Just for the inspector
    public int damage; // Make negative for restoration
    public string damageType; // To restore/remove mp, use "magic"

    [Space]
    public Shape[] shapes; // No shapes = self or previous attacks shape
    public float range;
    public string attackDelegate;
    public string[] atkDelegateArgs;

    [Space]
    public _Status[] status;

}

[System.Serializable] // Another script would probably handle actually creating the list of tiles. Or I could make a delegate?
public class Shape
{
    public BasicShape basicShape;
    public int distance;
    public float falloff;
    public Vector3 offset;
    public string[] args;

    public enum BasicShape
    {
        Square,
        Circle,
        Diamond,
        Ring,
        LineDirection,
        LineFromTo,
        Cone,
    }
}

//[CreateAssetMenu(fileName = "New_StatusData", menuName = "_New/Status", order = 1)]
[System.Serializable]
public class _Status
{
    public bool terrain; // True to stay on tile. False to stay on player
    public int duration;
    public float power;

    public string statusDelegate;
    public string[] args;
}