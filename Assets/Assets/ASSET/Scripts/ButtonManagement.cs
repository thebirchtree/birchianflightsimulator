using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManagement : MonoBehaviour {

	// Use this for initialization
	public void NewGameBtn(string newGameLevel)
	{
		SceneManager.LoadScene(newGameLevel);

	}
	public void ExitGameBtn()
	{
		Application.Quit ();
	}
}
