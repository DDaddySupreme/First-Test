using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _MapInfo : MonoBehaviour
{
    public Dictionary<Vector3, _Node> map;
    public LayerMask nodeOnly;

    void Start()
    {
        // Debugging. Create constructor for _Node

        /*for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3 loc = new Vector3(node.position.x + x, node.position.y + y);
                if (!map.ContainsKey(loc))
                {
                    map.Add();
                }
            }
        }*/
    }

    void Update()
    {

    }

    public List<_Node> Nearby (_Node node)
    {
        List<_Node> list = new List<_Node>();

        // Decide what to do with diagonals
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3 loc = new Vector3(node.position.x + x, node.position.y + y);
                if (map.ContainsKey(loc))
                {
                    list.Add(map[loc]);
                }
            }
        }

        return list;
    }

    public List<_Node> GetShape(Shape shape, Vector3 pos)
    {
        pos += shape.offset; // I'm not sure if pos is a private variable, or a refernce to another variable. This might not work
        List<_Node> result = new List<_Node>();

        switch (shape.basicShape)
        {
            case Shape.BasicShape.Circle:

                for (int n = -shape.distance; n <= shape.distance; n++)
                {
                    for (int p = -shape.distance; p <= shape.distance; p++)
                    {
                        Vector3 temp = new Vector3(n, p) + pos;

                        if (map.ContainsKey(temp) && Vector3.Distance(pos, temp) <= shape.distance)
                        {
                            result.Add(map[temp]);
                        }
                    }
                }
                break;

            case Shape.BasicShape.Cone:
                

                break;

            case Shape.BasicShape.LineDirection:

                Vector3 startingPoint = new Vector3(shape.distance / 2f, 0) - pos;
                startingPoint = Quaternion.Euler(0, 0, float.Parse(shape.args[0])) * startingPoint;
                startingPoint += pos;

                foreach (RaycastHit2D hit in Physics2D.RaycastAll(startingPoint, (Quaternion.Euler(0, 0, float.Parse(shape.args[0])) * Vector3.right), shape.distance, nodeOnly))
                {
                    if (hit.transform.gameObject.GetComponent<_Node>() != null)
                    {
                        result.Add(hit.transform.gameObject.GetComponent<_Node>());
                    }
                }
                break;

            case Shape.BasicShape.LineFromTo:

                Vector3 otherPos = new Vector3(int.Parse(shape.args[0]), int.Parse(shape.args[1])); // Kinda wanna change this to use 1 arg. Also maybe gotta decide which arg to use?

                foreach (RaycastHit2D hit in Physics2D.RaycastAll(pos, otherPos - pos, Vector3.Distance(pos, otherPos), nodeOnly))
                {
                    if (hit.transform.gameObject.GetComponent<_Node>() != null)
                    {
                        result.Add(hit.transform.gameObject.GetComponent<_Node>());
                    }
                }
                break;

            case Shape.BasicShape.Square:

                for (int n = -shape.distance; n <= shape.distance; n++)
                {
                    for (int p = -shape.distance; p <= shape.distance; p++)
                    {
                        Vector3 temp = new Vector3(n, p) + pos;
                        
                        if (map.ContainsKey(temp))
                        {
                            result.Add(map[temp]);
                        }
                    }
                }
                break;
        }

        return result;
    }

    public List<_Node> GetPath(Vector3 currentPos, Vector3 targetPos)
    {
        //lookingForPath = true;
        List<_Node> path = new List<_Node>();
        List<_Node> untested = new List<_Node>();
        List<_Node> tested = new List<_Node>();
        _Node start = null;
        _Node current;
        Dictionary<_Node, float> gScore = new Dictionary<_Node, float>(); // Distance from start
        Dictionary<_Node, float> hScore = new Dictionary<_Node, float>(); // Estemated distance from end
        Dictionary<_Node, float> fScore = new Dictionary<_Node, float>(); // g + h
        Dictionary<_Node, _Node> previous = new Dictionary<_Node, _Node>();

        Debug.DrawLine(new Vector3(targetPos.x - 0.5f, targetPos.y - 0.5f), new Vector3(targetPos.x + 0.5f, targetPos.y + 0.5f), Color.blue, 1f);
        Debug.DrawLine(new Vector3(targetPos.x + 0.5f, targetPos.y - 0.5f), new Vector3(targetPos.x - 0.5f, targetPos.y + 0.5f), Color.blue, 1f);

        foreach (KeyValuePair<Vector3, _Node> pair in map)
        {
            _Node node = pair.Value;
            if (Mathf.RoundToInt(node.position.x) == Mathf.RoundToInt(currentPos.x) && Mathf.RoundToInt(node.position.y) == Mathf.RoundToInt(currentPos.y))
            {
                start = node;
                gScore[start] = 0;
                start.position = currentPos;
                previous[start] = null;
                tested.Add(start);
            }
            else
            {
                gScore.Add(node, Mathf.Infinity);
            }
            hScore.Add(node, Vector3.Distance(node.position, targetPos));
            fScore.Add(node, Mathf.Infinity);

            untested.Add(node);
        }

        //start = graph[distance + 1, distance + 1];

        //start.f = start.g + start.h;
        while (untested.Count > 0)
        {
            current = null; // Find lowest f value 
            foreach (_Node node in untested)
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
                path.Add(current);

                if (previous.ContainsKey(current))
                {
                    while (previous[current] != null)
                    {
                        path.Add(previous[current]);
                        Debug.DrawLine(previous[current].position, current.position, Color.red, 2f);
                        current = previous[current];
                    }
                }
                path.Reverse();
                Debug.Log("Found path! ");
                return path;
                //currentPath = path;
                //lookingForPath = false;
                //yield break;
            }
            untested.Remove(current);
            tested.Add(current);

            foreach (_Node node in Nearby(current))
            {
                if (tested.Contains(node) || node.travelRate != 0)
                {
                    continue;
                }

                float tempG = gScore[current] + (Vector3.Distance(current.position, node.position) / node.travelRate);
                if (tempG < gScore[node])
                {
                    gScore[node] = tempG;
                    previous[node] = current;
                }

            }

            //yield return new WaitForSeconds(0f);
        }

        //lookingForPath = false;
        Debug.LogError("Pathfinding failed! Tried to get from " + transform.position + " to " + targetPos + " (with rounding)");
        path = new List<_Node>();
        return null;
        //return new List<_Node> { start };
    }
}