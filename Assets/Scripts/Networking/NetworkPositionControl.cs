/// <summary>
/// Network position control.
/// 
/// "Already there" - Chrono Legionnaire
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class NetworkPositionControl : MonoBehaviour
{
	/// A position and rotation point sent over the network
	[System.Serializable]
	public struct DataPoint
	{
		public Vector3 position;
		public Quaternion rotation;
		public double timeStamp;

		public Vector3 velocity;
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

#if UNITY_EDITOR
	public float speed;
	public Vector3 velocity;
	public Vector3 rotVel;

	public int ownerID = -1;
#endif

	// Are we going to try to maintain our networked position?
	private bool readNewPositionData = true;
	
	// Unity Callback: Do not change signature
	private void Awake()
	{
		this.newerData = new DataPoint();
		this.olderData = new DataPoint();
	}

	// Unity Callback: Do not change signature
	private void OnSerializeNetworkView( BitStream _stream, NetworkMessageInfo _info )
	{
		if ( _stream.isWriting == true ) 
		{
			Vector3 pos = this.transform.position;
			Quaternion rot = this.transform.rotation;
			_stream.Serialize( ref pos );
			_stream.Serialize( ref rot );
			if ( this.GetComponent<Rigidbody>() != null )
			{
				Vector3 vel = this.GetComponent<Rigidbody>().velocity;
				_stream.Serialize( ref vel );
			}
		}
		else if ( _stream.isReading == true )
		{
			Vector3 pos = Vector3.zero;
			Quaternion rot = Quaternion.identity;
			_stream.Serialize( ref pos );
			_stream.Serialize( ref rot );

			Vector3 vel = Vector3.zero;
			if ( this.GetComponent<Rigidbody>() != null )
			{
				_stream.Serialize( ref vel );
			}

			this.StoreData( pos, rot, vel, _info.timestamp );
		}	
	}
	 
	/// <summary>
	/// Stores the data that is sent over the network (kept separate for testing purposes)
	/// </summary>
	/// <param name="_pos">The last known position</param>
	/// <param name="_rot">The last known rotation</param>
	/// <param name="_time">The time at which the pos and rot were sent</param>
	public void StoreData( Vector3 _pos, Quaternion _rot, Vector3 _vel, double _time )
	{
		if ( _time < this.newerData.timeStamp )
		{
			DebugConsole.Log( "Receiving early packet" );
			return;
		}

		this.olderData.rotation = this.newerData.rotation;
		this.olderData.position = this.newerData.position;
		this.olderData.velocity = this.newerData.velocity;
		this.olderData.timeStamp = this.newerData.timeStamp;

		this.newerData.rotation = _rot;
		this.newerData.position = _pos;
		this.newerData.velocity = _vel;
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
		if ( Network.peerType != NetworkPeerType.Disconnected 
		  && this.GetComponent<NetworkView>() != null
		  && !GetComponent<NetworkView>().isMine )
		{
			// This is separate to facilitate local testing
			this.TransformLerp( Network.time );
#if UNITY_EDITOR
			this.ownerID = Common.NetworkID( this.GetComponent<NetworkView>().viewID.owner );
#endif
		}

#if UNITY_EDITOR
		if ( this.GetComponent<Rigidbody>() != null )
		{
			this.speed = this.GetComponent<Rigidbody>().velocity.magnitude;
			this.velocity = this.GetComponent<Rigidbody>().velocity;
			this.rotVel = this.GetComponent<Rigidbody>().angularVelocity;
		}
#endif
	}

	/// <summary>
	/// Gradually moves this position towards where it is thought it should be at the current time
	/// </summary>
	/// <param name="_currentTime">The time</param>
	public void TransformLerp( double _currentTime )
	{
		// We need two or more points for this to work
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

		if ( this.GetComponent<Rigidbody>() != null && this.olderData.velocity.magnitude > 0.0f )
		{
			Vector3 velocityGuess = Vector3.Lerp( this.olderData.velocity, this.newerData.velocity, multiplier );
			Vector3 desiredPos = this.newerData.position + (float)timeSince * velocityGuess;
			this.transform.position = Vector3.Lerp( this.transform.position, desiredPos, this.lerpRate );

			this.GetComponent<Rigidbody>().velocity = velocityGuess;
			this.transform.rotation = targetRotation;
		}
		else
		{
			Vector3 targetPosition = this.newerData.position + this.transform.forward * multiplier * this.distTravelled;
            
            if ( this.lerp == true )
			{
				this.transform.position = Vector3.Lerp( this.transform.position, targetPosition, Time.deltaTime * this.lerpRate * this.distTravelled );
				this.transform.rotation = Quaternion.Slerp( this.transform.rotation, targetRotation, Time.deltaTime * this.lerpRate );
			}
			else
			{
				this.transform.position = targetPosition;
				this.transform.rotation = targetRotation;
			}
		}
	}

	public void SetUpdatePosition( bool _toggle )
	{
		DebugConsole.Log( "Toggled position updating to " + _toggle, this );
		this.readNewPositionData = _toggle;
	}

	public Vector3 CalculateVelocity()
	{
		if ( this.newerData.timeStamp == 0.0f || this.olderData.timeStamp == 0.0f )
		{
			return Vector3.zero;
		}
		else
		{
			return ( this.newerData.position - this.olderData.position ) / (float)this.timeDiff;
		}
	}
	
}
