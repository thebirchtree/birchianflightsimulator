using UnityEngine;
using System.Collections;

public class SFX : MonoBehaviour {
	AudioSource sound;
		public AudioClip impact;

		void Start()
		{
			sound = GetComponent<AudioSource> ();
		}

		void OnTriggerEnter2D(Collider2D other)
		{ 
			if (Input.GetButtonDown("Fire1"))
			{    
				sound.PlayOneShot (impact);
			}
		}
	}
