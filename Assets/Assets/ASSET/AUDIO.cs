using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AUDIO : MonoBehaviour {
	AudioSource audioSource;

	void Start()
	{
		audioSource = GetComponent<AudioSource>();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
			audioSource.mute = !audioSource.mute;
	}
}