using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Character : MonoBehaviour
{
    public _Stat[] stats;
    public Sprite portrait;

    void Start()
    {

    }

    void Update()
    {

    }
}

[System.Serializable]
public class _Stat
{
    public string name;
    public int current;
    public int max;
}