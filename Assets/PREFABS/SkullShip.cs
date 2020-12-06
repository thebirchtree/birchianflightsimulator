using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullShip : MonoBehaviour {

	public void OnTriggerEnter(Collider col)
	{

		HealthbarScript1.health -= 10f;
	}
}
