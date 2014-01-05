using UnityEngine;
using System.Collections;

public class NetworkCube : MonoBehaviour
{
	private void Awake()
	{
		if ( this.networkView.isMine == false )
		{
			this.enabled = false;
		}
	}

	private void Update ()
	{
		this.rigidbody.velocity = new Vector3( Input.GetAxis( "Horizontal" ), 0.0f, Input.GetAxis( "Vertical" ) );
	}
}
