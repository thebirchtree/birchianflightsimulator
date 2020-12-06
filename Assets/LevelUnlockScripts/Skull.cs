using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skull : MonoBehaviour {

	public void OnTriggerEnter(Collider col)
	{
		
		HealthbarScript.health -= 10f;
	}
}
