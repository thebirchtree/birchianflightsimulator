/*Copyright 2017
Made by BirchTree
Created in September 2017
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scriptsphere : MonoBehaviour{
	public int count;
	public Text countText;

	void Start()
	{
		count = 100;

	}
	

	void OnTriggerEnter(Collider other) 
	{
		if (other.gameObject.CompareTag ("Currency")) {
			other.gameObject.SetActive (false);
			count = count + 1;
		}
	}




}
