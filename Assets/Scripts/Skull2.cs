using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Skull2 : MonoBehaviour {

	// Use this for initialization
	public void OnTriggerEnter (Collider col ) {
		{
			HealthbarScript1.health -= 10f;
		}
	}


}
