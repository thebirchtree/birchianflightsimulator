using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyMothership : MonoBehaviour {


	public GameObject explosion;

	void OnTriggerEnter(Collider impactObject)
	{
		if(HealthbarScript1.health <= 0) 
			//if (impactObject.tag != "EnemyAI") 
		{
			//if(HealthbarScript.health <= 0 )

			Instantiate (explosion, transform.position, transform.rotation);
			Destroy (transform.parent.gameObject);
		}

	}
	void OnTriggerEnter3D(Collider col)
	{
		//LevelControlScript.instance.youWin ();
	}

}