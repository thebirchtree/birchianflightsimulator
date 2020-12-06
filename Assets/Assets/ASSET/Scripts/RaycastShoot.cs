using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastShoot : MonoBehaviour {
	public int gunDamage = 1;
	public float fireRate = .2f;
	public float weaponRange = 10000;
 	public float hitForce = 100f;
	public Transform gunEnd;


	private Camera fpsCam;
	private WaitForSeconds shotDuration = new WaitForSeconds(0.7f);
	private AudioSource gunAudio;
	private LineRenderer laserLine;
	private float nextFire;

	void Start () {
		laserLine = GetComponent<LineRenderer> ();
		gunAudio = GetComponent<AudioSource> ();
		fpsCam = GetComponentInParent <Camera> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("Fire1") && Time.time >nextFire)

			nextFire = Time.time + fireRate;

			StartCoroutine(ShotEffect());

			Vector3 rayOrigin = fpsCam.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 0));
			RaycastHit hit;

			laserLine.SetPosition(0, gunEnd.position);

			if (Physics.Raycast(rayOrigin,fpsCam.transform.forward, out hit, weaponRange))
				{
				laserLine.SetPosition(1, hit.point);

				}
			else
			{
				laserLine.SetPosition(1, rayOrigin + (fpsCam.transform.forward * weaponRange));
			}

		}


	private IEnumerator ShotEffect()
			{
				gunAudio.Play ();

				laserLine.enabled = true;
				yield return shotDuration;
				laserLine.enabled = false;
			}
	}