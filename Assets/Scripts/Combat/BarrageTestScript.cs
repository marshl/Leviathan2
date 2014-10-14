using UnityEngine;
using System.Collections;

public class BarrageTestScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown (KeyCode.Space))
		{
			this.GetComponent<WeaponBase>().SendFireMessage ();
		}
	
	}
}
