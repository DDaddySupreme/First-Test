using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Item : ScriptableObject
{
    [Multiline]
    public string description;
    public int cost;

    void Start()
    {

    }

    void Update()
    {

    }
}

public class _Consumable : _Item
{
    

}

public class _Equipable : _Item
{


}

public class _Scroll : _Item
{


}
