using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinScript : MonoBehaviour {

	void OnTriggerEnter (Collider col)
	{
		GameControlScript.moneyAmount += 10;
		Destroy (gameObject);
	}
}
