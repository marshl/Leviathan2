using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkPositionControl : MonoBehaviour
{
	private struct DataPoint
	{
		/*public DataPoint( Vector3 _pos, Quaternion _forward, double _timeStamp )
		{
			this.position = _pos;
			this.rotation = _rotation;
			this.timeStamp = _timeStamp;
		}
*/
		public Vector3 position;
		//public Vector3 forward;
		public Quaternion rotation;
		public double timeStamp;
	};
	
	private DataPoint olderData;
	private DataPoint newerData;

	private double timeDiff = 0.0f;
	private float distTravelled = 0.0f;
	//private float angleChange = 0.0f;
	private Vector3 axis = Vector3.up;
	private float twistChange;

	public bool lerp;
	public float lerpRate;

	   
	private void Awake()
	{
		if ( this.networkView == null
		|| networkView.isMine == true
		|| ( Network.isClient == false
		  && Network.isServer == false ) )
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
			//Vector3 forward = this.transform.forward;
			_stream.Serialize( ref pos );
			_stream.Serialize( ref rot );
			//_stream.Serialize( ref forward );
		}
		else if ( _stream.isReading == true )
		{
			Vector3 pos = Vector3.zero;
			Quaternion rot = Quaternion.identity;
			//Vector3 forward = Vector3.zero;
			_stream.Serialize( ref pos );
			_stream.Serialize( ref rot );
			//_stream.Serialize( ref forward );

			this.SerialiseData( pos, rot, _info.timestamp );
			//this.SerialiseData( pos, forward, _info.timestamp );
		}	
	}
	 
	public void SerialiseData( Vector3 _pos, 
	                         //Vector3 _forward, 
	                           Quaternion _rot, 
	                          double _time )
	{
		//this.olderData.forward = this.newerData.forward;
		this.olderData.rotation = this.newerData.rotation;
		this.olderData.position = this.newerData.position;
		this.olderData.timeStamp = this.newerData.timeStamp;

		//this.newerData.forward = _forward;
		this.newerData.rotation = _rot;
		this.newerData.position = _pos;
		this.newerData.timeStamp = _time;
		
		this.timeDiff = this.newerData.timeStamp - this.olderData.timeStamp;
		//Debug.Log( "TimeDiff: " + timeDiff );
		 
		/*if ( this.newerData.forward != this.olderData.forward )
		{
			this.axis = Vector3.Cross( this.olderData.forward, this.newerData.forward );
			this.angleChange = Vector3.Angle( this.olderData.forward, this.newerData.forward );
		}
		*/
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
		this.GuesstimatePosition( Network.time );
	}

	public void GuesstimatePosition( double _currentTime )
	{
		// We need two or more points for this to work
		//TODO: A basic move using only one point (LM:21/02/14)
		if ( olderData.timeStamp == 0.0f 
		    || newerData.timeStamp == 0.0f )
		{
			return;
		}

		double timeSince = _currentTime - this.newerData.timeStamp;
		float multiplier = (float)(timeSince / timeDiff);
		//Debug.Log( "OldTime:" + this.olderData.timeStamp + " NewTime:" + this.newerData.timeStamp + "Diff:" + this.timeDiff + " Since:" + timeSince + " Multiplier: " + multiplier );
		//Vector3 forwardGuess = Quaternion.AngleAxis( this.angleChange * multiplier, this.axis ) * this.newerData.forward;
		
		//Quaternion guessRotation = Quaternion.Lerp( this.olderData.rotation, this.newerData.rotation, 1.0f + multiplier );
		//Quaternion guessRotation = this.olderData.rotation * this.newerData.rotation;
		//Vector3 oldEuler = this.olderData.rotation.eulerAngles;
		//Vector3 newEuler = this.newerData.rotation.eulerAngles;

		//Quaternion increment = Quaternion.Lerp( this.olderData.rotation, this.newerData.rotation, multiplier );


		//Vector3 rotEuler = newEuler + ( newEuler - oldEuler ) * multiplier;
		//Quaternion.Lerp(
		//TODO: This should probably lerp towards the target position/rotation
		//TODO: Derp, this doesn't handle twist
		//this.transform.LookAt( this.transform.position + forwardGuess );
		//Debug.Log( guessRotation );

		//this.transform.rotation = increment * this.newerData.rotation;
		Quaternion targetRotation = this.newerData.rotation * Quaternion.AngleAxis( Quaternion.Angle( this.olderData.rotation, this.newerData.rotation ) * multiplier, this.axis );

		//this.transform.localRotation = guessRotation;
		//this.transform.localRotation = Quaternion.Euler( rotEuler );
		//this.transform.localPosition = this.newerData.position;
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
		//this.transform.position = this.newerData.position + forwardGuess * multiplier * this.distTravelled;
	}
}
