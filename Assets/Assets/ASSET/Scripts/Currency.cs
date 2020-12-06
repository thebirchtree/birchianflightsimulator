using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Currency : MonoBehaviour
{

	public int gold;
	GameObject currencyUI;

	void Start()
	{
		currencyUI = GameObject.Find("Pickup");
	}
	void Update()
	{
		currencyUI.GetComponent<Text>().text = gold.ToString();
		if (gold < 0)
		{
			gold = 0;
		}
	}
	void OnTriggerEnter3D (Collider2D col)
	{
		GameControlScript.moneyAmount += 1;
		Destroy (gameObject);
	}
}