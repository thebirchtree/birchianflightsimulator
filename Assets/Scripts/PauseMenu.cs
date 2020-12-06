using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour {
	public static bool GameIsPaused = false;
	public static bool GameIsResumed = true;
	public GameObject pauseMenuUI; 
	// Update is called once per frame
	public void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (GameIsPaused) {
				Resume ();
			} else if (GameIsResumed) {
				Pause();
			
			}
		}
	}
	public void Resume ()
	{
		pauseMenuUI.SetActive (false);
		Time.timeScale = 1f;
		GameIsPaused = false;
	}
	public void Pause () 
	{
		pauseMenuUI.SetActive (true);
		Time.timeScale = 0f;
		GameIsPaused = true;
	}
}
