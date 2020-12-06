
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
namespace PhotonTutorial.Menus

{
public class PlayerNameINput : MonoBehaviour {

			[SerializeField] private InputField nameInputField = null;
			[SerializeField] private Button continueButton = null;

			private const string PlayerPrefsNameKey = "PlayerName";

			private void Start () => SetUpInputField (); 
	
	
			private void SetUpInputField ()
			{
			if (!PlayerPrefs.HasKey (PlayerPrefsNameKey)) {return;}

			string defaultName = PlayerPrefs.GetString (PlayerPrefsNameKey);

			nameInputField.text = defaultName;
			SetPlayerName (defaultName);
			}

		public void SetPlayerName(string name)
		{
			continueButton.interactable = !string.IsNullOrEmpty (name);
		}

		public void SavePlayerName()
		{
			string playerName = nameInputField.text;

			PhotonNetwork.NickName = playerName;

		PlayerPrefs.SetString(PlayerPrefsNameKey, playerName);
		}
	
	}

}
