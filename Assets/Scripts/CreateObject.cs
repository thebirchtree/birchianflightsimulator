using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateObject : MonoBehaviour {
	public Transform Spawnpoint;
	public GameObject Prefab;
//	public float timeUntilNextPrefab 3.0f;
	// Use this for initialization
	void Start()
	{
		StartCoroutine (OnTriggerEnter ());
	}
	IEnumerator OnTriggerEnter () 
	{
		yield return new WaitForSeconds(3);

		GameObject RigidPrefab;

		RigidPrefab = Instantiate (Prefab, Spawnpoint.position, Spawnpoint.rotation) as GameObject;
		yield return new WaitForSeconds(3);

	}
	

}
