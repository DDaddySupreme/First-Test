using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour {

    public float movementSpeed;
    public float maxSpeed;
    public float invalnurability;
    public PauseMenuScript pauseController;

    public LayerMask interactionLayers;
    public Vector2 rotation; 
    public RaycastHit2D interactionRay;
    private LoadInfo loadInfo;
    private Rigidbody2D rigidBody;
    private Vector2 input;

	void Start () {
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
        loadInfo = LoadInfo.Instance;

        transform.position = loadInfo.spawnCoords;
        //pauseController.CloseDialogue();
    }

	void Update () {
        if (loadInfo.pause == false)
        {
            if (invalnurability > 0)
            {
                invalnurability -= Time.deltaTime;
            }
            else if (invalnurability < 0)
            {
                invalnurability = 0;
            }

            input.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            rigidBody.velocity = input * movementSpeed;
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) < Mathf.Abs(Input.GetAxisRaw("Vertical")))
            {
                if (Input.GetAxisRaw("Vertical") > 0)
                {
                    rotation = Vector2.up;
                }
                else
                {
                    rotation = Vector2.down;
                }
            }
            else if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > Mathf.Abs(Input.GetAxisRaw("Vertical")))
            {
                if (Input.GetAxisRaw("Horizontal") > 0)
                {
                    rotation = Vector2.right;
                }
                else
                {
                    rotation = Vector2.left;
                }
            }
            else
            {
                // To do: diagonal? Prolly not
            }
        }
        else
        {
            rigidBody.velocity = new Vector3(0, 0);
        }

        //gameObject.GetComponent<SpriteRenderer>().sortingOrder = 

        if (Input.GetButtonUp("Interact") && !loadInfo.pause)
        {
            Debug.Log("Interacting..."); 
            interactionRay = Physics2D.Raycast(transform.position, rotation, 1, interactionLayers); // To do: learn how to use layermasks without the inspector
            if (interactionRay)
            {
                switch (interactionRay.collider.tag)
                {
                    case "Walls":
                        Debug.Log("Just a wall.");
                        break;

                    case "WorldEnemy":
                        Debug.Log("Look out, an enemy!");
                        break;

                    case "Item":
                        Debug.Log(interactionRay.collider.gameObject.GetComponent<ItemPickupScript>().item.Description);
                        break;

                    case "Door":
                        Debug.Log(interactionRay.collider.gameObject.GetComponent<DoorScript>().description);
                        break;

                    case "Chest":
                        ChestScript chest = interactionRay.collider.gameObject.GetComponent<ChestScript>();
                        for (int n = 0; n < chest.items.Count; n++)
                        {
                            if (n < chest.amounts.Count)
                            {
                                InventoryScript.Instance.ModifyInventory(chest.amounts[n], chest.items[n]);
                            }
                        }
                        break;

                    case "WorldNpc":
                        Debug.Log("Talking to an NPC!");
                        pauseController.SetupDialogue(interactionRay.collider.gameObject.GetComponent<NpcWorldScript>().dialogue);
                        break;
                }
            }
            else
            {
                
                Debug.Log("Nothing to interact with!");
            }
        }
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "WorldEnemy" && invalnurability <= 0)
        {
            loadInfo.EnterBAttle(collision.gameObject);
            return;
        }

    }
}
