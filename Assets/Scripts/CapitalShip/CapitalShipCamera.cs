using UnityEngine;
using System.Collections;
using System.Xml;

public class CapitalShipCamera : MonoBehaviour
{
	/// <summary>
	/// The transform around which this camera will rotate
	/// </summary>
	public Transform targetTransform;

	/// <summary>
	/// The rate at which the camera will move to the left and right
	/// </summary>
	public float xSpeed;

	/// <summary>
	/// The rate at which the camrea will move up and down
	/// </summary>
	public float ySpeed;

	/// <summary>
	/// The minimum distance the camera can be from the target
	/// </summary>
	public float distanceMin;

	/// <summary>
	/// The maximum distance the camera can be from the target
	/// </summary>
	public float distanceMax;

	/// <summary>
	/// The rate at which the camera can be zoomed out per second
	/// </summary>
	public float scrollRate;

	/// <summary>
	/// The current distance away from the target
	/// </summary>
	public float currentDistance = 10.0f;

	/// <summary>
	/// The current left-right (longitudinal) angle around the target
	/// </summary>
	private float x = 0.0f;

	/// <summary>
	/// The current up-down (latitudinal) angle around the target
	/// </summary>
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
        Vector3 position = rotation * new Vector3( 0.0f, 0.0f, -this.currentDistance ) + this.targetTransform.position;
        
        this.transform.rotation = rotation;
    	this.transform.position = position;
	}
}
