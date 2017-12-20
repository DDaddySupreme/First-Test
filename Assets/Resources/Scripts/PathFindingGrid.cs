using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class PathFindingGrid : MonoBehaviour {

    public Tilemap tileMap;
    public LayerMask wallsOnly;
    //[System.Obsolete]
    public static Node[,] graph;
    //public Dictionary<Vector3, Node> graphNew = new Dictionary<Vector3, Node>();
    public int distance;

    public class Node
    {
        public List<Node> nearby;
        //public Node previous;
        public Vector3 position;
        //public PathFindState pathFindState = PathFindState.Untested;
        //public float g = Mathf.Infinity; // Distance from start
        //public float h = Mathf.Infinity; // Distance from end (estemate)
        //public float f; // G + H
        public bool walkable = true;

        public Node()
        {
            nearby = new List<Node>();
        }
        public Node(Vector3 pos)
        {
            position = pos;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += CreateGrid;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= CreateGrid;
    }

    private void Awake()
    {
        CreateGrid(SceneManager.GetActiveScene());
    }

    private void CreateGrid(Scene scene, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (mode == LoadSceneMode.Additive)
        {
            return;
        }
        Debug.Log("Making grid for " + scene);

        graph = new Node[(distance * 2) + 1, (distance * 2) + 1];
        for (int x = 0; x < (distance * 2) + 1; x++)
        {
            for (int y = 0; y < (distance * 2) + 1; y++)
            {
                graph[x, y] = new Node();
                graph[x, y].position.x = (x - distance);
                graph[x, y].position.y = (y - distance);
                //graph[x, y].h = Vector2.Distance(graph[x, y].position, targetPos);
                //untested.Add(graph[x, y]);
                if (Physics2D.Linecast(graph[x, y].position, graph[x, y].position, wallsOnly))
                {
                    graph[x, y].walkable = false;
                }
                else
                {

                }
            }
        }

        for (int x = 0; x < (distance * 2) + 1; x++)
        {
            for (int y = 0; y < (distance * 2) + 1; y++)
            {
                //graph[x, y].nearby.Add()
                if (x > 0)
                {
                    graph[x, y].nearby.Add(graph[x - 1, y]);
                }
                if (x < (distance * 2) - 1)
                {
                    graph[x, y].nearby.Add(graph[x + 1, y]);
                }

                if (y > 0)
                {
                    graph[x, y].nearby.Add(graph[x, y - 1]);
                }
                if (y < (distance * 2) - 1)
                {
                    graph[x, y].nearby.Add(graph[x, y + 1]);
                }

                if (x > 0 && y > 0)
                {
                    graph[x, y].nearby.Add(graph[x - 1, y - 1]);
                }
                if (x < (distance * 2) - 1 && y < (distance * 2) - 1)
                {
                    graph[x, y].nearby.Add(graph[x + 1, y + 1]);
                }

                if (y > 0 && x < (distance * 2) - 1)
                {
                    graph[x, y].nearby.Add(graph[x + 1, y - 1]);
                }
                if (x > 0 && y < (distance * 2) - 1)
                {
                    graph[x, y].nearby.Add(graph[x - 1, y + 1]);
                }
            }
        }
    }
}
