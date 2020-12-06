using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	public static GameManager instance;

	public static int CoinCount { get { return PlayerPrefs.GetInt ("CoinCount"); } set { PlayerPrefs.SetInt ("CoinCoint", value); } }
	public void OnTriggerEnter (Collider col) {
		IncreaseCoins ();
		Destroy (gameObject);
	}
	public void IncreaseCoins () {
		GameManager.CoinCount++;
	}

	void Awake () {
		MakeSingleton ();
	}
	void MakeSingleton () {


		if (instance != null) {
			Destroy (gameObject);
		}

			else {
				instance = this;

		}
	}
	void Start () {
		if (!PlayerPrefs.HasKey ("HASPLAYEDBEFORE")) {
			PlayerPrefs.SetInt ("HASPLAYEDBEFORE", 200);

			PlayerPrefs.SetInt ("CoinCount", 200);
}
	}

}