using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMoney : MonoBehaviour
{
	public int gold;
	// Use this for initialization
	void Start () 
	{
		gold = 100;
			
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
		public void addMoney(int moneyToAdd)
		{
		gold += moneyToAdd;
		
		}
	public void subtractMoney(int moneytoSubtract)
	{
		if (gold - moneytoSubtract < 0) {
			Debug.Log ("We don't have enough gold");

		} else {
			gold -= moneytoSubtract;
		}
	}
}
