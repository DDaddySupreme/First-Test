using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickupScript : MonoBehaviour {

    public Item item;
    [Space]
    public int amount;


    private void OnTriggerEnter2D(Collider2D collision)
    {        
        if (collision.gameObject.tag == "WorldPlayer")
        {
            Destroy(gameObject);
            InventoryScript.Instance.ModifyInventory(amount, item);
        }
    }
}
