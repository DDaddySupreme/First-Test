using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class SpawnEnemies : MonoBehaviour {

    public Tilemap tileMap = null;
    public int maxTotalEnemies;
    public int distanceFromPlayer;
    public List<GameObject> enemies;
    public List<int> enemyChance;

    //[HideInInspector]
    public List<Vector3> availablePlaces;
    public List<GameObject> activeEnemies;
    public GameObject player;

	void Start () {
        tileMap = transform.GetComponentInParent<Tilemap>();
        availablePlaces = new List<Vector3>();
        activeEnemies = new List<GameObject>();

        for (int n = tileMap.cellBounds.xMin; n < tileMap.cellBounds.xMax; n++)
        {
            for (int p = tileMap.cellBounds.yMin; p < tileMap.cellBounds.yMax; p++)
            {
                Vector3Int localPlace = (new Vector3Int(n, p, (int)tileMap.transform.position.y));
                Vector3 place = tileMap.CellToWorld(localPlace);
                if (tileMap.HasTile(localPlace))
                {
                    //Node node = new Node();
                    Debug.Log("Tile at: " + place);
                    availablePlaces.Add(place);
                }
                else
                {
                    //Debug.Log("No tile at: " + place);
                }
            }
        }
    }
	
	void Update () {

        if (activeEnemies.Count < maxTotalEnemies && !LoadInfo.Instance.pause)
        {
            CheckAndSpawn();
        }
	}

    void CheckAndSpawn ()
    {
        Vector3 selectedPos = availablePlaces[Random.Range(0, availablePlaces.Count - 1)];
        List<GameObject> tempList = new List<GameObject>();
        int totalRarity = 0;
        int selectedChance = 0;

        if (activeEnemies.Count >= maxTotalEnemies || LoadInfo.Instance.pause || Vector2.Distance(selectedPos, player.transform.position) < distanceFromPlayer)
        {
            return;
        }

        for (int n = 0; n < enemies.Count; n++)
        {
            if (n < enemyChance.Count)
            {
                totalRarity += enemyChance[n];
                for (int p = 0; p <= enemyChance[n]; p++)
                {
                    tempList.Add(enemies[n]);
                }
            }
            else
            {
                break;
            }
        }


        selectedChance = Random.Range(0, totalRarity);
        // spawn tempList[selectedChance]
        if (tempList.Count > selectedChance)
        {
            activeEnemies.Add(Instantiate(tempList[selectedChance], selectedPos, new Quaternion()));
        }
        else
        {
            Debug.LogError("The selected enemy is not valid! \n" + tempList + " " + selectedChance);
        }
    }
}
