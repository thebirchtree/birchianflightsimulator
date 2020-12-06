using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpMoney : MonoBehaviour {
	public int money;
	// Use this for initialization
	void Start () {
		money = 10;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public void addMoney (int moneyToAdd) {
		money += moneyToAdd;
	}
	public void subtractMoney (int moneyToSubtract) {
		if (money - moneyToSubtract < 0) {
			Debug.Log (" DONT HAVE ENOUGH MONEY");
		}
		else {
			money -= moneyToSubtract;
		}
	}
}
