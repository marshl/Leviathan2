using UnityEngine;
using System.Collections;

public class NetworkPositionTest : MonoBehaviour
{
	public NetworkPositionControl other;
	public float turnSpeed;
	public float moveSpeed;
	public float twistSpeed;

	public float latency;
	private float timer;
	
	private void Update ()
	{
		this.timer += Time.deltaTime;
		if ( this.timer >= this.latency )
		{
			this.timer = 0.0f;
			this.other.SerialiseData( this.transform.position, this.transform.rotation, Time.realtimeSinceStartup );
			//this.other.SerialiseData( this.transform.position, this.transform.forward, Time.realtimeSinceStartup );
		}
		 
		this.other.GuesstimatePosition( Time.realtimeSinceStartup );
		this.transform.Rotate( this.transform.up, this.turnSpeed * Time.deltaTime );
		this.transform.Rotate( this.transform.forward, this.twistSpeed * Time.deltaTime );
		this.transform.position += this.transform.forward * Time.deltaTime * this.moveSpeed;
	}
}
