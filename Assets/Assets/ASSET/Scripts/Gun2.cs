using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun2 : MonoBehaviour {
	public float damage = 50f;
	public float range = 1000f;
	public WeaponObject[] weapons;
	public int currentWeapon = 5;
	public Camera fpsCam;
	// Use this for initialization
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButton("Fire1")) {
		}
	}
	void Shoot()
	{
		RaycastHit hit;
		if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, weapons[currentWeapon].range))
		{
			Debug.Log (hit.transform.name);
			 
			Target target = hit.transform.GetComponent<Target>();
			if (target != null) {
				target.TakeDamage(damage);
			}
		}

	}
}

