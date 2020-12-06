using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInv : MonoBehaviour

{
	public InventoryObject inventory;
    // Start is called before the first frame update
 
	public void OnTriggerEnter(Collider other)
	{
		var item = other.GetComponent<Item>();
		if (item)
		{
			inventory.AddItem(item.item, 1);
			Destroy(other.gameObject);
		}
	}
	public void SavePlayer()
	{
		

			inventory.Save();


	}
	public void LoadPlayer()
	{
		
			inventory.Load();

	}

}
