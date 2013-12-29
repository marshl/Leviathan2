using UnityEngine;
using System.Collections;
using System.Xml;

public class CapitalShipCamera : MonoBehaviour
{
	public Transform targetTransform;

	public float xSpeed = 250.0f;
	public float ySpeed = 120.0f;

	public float yMinLimit = -20.0f;
	public float yMaxLimit = 80f;
	
	public float distanceMin = 5.0f;
	public float distanceMax = 20.0f;
	
	public float scrollRate = 5.0f;
	
	public float panSpeed = 1.0f;
	
	private float distance = 10.0f;
	private float x = 0.0f;
	private float y = 0.0f;
	
	public void Start ()
	{
		if ( this.targetTransform == null )
		{
			GameObject newObj = new GameObject();
			this.targetTransform = newObj.transform;
		}
		
	    Vector3 angles = this.transform.eulerAngles;
	    this.x = angles.y;
	    this.y = angles.x;
		
		this.distance = (this.distanceMin + this.distanceMax) / 2.0f;
	}
	

	public void LateUpdate()
	{
		/// Zoom controls on the scroll wheel
		this.distance -= Input.GetAxis("Mouse ScrollWheel" ) * this.scrollRate;
		this.distance = Mathf.Clamp( this.distance, this.distanceMin, this.distanceMax );	
		
		/// If the right mouse button is held down, rotate the camera
    	if ( Input.GetMouseButton(1) == true )
		{
	        this.x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
	        this.y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
	 		
	 		//this.y = ClampAngle( this.y, yMinLimit, yMaxLimit);   
		}
		
		/// If the middle mouse button is held down, pan the camera
		if ( Input.GetMouseButton(2) == true )
		{
			targetTransform.rotation = transform.rotation;
            targetTransform.Translate( Vector3.right * -Input.GetAxis("Mouse X") * panSpeed);
            targetTransform.Translate( transform.up * -Input.GetAxis("Mouse Y") * panSpeed, Space.World);
			
			this.targetTransform.localPosition = new Vector3( this.targetTransform.localPosition.x,
				Mathf.Max(0.0f, this.targetTransform.localPosition.y) , this.targetTransform.localPosition.z );
		}
		
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -this.distance) + this.targetTransform.position;
        
        this.transform.rotation = rotation;
    	this.transform.position = position;
	}
	
	/// Keeps the camera angle in the limits
	static public float ClampAngle( float _angle, float _min, float _max )
	{
		while ( _angle < -360.0f )
			_angle += 360.0f;
		while ( _angle > 360.0f )
			_angle -= 360.0f;
		
		return Mathf.Clamp( _angle, _min, _max );
	}
}
