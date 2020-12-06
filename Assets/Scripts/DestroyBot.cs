using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyBot : MonoBehaviour {


	public GameObject explosion;
	public TextMesh Text;
	public void OnTriggerEnter(Collider impactObject)
	{
		if (impactObject.tag != "Enemy") 
		{
				Instantiate (explosion, transform.position, transform.rotation);
			Destroy (transform.parent.parent.gameObject);

		}

	}
	void OnTriggerEnter3D(Collider col)
	{
		//LevelControlScript.instance.youWin ();
	}

}