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

	private List<NetworkPositionControl.DataPoint> dataPoints;

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
			this.timer = 0.0f;

			NetworkPositionControl.DataPoint data = new NetworkPositionControl.DataPoint();
			data.timeStamp = (double)Time.time;
			data.position = this.transform.localPosition;
			data.rotation = this.transform.rotation;
			 
			this.dataPoints.Add( data );
		}
		 
		// "Send" the packet once it is older than latency speed
		if ( this.dataPoints.Count > 0 
		    && Time.time - (float)this.dataPoints[0].timeStamp > this.latency )
		{
			this.other.StoreData( dataPoints[0].position, dataPoints[0].rotation, dataPoints[0].timeStamp );
			this.dataPoints.RemoveAt( 0 );
		}

		this.other.TransformLerp( Time.realtimeSinceStartup );
		this.transform.Rotate( this.transform.up, this.turnSpeed * Time.deltaTime );
		this.transform.Rotate( this.transform.forward, this.twistSpeed * Time.deltaTime );
		this.transform.position += this.transform.forward * Time.deltaTime * this.moveSpeed;
	}
}
