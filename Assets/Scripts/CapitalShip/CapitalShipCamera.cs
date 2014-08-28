using UnityEngine;
using System.Collections;
using System.Xml;

public class CapitalShipCamera : MonoBehaviour
{
	public CapitalShipMaster masterScript;
	
	public float xSpeed;
	public float ySpeed;
	
	public float distanceMin;
	public float distanceMax;
	
	public float scrollRate;
	
	public float currentDistance = 10.0f;
	
	private float x = 0.0f;
	private float y = 0.0f;

	public void Awake ()
	{
	    Vector3 angles = this.transform.eulerAngles;
	    this.x = angles.y;
	    this.y = angles.x;
	}

	public void LateUpdate()
	{
		/// Zoom controls on the scroll wheel
		this.currentDistance -= Input.GetAxis("Mouse ScrollWheel" ) * this.scrollRate;
		this.currentDistance = Mathf.Clamp( this.currentDistance, this.distanceMin, this.distanceMax );	
		
		/// If the right mouse button is held down, rotate the camera
    	if ( Input.GetMouseButton(1) == true )
		{
			this.x += Input.GetAxis("Mouse X") * this.xSpeed;
			this.y -= Input.GetAxis("Mouse Y") * this.ySpeed;
		}

        Quaternion rotation = Quaternion.Euler( y, x, 0 );
        Vector3 position = rotation * new Vector3( 0.0f, 0.0f, -this.currentDistance ) + this.masterScript.transform.position;
        
        this.transform.rotation = rotation;
    	this.transform.position = position;
	}
}
