using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAircraft : MonoBehaviour {


		public GameObject explosion;

		void OnTriggerEnter(Collider impactObject)
		{
			if(HealthbarScript.health <= 10 )
			//if (impactObject.tag != "EnemyAI") 
		{
			//if(HealthbarScript.health <= 0 )

				Instantiate (explosion, transform.position, transform.rotation);
				Destroy (transform.parent.parent.gameObject);
			}

		}
		void OnTriggerEnter3D(Collider col)
		{
			//LevelControlScript.instance.youWin ();
		}

	}