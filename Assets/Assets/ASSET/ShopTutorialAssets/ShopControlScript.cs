using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShopControlScript : MonoBehaviour {

	int moneyAmount = 0;
	int isRifleSold;

	public Text moneyAmountText;
	public Text riflePrice;

	public Button buyButton;

	// Use this for initialization
	void Start () {
		moneyAmount = PlayerPrefs.GetInt ("MoneyAmount");
	}
	
	// Update is called once per frame
	void Update () {
		
		moneyAmountText.text = "Money: " + moneyAmount.ToString() + "$";

		isRifleSold = PlayerPrefs.GetInt ("IsRifleSold");

		if (moneyAmount >= 1 && isRifleSold == 0)
			buyButton.interactable = true;
		else
			buyButton.interactable = false;	
	}

	public void buyRifle()
	{
		moneyAmount -= 0;
		PlayerPrefs.SetInt ("IsRifleSold", 1);
		riflePrice.text = "Sold!";
		buyButton.gameObject.SetActive (false);
	}

	public void exitShop()
	{
		PlayerPrefs.SetInt ("MoneyAmount", moneyAmount);
		SceneManager.LoadScene ("LevelFinder");
	}
		

}
