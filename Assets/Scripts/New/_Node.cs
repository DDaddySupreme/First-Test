using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Node //: MonoBehaviour
{
    public Vector3 position;
    public List<_Status> statusEffects;
    public float travelRate; // 1 is normal, 0.5 requires double movement, 0 is impassable, 2 requires half movement

    public _Node (Vector3 pos, float rate)
    {
        position = pos;
        statusEffects = new List<_Status>();
        travelRate = rate;
    }
}
