using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LEVELSELECTOR : MonoBehaviour {
	public Button L2, L3, L4, L5, L6, L7, L8;
	int levelPassed;
	public void Start () {
		levelPassed = PlayerPrefs.GetInt("LevelPassed");
		L2.interactable = false;
		L3.interactable = false;
		L4.interactable = false;
		L5.interactable = false;
		L6.interactable = false;
		L7.interactable = false;
		L8.interactable = false;

		switch (levelPassed) {
		case 1:
			L2.interactable = true;
			break;
		case 2:
			L2.interactable = true;
			L3.interactable = true;
			break;
		case 3:
			L2.interactable = true;
			L3.interactable = true;
			L4.interactable = true;
			break;
		case 4:
			L2.interactable = true;
			L3.interactable = true;
			L4.interactable = true;
			L5.interactable = true;
			break;
		case 5:
			L2.interactable = true;
			L3.interactable = true;
			L4.interactable = true;
			L5.interactable = true;
			L6.interactable = true;
			break;
		case 6:
			L2.interactable = true;
			L3.interactable = true;
			L4.interactable = true;
			L5.interactable = true;
			L6.interactable = true;
			L7.interactable = true;
			break;
		case 7:
			L2.interactable = true;
			L3.interactable = true;
			L4.interactable = true;
			L5.interactable = true;
			L6.interactable = true;
			L7.interactable = true;
			L8.interactable = true;
			break;


		}

	}
	public void leveltoload (int level){
		SceneManager.LoadScene (level);
	}
}
