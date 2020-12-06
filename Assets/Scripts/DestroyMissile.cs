﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyMissile : MonoBehaviour
{


	public GameObject explosion;

	void OnTriggerEnter(Collider impactObject)
	{
		if (impactObject.tag != "Nissile")
		{

			Instantiate(explosion, transform.position, transform.rotation);
			Destroy(transform.parent.parent.gameObject);
		}

	}
	void OnTriggerEnter3D(Collider col)
	{
		//LevelControlScript.instance.youWin ();
	}

}