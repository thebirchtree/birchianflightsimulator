using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public class Login : MonoBehaviour {
	public GameObject username;
	public GameObject email;
	public GameObject password;
	public GameObject confPassword;
	private string Username;
	private string Email;
	private string Password;
	private string ConfPassword;
	private string form;
	private bool EmailValid = false;

	// Use this for initialization
	void Start () {
		
	}
	public void RegisterButton(){ 
		bool UN = false;
		bool EM = false;
		bool PW = false;
		bool CPW = false;

		if (Username != "") {
			if (System.IO.File.Exists (@"E:/UnityTestFolder/" + Username + ".txt")) {
				UN = true;
			} else { 
				Debug.LogWarning ("Username Taken");
			}

		} else {
			Debug.LogWarning ("Username Field Empty");
		} if (Email != "") {
			if (EmailValid) {
				if (Email.Contains ("@")) {
					if (Email.Contains (".")) {
						EM = true;
					}
				}
			}
		}
			
		print ("Registration Succesful");
		
	}
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Return)) {
			if (Password != ""&&Email != ""&&Password != ""&&ConfPassword != "") {
				RegisterButton();
		}
		Username = username.GetComponent<InputField> ().text;
		Email = email.GetComponent<InputField> ().text;
			Password = password.GetComponent<InputField> ().text;
			ConfPassword = confPassword.GetComponent<InputField> ().text;

	}
}
}
