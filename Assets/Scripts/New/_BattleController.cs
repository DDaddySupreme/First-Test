using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _BattleController : MonoBehaviour
{
    public List<GameObject> charList = new List<GameObject>();
    public Dictionary<GameObject, _Character> scriptDict;
    public LayerMask clickMask;
    public _MapInfo map;

    public GameObject turn;
    // public List<GameObject> turnOrder;
    /// Turn could be handld a few ways. I could do it like with the last battle controller where it uses speed to determine who shoudld go next, but there would be no complete rounds
    /// We could make a list of turns for the round, and it restarts iniative after that. There could be a 'actions' stat, or it could be depending on your speed, like every 100 is another turn
    /// Personally, I thik I like the previous system I had. Though there can be some problems with it. A resistance stat, for example, would go away faster if your speed is higher
    /// whereas if that status effect goes down every round, not turn, it wouldn't matter what your speed is. But also, a small difference in speed would result in a small difference in turns
    /// instead of no difference, like what would happen witht eh rounds system. Another option is active time battle, life Final Fanasy... But I don't know if I like that.


    private float TotalTime // Used to decide turns
    {
        get
        {
            float temp = 0;
            foreach (GameObject obj in charList)
            {
                temp += scriptDict[obj].FindStat("speed");
            }
            return temp;
        }
    }

    void Start()
    {

    }

    void Update()
    {
        //Debug.Log("Shoop");
        if (Input.GetButtonDown("Interact"))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.Log("Boop");
            if (Physics.Raycast(ray, out hit, clickMask))
            {
                Transform objectHit = hit.transform;
                Debug.Log(Vector3Int.RoundToInt(hit.point));
                if (map != null)
                {
                    map.GetPath(turn.transform.position, Vector3Int.RoundToInt(hit.point));
                }
            }
        }
    }

    public void EndTurn()
    {

    }
}
