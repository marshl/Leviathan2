using UnityEngine;
using System.Collections;
using System.Xml;

public class RotatingCamera : MonoBehaviour
{
	public Transform target;
	
	public float xSpeed = 1.0f;
	public float ySpeed = 1.0f;
	
	public float distanceMin = 5.0f;
	public float distanceMax = 15.0f;
	
	public float scrollRate = 1.0f;
	
	public float currentDistance = 10.0f;
	
	public float x = 0.0f;
	public float y = 20.0f;

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
        Vector3 position = rotation * new Vector3( 0.0f, 0.0f, -this.currentDistance ) + this.target.position;
        
        this.transform.localRotation = rotation;
    	this.transform.localPosition = position;
	}
}
