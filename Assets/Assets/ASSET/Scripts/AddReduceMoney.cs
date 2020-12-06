using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddReduceMoney : MonoBehaviour {
	PlayerMoney script;

	public int addAmount;

	void Start()
	{
		script = GameObject.FindWithTag ("GameController").GetComponent<PlayerMoney>();
	}

	void OnTriggerEnter(Collider obj)
	{
		if (obj.gameObject.tag == "Player")
			script.gold += addAmount;

		Destroy (gameObject);
	}
}

