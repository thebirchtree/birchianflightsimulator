using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 
public class MainMenuManager : MonoBehaviour {
	public Text currentCoinText;

	// Update is called once per frame
	public void Update () {
		currentCoinText.text = GameManager.CoinCount.ToString ();
	}
	public void IncrementCoins () {
		GameManager.CoinCount++;
	}

}
