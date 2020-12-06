using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthbarScript1 : MonoBehaviour {

	public Image healthBar;
	public  Text healthText;
	public float maxHealth = 100f;
	public static float health;
	public void Start ()
	{
		healthBar = GetComponent<Image> ();
		health = maxHealth;

	}
	public void Update () {
		healthBar.fillAmount = health / maxHealth;
		healthText.text = health.ToString () + " / " + maxHealth.ToString ();
		 
	}

}
