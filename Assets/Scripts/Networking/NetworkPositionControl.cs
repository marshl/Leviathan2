using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NetworkPositionControl : MonoBehaviour
{
	/// A position and rotation point sent over the network
	public struct DataPoint
	{
		public Vector3 position;
		public Quaternion rotation;
		public double timeStamp;
	};
	

	// Two points of data to be used to guess the future position
	private DataPoint olderData;
	private DataPoint newerData;

	// Precalculated variables that only have to be set every time a packet is received
	private double timeDiff = 0.0f;
	private float distTravelled = 0.0f;
	private Vector3 axis = Vector3.up;
	private float twistChange;

	/// <summary>
	/// Will this object lerp its position towards the guesstimate?
	/// </summary>
	public bool lerp;
	
	/// <summary>
	/// If it is, how fast will it move
	/// </summary>
	public float lerpRate;

	// Are we going to try to maintain our networked position?
	private bool readNewPositionData = true;

	public int ownerID = -1;

	// Unity Callback: Do not change signature
	private void Awake()
	{
		// Various reasons why this script is not needed
		if ( this.networkView == null
		  || networkView.isMine == true 
		  || Network.peerType == NetworkPeerType.Disconnected )
		{
			this.enabled = false;
			return;
		}
		this.newerData = new DataPoint();
		this.olderData = new DataPoint();
	}

	// Unity Callback: Do not change signature
	private void OnSerializeNetworkView( BitStream _stream, NetworkMessageInfo _info )
	{
		if ( _stream.isWriting == true ) 
		{
			Vector3 pos = this.transform.localPosition;
			Quaternion rot = this.transform.localRotation;
			_stream.Serialize( ref pos );
			_stream.Serialize( ref rot );
		}
		else if ( _stream.isReading == true )
		{
			Vector3 pos = Vector3.zero;
			Quaternion rot = Quaternion.identity;
			_stream.Serialize( ref pos );
			_stream.Serialize( ref rot );

			this.StoreData( pos, rot, _info.timestamp );
		}	
	}
	 
	/// <summary>
	/// Stores the data that is sent over the network (kept separate for testing purposes)
	/// </summary>
	/// <param name="_pos">The last known position</param>
	/// <param name="_rot">The last known rotation</param>
	/// <param name="_time">The time at which the pos and rot were sent</param>
	public void StoreData( Vector3 _pos, Quaternion _rot, double _time )
	{
		if ( _time < this.newerData.timeStamp )
		{
			Debug.Log( "receiving early packet" );
			return;
		}

		this.olderData.rotation = this.newerData.rotation;
		this.olderData.position = this.newerData.position;
		this.olderData.timeStamp = this.newerData.timeStamp;

		this.newerData.rotation = _rot;
		this.newerData.position = _pos;
		this.newerData.timeStamp = _time;
		
		this.timeDiff = this.newerData.timeStamp - this.olderData.timeStamp;

		Vector3 vectorTravelled = this.newerData.position - this.olderData.position;
		this.distTravelled = vectorTravelled.magnitude;

		Vector3 oldForward = this.olderData.rotation * Vector3.forward;
		Vector3 newForward = _rot * Vector3.forward;
		if ( oldForward != newForward )
		{
			this.axis = Vector3.Cross( oldForward, newForward );
		}
	}

	private void Update()
	{
		// This is separate to facilitate local testing
		this.TransformLerp( Network.time );

		this.ownerID = Common.NetworkID( this.networkView.viewID.owner );
	}

	/// <summary>
	/// Gradually moves this position towards where it is thought it should be at the current time
	/// </summary>
	/// <param name="_currentTime">The time</param>
	public void TransformLerp( double _currentTime )
	{
		// We need two or more points for this to work
		//TODO: A basic move using only one point (LM:21/02/14)
		if ( olderData.timeStamp == 0.0f 
		    || newerData.timeStamp == 0.0f
		    || !readNewPositionData)
		{
			return;
		}

		double timeSince = _currentTime - this.newerData.timeStamp;
		float multiplier = (float)(timeSince / timeDiff);

		Quaternion targetRotation = this.newerData.rotation * Quaternion.AngleAxis( 
			 Quaternion.Angle( this.olderData.rotation, this.newerData.rotation ) * multiplier, this.axis
	    );

		Vector3 targetPosition = this.newerData.position + this.transform.forward * multiplier * this.distTravelled;

		if ( this.lerp == true )
		{
			this.transform.localPosition = Vector3.Lerp( this.transform.localPosition, targetPosition, Time.deltaTime * this.lerpRate * this.distTravelled );
			this.transform.localRotation = Quaternion.Slerp( this.transform.localRotation, targetRotation, Time.deltaTime * this.lerpRate );
		}
		else
		{
			this.transform.localPosition = targetPosition;
			this.transform.localRotation = targetRotation;
		}
	}

	public void TogglePositionUpdates( bool _toggle )
	{
		print("Toggled position updating to " + _toggle);
		readNewPositionData = _toggle;
	}

	public Vector3 CalculateVelocity()
	{
		/*//First up, we need to know the two most recent position data points.
		//These are used to calculate the distance moved in that time.

		Vector3 travelledVector = this.newerData.position - this.olderData.position;

		//Next we scale this vector up using the time difference to work out one second of velocity.
		//I really badly wish you could overload Vector3 operators for this.

		travelledVector.x = travelledVector.x * (float)(1 / timeDiff ) ;
		travelledVector.y = travelledVector.y * (float)(1 / timeDiff ) ;
		travelledVector.z = travelledVector.z * (float)(1 / timeDiff ) ;

		//This will not be perfect, but it should be good enough to be useful.

		return travelledVector;
		*/


		return ( this.newerData.position - this.olderData.position ) / (float)this.timeDiff;
	}
	
}
