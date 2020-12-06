using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class PlayerController : MonoBehaviour {

	public float speed;
	public Text countText;
	public Text winText;
	public GameObject WinGO;
	private Rigidbody rb;
	public int count;
	public GameObject winSound;

	void Start ()
	{
		rb = GetComponent<Rigidbody>();
		count = 0;
		SetCountText ();
		winText.text = "";
	}

	void FixedUpdate ()
	{
	//float moveHorizontal = Input.GetAxis ("Horizontal");
	//	float moveVertical = Input.GetAxis ("Vertical");

		//Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);

	//	rb.AddForce (movement * speed);
	}
	void OnTriggerEnter(Collider other) 
	{
		if (other.gameObject.CompareTag ( "pickup"))
		{
			other.gameObject.SetActive (false);
			count = count + 1;
			SetCountText ();
		}
	}

	void SetCountText ()
	{
		countText.text = "Coins: " + count.ToString ();
		if (count >= 3)
		{
			WinGO.SetActive (true);	
			winSound.SetActive (true);	

		}
	}


}