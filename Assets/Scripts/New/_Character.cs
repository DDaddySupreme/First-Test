using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Character : MonoBehaviour
{
    public _Stat[] stats;

    void Start()
    {

    }

    void Update()
    {

    }
}

public class _Stat
{
    public string name;
    public int current;
    public int max;
}