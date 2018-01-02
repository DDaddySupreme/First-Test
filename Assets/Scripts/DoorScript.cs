using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class DoorScript : MonoBehaviour {

    public string levelName;
    public Vector3 spawnCoords;
    [Space][Multiline]
    public string description;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "WorldPlayer" && LoadInfo.Instance.gameState == LoadInfo.GameState.World)
        {
            if (levelName == "" || levelName == "this")
            {
                collision.transform.position = spawnCoords;
            }
            else
            {
                LoadInfo.Instance.spawnCoords = spawnCoords;
                LoadInfo.Instance.returnLevel = SceneManager.GetActiveScene();
                SceneManager.LoadScene(levelName);
            }
        }
    }

}
