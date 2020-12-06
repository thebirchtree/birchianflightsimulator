using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class THRUST : MonoBehaviour {
	public float thrust;
	public Rigidbody rb;
	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	void FixedUpdate()
	{
		rb.AddRelativeForce(Vector3.forward * 2);
	}
}