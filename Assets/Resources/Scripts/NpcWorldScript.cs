using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class NpcWorldScript : MonoBehaviour
{
    [Header("Dialogue:")]
    public Dialogue dialogue;

    [Header("Shop:")]
    public int pricePercent;
    public List<Item> shopInventory;

    [Space, Header("Enemies:")]
    public List<GameObject> enemies;

    [Space, Header("AI:")]
    public int fovAngle = 180;
    public int detectionRange = 16;
    public float movementSpeed = 2;
    public float waitTime = 2f;
    public LayerMask opaqueLayers;
    public Vector3 goalPosition;
    private Rigidbody2D rigidBody;
    private GameObject target; // If/when I add multiplayer, this will become an array or list
    private int currentPatrollPoint = 0;
    private float timer = 0;
    private bool lookingForPath = false;
    RaycastHit2D hit;
    List<PathFindingGrid.Node> currentPath = new List<PathFindingGrid.Node>();
    //PathFindingGrid.Node currentNode;
    //RaycastHit wallDetector;
    //Tilemap map;

    public List<Vector3> patrollPoints;

    [Space]
    public PassiveAction passiveAction = PassiveAction.Patroll;
    public ActiveAction activeAction = ActiveAction.Forget;
    [SerializeField] CurrentAction currentAction = CurrentAction.Passive;

    public enum PassiveAction
    {
        Stay,
        Patroll,
        Wander
    }

    public enum ActiveAction
    {
        Ignore,
        Forget,
        Search,
        RunAway
        //SeekFriends?
    }

    public enum CurrentAction
    {
        Aggro,
        LoseSight,
        Passive
    }


    public enum PathFindState
    {
        Untested,
        Unavailable,
        Tested
    }

    public IEnumerator GetPath(Vector3 targetPos)
    {
        lookingForPath = true;
        List<PathFindingGrid.Node> tempPath = new List<PathFindingGrid.Node>();
        List<PathFindingGrid.Node> untested = new List<PathFindingGrid.Node>();
        List<PathFindingGrid.Node> tested = new List<PathFindingGrid.Node>();
        PathFindingGrid.Node start = null;
        PathFindingGrid.Node current;
        Dictionary<PathFindingGrid.Node, float> gScore = new Dictionary<PathFindingGrid.Node, float>(); // Distance from start
        Dictionary<PathFindingGrid.Node, float> hScore = new Dictionary<PathFindingGrid.Node, float>(); // Estemated distance from end
        Dictionary<PathFindingGrid.Node, float> fScore = new Dictionary<PathFindingGrid.Node, float>(); // g + h
        Dictionary<PathFindingGrid.Node, PathFindingGrid.Node> previous = new Dictionary<PathFindingGrid.Node, PathFindingGrid.Node>();

        Debug.DrawLine(new Vector3(targetPos.x - 0.5f, targetPos.y - 0.5f), new Vector3(targetPos.x + 0.5f, targetPos.y + 0.5f), Color.blue, 1f);
        Debug.DrawLine(new Vector3(targetPos.x + 0.5f, targetPos.y - 0.5f), new Vector3(targetPos.x - 0.5f, targetPos.y + 0.5f), Color.blue, 1f);

        foreach (PathFindingGrid.Node node in PathFindingGrid.graph)
        {
            if (Mathf.RoundToInt(node.position.x) == Mathf.RoundToInt(transform.position.x) && Mathf.RoundToInt(node.position.y) == Mathf.RoundToInt(transform.position.y))
            {
                start = node;
                gScore[start] = 0;
                start.position = transform.position;
                previous[start] = null;
                tested.Add(start);
            }
            else
            {
                gScore.Add(node, Mathf.Infinity);
            }
            hScore.Add(node, Vector2.Distance(node.position, targetPos));
            fScore.Add(node, Mathf.Infinity);

            untested.Add(node);
        }

        //start = PathFindingGrid.graph[distance + 1, distance + 1];

        //start.f = start.g + start.h;
        while (untested.Count > 0)
        {
            current = null; // Find lowest f value 
            foreach (PathFindingGrid.Node node in untested)
            {
                fScore[node] = gScore[node] + hScore[node];
                if (current == null || fScore[node] < fScore[current])
                {
                    current = node;
                }
            }

            Debug.DrawLine(new Vector3(current.position.x - 0.5f, current.position.y - 0.5f), new Vector3(current.position.x + 0.5f, current.position.y + 0.5f), Color.magenta, 0f);
            Debug.DrawLine(new Vector3(current.position.x + 0.5f, current.position.y - 0.5f), new Vector3(current.position.x - 0.5f, current.position.y + 0.5f), Color.magenta, 0f);

            //Debug.Log("Testing: " + current.position);
            if (Mathf.Round(current.position.x) == Mathf.Round(targetPos.x) && Mathf.Round(current.position.y) == Mathf.Round(targetPos.y))
            {
                tempPath.Add(current);

                if (previous.ContainsKey(current))
                {
                    while (previous[current] != null)
                    {
                        tempPath.Add(previous[current]);
                        Debug.DrawLine(previous[current].position, current.position, Color.red, 2f);
                        current = previous[current];
                    }
                }
                tempPath.Reverse();
                //Debug.Log("Found path! ");
                currentPath = tempPath;
                lookingForPath = false;
                yield break;
            }
            untested.Remove(current);
            tested.Add(current);

            foreach (PathFindingGrid.Node node in current.nearby)
            {
                if (tested.Contains(node) || node.walkable == false)
                {
                    continue;
                }

                float tempG = gScore[current] + Vector3.Distance(current.position, node.position);
                if (tempG < gScore[node])
                {
                    gScore[node] = tempG;
                    previous[node] = current;
                }

            }

            yield return new WaitForSeconds(0f);
        }

        lookingForPath = false;
        Debug.LogError("Pathfinding failed! Tried to get from " + transform.position + " to " + targetPos + " (with rounding)");
        currentPath = new List<PathFindingGrid.Node>();
        //return new List<PathFindingGrid.Node> { start };
    }

    public void GoTowardsNode()
    {
        //timer = 0;
        float tempSpeed;
        if (currentPath.Count > 0)
        {
            if (currentAction == CurrentAction.Passive)
            {
                tempSpeed = movementSpeed / 2;
            }
            else
            {
                tempSpeed = movementSpeed;
            }

            int count = 0;
            while (count < currentPath.Count - 1 && Vector2.Distance(transform.position, currentPath[1].position) < Vector2.Distance(currentPath[0].position, currentPath[1].position))
            {
                currentPath.Remove(currentPath[0]);
            }

            //currentNode = currentPath[0];

            if (Vector3.Distance(transform.position, currentPath[0].position) > 0.1f )//&& (Mathf.Abs(rigidBody.velocity.x) > (movementSpeed / 8f) || Mathf.Abs(rigidBody.velocity.y) > (movementSpeed / 8f) || timer < waitTime))
            {
                transform.position = Vector2.MoveTowards(transform.position, currentPath[0].position, (tempSpeed * Time.deltaTime));
                //rigidBody.velocity = new Vector3((currentNode.position.x - transform.position.x) / Vector3.Distance(transform.position, currentNode.position), // Trigonometry
                            //(currentNode.position.y - transform.position.y) / Vector3.Distance(transform.position, currentNode.position)) * tempSpeed;
            }
            else
            {
                rigidBody.velocity = new Vector3(0, 0);
                timer = 0;
                currentPath.Remove(currentPath[0]);
            }
        }

    }

    void Start()
    {
        if (passiveAction == PassiveAction.Patroll && patrollPoints.Count < 2)
        {
            passiveAction = PassiveAction.Stay;
        }
        else if (passiveAction == PassiveAction.Patroll && !lookingForPath)
        {
            StartCoroutine(GetPath(patrollPoints[currentPatrollPoint]));
            //currentPath = GetPath(patrollPoints[currentPatrollPoint]);
        }

        target = GameObject.FindGameObjectWithTag("WorldPlayer");
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
        //StartCoroutine(GetPath(target.transform.position));
        ////currentPath = GetPath(target.transform.position);
        //goTowardsNode();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (LoadInfo.Instance.pause == false)
        {
            hit = Physics2D.Linecast(transform.position, target.transform.position, opaqueLayers);

            if (hit.collider.gameObject.tag == "WorldPlayer" && activeAction != ActiveAction.Ignore)
            {
                currentAction = CurrentAction.Aggro;
            }
            else if (currentAction == CurrentAction.Aggro && activeAction != ActiveAction.Ignore)
            {
                currentAction = CurrentAction.LoseSight;
            }

            switch (currentAction)
            {
                case CurrentAction.Passive:

                    switch (passiveAction)
                    {
                        case PassiveAction.Patroll: // To do: double check to see if there's any bugs
                            if ((Vector3.Distance(transform.position, patrollPoints[currentPatrollPoint]) < 0.25f ||
                                Vector3.Distance(currentPath.Last().position, patrollPoints[currentPatrollPoint]) > 0.25f) && timer > waitTime && !lookingForPath)
                            {
                                timer = 0;
                                currentPatrollPoint += 1;
                                if (currentPatrollPoint >= patrollPoints.Count)
                                {
                                    currentPatrollPoint = 0;
                                }
                                StartCoroutine(GetPath(patrollPoints[currentPatrollPoint]));
                                //currentPath = GetPath(patrollPoints[currentPatrollPoint]);
                            }

                            GoTowardsNode();
                            break;

                        case PassiveAction.Wander:

                            if ((Vector3.Distance(transform.position, goalPosition) < 0.1 || currentPath.Count == 0) && timer > waitTime && !lookingForPath)
                            {
                                patrollPoints.Clear();
                                foreach (PathFindingGrid.Node node in PathFindingGrid.graph)
                                {
                                    if (Vector3.Distance(transform.position, node.position) <= 3 && node.walkable) //&& Random.Range(0, 100) < 10 )
                                    {
                                        patrollPoints.Add(node.position);
                                    }
                                }
                                //Debug.Log("PatrollPoints: " + patrollPoints.Count);
                                goalPosition = patrollPoints[Random.Range(0, patrollPoints.Count - 1)];
                                StartCoroutine(GetPath(goalPosition));
                                //currentPath = GetPath(goalPosition);
                            }
                            else
                            {
                                GoTowardsNode();
                            }
                            break;

                        case PassiveAction.Stay:
                            rigidBody.velocity = new Vector3(0, 0);
                            break;
                    }

                    break;

                case CurrentAction.Aggro:

                    switch (activeAction)
                    {
                        case ActiveAction.Forget:
                        case ActiveAction.Search:

                            if ((currentPath.Count == 0 || Vector3.Distance(currentPath.Last().position, target.transform.position) > 1))
                            {
                                if (!lookingForPath)
                                {
                                    goalPosition = hit.point;
                                    StartCoroutine(GetPath(target.transform.position));
                                }
                                else if (Vector3.Distance(goalPosition, target.transform.position) > 1)
                                {
                                    StopAllCoroutines();
                                    lookingForPath = false;

                                    goalPosition = hit.point;
                                    StartCoroutine(GetPath(target.transform.position));
                                }
                            }

                            GoTowardsNode();
                            break;

                        case ActiveAction.Ignore:


                            break;
                    }

                    break;

                case CurrentAction.LoseSight:

                    switch (activeAction)
                    {
                        case ActiveAction.Forget:

                            rigidBody.velocity = new Vector3(0, 0);
                            currentAction = CurrentAction.Passive;
                            break;

                        case ActiveAction.Search:
                            if (Vector3.Distance(transform.position, goalPosition) <= 0.1 || currentPath.Count == 0)
                            {
                                rigidBody.velocity = new Vector3(0, 0);
                                currentAction = CurrentAction.Passive;
                            }
                            else
                            {
                                GoTowardsNode();
                            }
                            break;

                        case ActiveAction.Ignore:

                            // Look really angry
                            break;
                    }

                    break;
            }
        }
        else
        {
            rigidBody.velocity = new Vector3(0, 0);
        }
    }
}