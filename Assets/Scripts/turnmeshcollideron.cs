using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class turnmeshcollideron : MonoBehaviour {

	public float afterSeconds;

	// Use this for initialization
	void Start () {
		Invoke ("Trigger", afterSeconds);

	}

	// Update is called once per frame
	void Update () {
		transform.GetComponent<MeshCollider> ().enabled = true;

	}
}
