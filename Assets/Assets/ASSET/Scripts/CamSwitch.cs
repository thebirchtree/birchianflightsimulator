using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamSwitch : MonoBehaviour {
		public Camera Camera1;
		public Camera Camera2;

		public void ShowOverheadView() {
		Camera1.enabled = false;
			Camera2.enabled = true;
		}

		public void ShowFirstPersonView() {
		Camera1.enabled = true;
		Camera2.enabled = false;
		}
	}