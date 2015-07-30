using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkPositionTest : MonoBehaviour
{
	public NetworkPositionControl other;
	public float turnSpeed;
	public float moveSpeed;
	public float twistSpeed;

	public float latency;
	public float sendRate;
	private float timer;
	
	public List<NetworkPositionControl.DataPoint> dataPoints;

	public bool goingLeft = true;
	public float turnAmount;
	public float turnStop;


	public bool turn;
	private void Awake()
	{
		this.dataPoints = new List<NetworkPositionControl.DataPoint>();
	}

	private void Update ()
	{
		this.timer += Time.deltaTime;
		// Store a "packet" every _sendRate_ seconds
		if ( this.timer >= this.sendRate )
		{
			this.timer -= this.sendRate;

			NetworkPositionControl.DataPoint data = new NetworkPositionControl.DataPoint();
			data.timeStamp = (double)Time.realtimeSinceStartup;
			data.position = this.transform.localPosition;
			data.rotation = this.transform.rotation;
			data.velocity = this.GetComponent<Rigidbody>() != null ? this.GetComponent<Rigidbody>().velocity : Vector3.zero;
			this.dataPoints.Add( data );
		}
		 
		// "Send" the packet once it is older than latency speed
		if ( this.dataPoints.Count > 0 
		    && Time.time - (float)this.dataPoints[0].timeStamp > this.latency )
		{
			this.other.StoreData( dataPoints[0].position, dataPoints[0].rotation, dataPoints[0].velocity, dataPoints[0].timeStamp );
			this.dataPoints.RemoveAt( 0 );
		}

		//this.transform.Rotate( this.transform.up, this.turnSpeed * Time.deltaTime );

		if ( this.GetComponent<Rigidbody>() != null )
		{
			this.GetComponent<Rigidbody>().velocity = this.transform.forward * this.moveSpeed;
			Debug.DrawRay( this.transform.position, this.GetComponent<Rigidbody>().velocity );
		}
		else
		{
			this.transform.position += this.transform.forward * Time.deltaTime * this.moveSpeed;
		}

		if ( this.turn )
		{
			this.transform.Rotate( this.transform.forward, this.twistSpeed * Time.deltaTime );
			this.transform.Rotate( this.transform.up, this.turnSpeed * Time.deltaTime * (this.goingLeft ? 1.0f : -1.0f) );
			this.turnAmount += Time.deltaTime;
			if ( this.turnAmount > this.turnStop )
			{
				this.goingLeft = !this.goingLeft;
				this.turnAmount = 0.0f;
			}
		}
		Debug.DrawLine( this.transform.position, this.other.transform.position, Color.red );
	}

	private void LateUpdate()
	{
		this.other.TransformLerp( Time.realtimeSinceStartup );
	}
}
