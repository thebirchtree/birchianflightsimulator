using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyself : MonoBehaviour {

		public float timeUntilDestruction;


		// Use this for initialization
		void Start () {
			Invoke ("SelfDestruct", timeUntilDestruction);
		}

		void SelfDestruct()
		{
			Destroy (gameObject);
		}

	}
