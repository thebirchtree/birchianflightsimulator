using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HOMING : MonoBehaviour {
	public Transform rocketTarget;
	public Rigidbody rocketRigidBody;
	public float rocketVelocity;
	public float turn;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		rocketRigidBody.velocity = transform.forward * rocketVelocity;
		var rocketTargetRotation = Quaternion.LookRotation(rocketTarget.position - transform.position); 
		rocketRigidBody.MoveRotation (Quaternion.RotateTowards (transform.rotation, rocketTargetRotation, turn));
	}
}
