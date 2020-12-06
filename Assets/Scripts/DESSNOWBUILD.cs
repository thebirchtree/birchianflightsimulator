using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DESSNOWBUILD : MonoBehaviour {


	public GameObject explosion;

	void OnTriggerEnter(Collider impactObject)
	{
		if (impactObject.tag != "Building") {

			Instantiate (explosion, transform.position, transform.rotation);
			Destroy (transform.gameObject);
		}

	}
	void OnTriggerEnter3D(Collider col)
	{
		//LevelControlScript.instance.youWin ();
	}

}