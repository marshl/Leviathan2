
/// <summary>
/// Camera orbit.
/// </summary>
/// Modified 16/09/12: Added animation toggle for starfighter

using UnityEngine;
using System.Collections;

public class CameraOrbit : MonoBehaviour
{
	public Transform target;

	public float maxDistance;
	public float minDistance;
	public float distanceInterval;

	public float maxHeight;
	public float heightInterval;

	public float rotationRate;

	private float currentTime;
	
	private void Update ()
	{
		this.currentTime += Time.deltaTime;

		float angle = this.currentTime * this.rotationRate;
		float dist = Common.SmoothPingPong( this.currentTime, this.minDistance, this.maxDistance, this.distanceInterval );
		float height = Common.SmoothPingPong( this.currentTime, -this.maxHeight, this.maxHeight, this.heightInterval );
		float newX = Mathf.Sin( angle ) * dist;
		float newZ = Mathf.Cos( angle ) * dist;

		//Debug.Log( "Angle:" + angle + " Height:" + height + " Dist:" + dist );

		this.transform.position = this.target.position + new Vector3(
			newX,
			height,
			newZ );
		  
		this.transform.LookAt( this.target );
	}
}
