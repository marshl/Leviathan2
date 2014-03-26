using UnityEngine;
using System.Collections;

[System.Obsolete]
public class PunchingBag : MonoBehaviour {

	public int teamNumber;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		if(Input.GetKeyDown(KeyCode.A))
		{
			this.rigidbody.AddForce(30,0,0);
		}
	
	}
}
