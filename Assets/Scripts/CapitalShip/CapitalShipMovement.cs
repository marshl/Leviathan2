using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CapitalShipMovement : MonoBehaviour
{
	public CapitalShipMaster master;
	
	public GameObject rotationSegmentPrefab;
	
	public LineRenderer tentativePathLine;
	
	public int rotationSegmentCount;
	
	// The increase in scale for the tentative rotation segments
	//   (the segments that many degrees the ship would turn if the user clicked)
	public float tentativeSegmentUpscale;
	
	// The radians that ship still has to turn
	private float currentTurnAmount = 0.0f;
	
	private bool isTurning = false;

	// The direction that the ship is currently turning (-1.0f = left, 1.0f = right)
	private float currentTurnDirection = 0.0f;

	private Vector3 lastSelectedPosition;
	
	// The rotational segments that the show where the ship is turning
	private GameObject[] rotationSegmentsActual;
	// The rotational segments that shows where the ship would turn
	private GameObject[] rotationSegmentsTentative;

	public float currentMovementSpeed;
	
	public int lineVertexCount;
	
	public float pathLength;
	
	// The radians that the ship turns per second
	public float turnRate;
	
	public float maxMoveSpeed;
	public float minMoveSpeed;

	public float moveAcceleration;

	public Transform avoidanceTransform;
	private CapitalShipMovement otherShip;
	public float avoidanceAngleRate;
	public float avoidanceAngleMax;

	public enum AVOIDANCE_STATE
	{
		NONE,
		UP_START,
		UP_RETURN,
		DOWN_START,
		DOWN_RETURN,
	};
	private AVOIDANCE_STATE currentAvoidanceState = AVOIDANCE_STATE.NONE;
	public float avoidanceCeiling;
	public float avoidanceFloor;
	public float avoidanceCurveDropRate;
	private bool isFollowingAvoidanceCurve;
	private float avoidanceCurveStartHeight;
	private float avoidanceCurveEndHeight;
	private float avoidCurveLength;
	private float currentAvoidCurvePoint;

	private float currentAvoidHeight;
	private float currentAvoidAngle;

	public float avoidanceAngleMultiplier;

	private void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		if ( this.networkView.isMine == false )
		{
			this.enabled = false;
		}
	}

	private void Start()
	{
		this.currentMovementSpeed = (this.maxMoveSpeed + this.minMoveSpeed) / 2;
#if UNITY_EDITOR
		if ( this.master.dummyShip == false )
#endif
		{
			this.tentativePathLine.SetVertexCount( lineVertexCount );
			
			this.CreateRotationSegments();
		}
	
		CapitalShipMovement[] otherScripts = GameObject.FindObjectsOfType<CapitalShipMovement>() as CapitalShipMovement[];
		foreach ( CapitalShipMovement other in otherScripts )
		{
			if ( other != this )
			{
				this.otherShip = other;
			}
		}
	}

	private void Update()
	{
		if ( this.master.isDying == false 
#if UNITY_EDITOR
		  && this.master.dummyShip == false
#endif 
		)
		{
			this.UpdateAccelerationInput();
			this.UpdateTurnLine();
		}

		// If the current position + the radius of a turn would place this ship out of bounds,
		//   start an emergency turn
		if ( IsPointOutOfBounds(this.transform.position + this.transform.forward * GetTurnRadiusAtSpeed( this.currentMovementSpeed )) )
		{
			float angle = Vector3.Dot( transform.right, (GameBoundary.instance.origin - this.transform.position).normalized );
			float direction = angle > 0.0f ? 1.0f : -1.0f;

			this.BeginTurn( angle, direction );
		}

		if ( this.isTurning == true )
		{
			this.currentTurnAmount -= Time.deltaTime * turnRate;
			this.transform.Rotate( Vector3.up, turnRate * Time.deltaTime * this.currentTurnDirection * Mathf.Rad2Deg);

#if UNITY_EDITOR
			if ( this.master.dummyShip == false )
				#endif
			{
				UpdateRotationSegments( this.rotationSegmentsActual, this.currentTurnAmount, this.currentTurnDirection );

				if ( this.currentTurnAmount <= 0.0f )
				{
					this.DisableRotationSegments();
					this.isTurning = false;
				}
			}
		}

		this.UpdateAvoidanceControl();
		   
		this.transform.position += this.transform.forward * this.currentMovementSpeed * Time.deltaTime;
	}

	/// <summary>
	/// Instantiates the rotation display segments around the ship
	/// </summary>
	private void CreateRotationSegments()
	{
		this.rotationSegmentsActual = new GameObject[rotationSegmentCount];
		this.rotationSegmentsTentative = new GameObject[rotationSegmentCount];
		for ( int j = 0; j < 2; ++j ) // 0=actual, 1=tentative
		{
			for ( int i = 0; i < rotationSegmentCount; ++i )
			{
				GameObject segmentObj = GameObject.Instantiate( this.rotationSegmentPrefab ) as GameObject;
				segmentObj.transform.Rotate( Vector3.up, (float)i / (float)rotationSegmentCount * 360.0f );
				segmentObj.transform.parent = this.transform;
				segmentObj.transform.localPosition = Vector3.zero;
				segmentObj.SetActive( false );

				if ( j == 0 )
				{
					this.rotationSegmentsActual[i] = segmentObj;
					segmentObj.transform.localScale = segmentObj.transform.localScale * tentativeSegmentUpscale;
				}
				else
				{
					this.rotationSegmentsTentative[i] = segmentObj;
				}
			}
		}
	}


	private void UpdateAccelerationInput()
	{

		if ( Input.GetKey( KeyCode.W ) )
		{
			this.currentMovementSpeed += Time.deltaTime * moveAcceleration;
		}
		else if ( Input.GetKey( KeyCode.S ) )
		{
			this.currentMovementSpeed -= Time.deltaTime * moveAcceleration;
		}

		this.currentMovementSpeed = Mathf.Clamp( this.currentMovementSpeed, minMoveSpeed, maxMoveSpeed );
	}

	/// <summary>
	/// Updates the turn line using the user's cursor position
	/// </summary>
	private void UpdateTurnLine()
	{
		Vector3 targetPos;
		bool posFound = Common.MousePositionToPlanePoint( out targetPos, Vector3.zero, Vector3.up );

		// If the mouse cursor is over a valid point, then move the target there
		// If not, just use the old position
		if ( posFound == true )
		{
			this.lastSelectedPosition = targetPos;
		}

		Vector3 vectorToTarget = this.lastSelectedPosition - this.transform.position;
		if ( vectorToTarget.sqrMagnitude == 0.0f )
		{
			DebugConsole.Log( Time.frameCount + " Preventing div/0" );
			return;
		}
		Vector3 rayToTarget = vectorToTarget.normalized;

		float angleToTarget = Vector3.Angle( transform.forward, rayToTarget);
		float amountToTurn = Mathf.Abs( angleToTarget ) * Mathf.Deg2Rad;

		// Left = -1.0f Right = 1.0f
		float directionToTurn = Vector3.Dot( this.transform.right, rayToTarget ) > 0 ? 1.0f : -1.0f;

		UpdateRotationSegments( this.rotationSegmentsTentative, amountToTurn, directionToTurn );

		// Set the positions of the line renderer for the tentative path
		Vector3[] pointArray = new Vector3[lineVertexCount];
		GetTravelPathPositions( ref pointArray,
		                       this.transform.position, this.transform.forward,
		                       this.currentMovementSpeed, amountToTurn, directionToTurn );

		this.tentativePathLine.enabled = true;
		for ( int i = 0; i < lineVertexCount; ++i )
		{
			this.tentativePathLine.SetPosition( i, pointArray[i] );
		}

		// If the mouse button is pressed down, set the actual path to the tentative one
		if ( Input.GetMouseButtonDown( 0 ) == true )
		{
			BeginTurn(amountToTurn,directionToTurn);
		}
	}

	private void UpdateAvoidanceControl()
	{
		if ( this.isFollowingAvoidanceCurve )
		{
			float oldPos = this.currentAvoidCurvePoint;
			this.currentAvoidCurvePoint += this.currentMovementSpeed * Time.deltaTime;

			if ( this.currentAvoidCurvePoint >= this.avoidCurveLength )
			{
				this.EndAvoidanceCurve();
			}
			else
			{
				float t1 = this.currentAvoidCurvePoint / this.avoidCurveLength;
				float t2 = oldPos / this.avoidCurveLength;

				//float d1 = Common.SmoothLerp( t1, this.avoidanceCurveStartHeight, this.avoidanceCurveEndHeight );
				//float d2 = Common.SmoothLerp( t2, this.avoidanceCurveStartHeight, this.avoidanceCurveEndHeight );

				float d1 = Common.SmoothPingPong( t1, this.avoidanceCurveStartHeight, this.avoidanceCurveEndHeight, 1 );
				float d2 = Common.SmoothPingPong( t2, this.avoidanceCurveStartHeight, this.avoidanceCurveEndHeight, 1 );

				this.currentAvoidHeight = d1;
				this.currentAvoidAngle = Mathf.Atan2( d2-d1, t1-t2 ) * this.avoidanceAngleMultiplier;
			}
		}
		else
		{
			switch ( this.currentAvoidanceState )
			{
			case AVOIDANCE_STATE.NONE:
			{
				break;
			}
			case AVOIDANCE_STATE.DOWN_RETURN:
			{
				if ( this.currentAvoidHeight >= 0 )
				{
					// Flatten off and finish up
					this.currentAvoidanceState = AVOIDANCE_STATE.NONE;
					this.currentAvoidAngle = 0.0f;
					this.currentAvoidHeight = 0.0f;
					break;
				}
				// If nose is down, bring it back up
				else if ( this.currentAvoidAngle <= 0.0f )
				{
					this.currentAvoidAngle += Time.deltaTime * this.avoidanceAngleRate;
					this.currentAvoidHeight -= this.GetAvoidanceHeightSpeed() * Time.deltaTime;
				}

				if ( this.currentAvoidAngle >= 0.0f )
				{
					this.StartAvoidanceCurve( 0.0f );
				}
				break;
			}
			case AVOIDANCE_STATE.DOWN_START:
			{
				if ( this.currentAvoidHeight == this.avoidanceFloor )
				{
					break;
				}
				// If the nose is pointing up, nose down
				if ( this.currentAvoidAngle < 0.0f )
				{
					this.currentAvoidAngle += Time.deltaTime * this.avoidanceAngleRate;
					this.currentAvoidHeight -= this.GetAvoidanceHeightSpeed() * Time.deltaTime;
				}

				// If the nose is flat, start curving down
				if ( this.currentAvoidAngle >= 0.0f )
				{
					this.StartAvoidanceCurve( this.avoidanceFloor );
				}
				break;
			}
			case AVOIDANCE_STATE.UP_RETURN:
			{
				// If we are back below normal height
				if ( this.currentAvoidHeight <= 0 )
				{
					// Flatten off and finish up
					this.currentAvoidanceState = AVOIDANCE_STATE.NONE;
					this.currentAvoidAngle = 0.0f;
					this.currentAvoidHeight = 0.0f;
					break;
				}
				// If nose is up, bring it back down
				else if ( this.currentAvoidAngle >= 0.0f )
				{
					this.currentAvoidAngle -= Time.deltaTime * this.avoidanceAngleRate;
					this.currentAvoidHeight += this.GetAvoidanceHeightSpeed() * Time.deltaTime;
				}

				if ( this.currentAvoidAngle <= 0.0f )
				{
					this.StartAvoidanceCurve( 0.0f );
				}
				break;
			}
			case AVOIDANCE_STATE.UP_START:
			{
				if ( this.currentAvoidHeight == this.avoidanceCeiling )
				{
					break;
				}
				// If the nose is pointing down, nose up
				if ( this.currentAvoidAngle > 0.0f )
				{
					this.currentAvoidAngle -= Time.deltaTime * this.avoidanceAngleRate;
					this.currentAvoidHeight += this.GetAvoidanceHeightSpeed() * Time.deltaTime;
				}

				// If the nose is flat, start curving up
				if ( this.currentAvoidAngle <= 0.0f )
				{
					this.StartAvoidanceCurve( this.avoidanceCeiling );
				}
				break;
			}
			default:
			{
				DebugConsole.Error( "Uncaught state " + this.currentAvoidanceState, this );
				return;
			}
			}
		}
		this.avoidanceTransform.localPosition = new Vector3( 0.0f, this.currentAvoidHeight, 0.0f );
		//this.avoidanceTransform.localRotation = Quaternion.Euler( this.currentAvoidAngle, 0.0f, 0.0f ); 
	}

	private bool IncreaseAvoidanceAngle()
	{
		if ( this.currentAvoidAngle >= this.avoidanceAngleMax )
		{
			return true;
		}
		this.currentAvoidAngle += Time.deltaTime * this.avoidanceAngleRate;
		return false;
	}

	private bool DecreaseAvoidanceAngle()
	{
		if ( this.currentAvoidAngle <=- this.avoidanceAngleMax )
		{
			return true;
		}
		this.currentAvoidAngle -= Time.deltaTime * this.avoidanceAngleRate;
		return false;
	}

	public void OnAvoidanceAreaEnter( CapitalShipMovement _otherShip )
	{
		this.otherShip = _otherShip;

		if ( this.currentAvoidanceState == AVOIDANCE_STATE.UP_RETURN
		  || this.currentAvoidanceState == AVOIDANCE_STATE.UP_START )
		{
			this.currentAvoidanceState = AVOIDANCE_STATE.UP_START;
		}
		else if ( this.currentAvoidanceState == AVOIDANCE_STATE.DOWN_RETURN
		       || this.currentAvoidanceState == AVOIDANCE_STATE.DOWN_START )
		{
			this.currentAvoidanceState = AVOIDANCE_STATE.DOWN_START;
		}
		else
		{
			// Determine which ship is on the left or right of the intercept line
			Vector3 f1 = this.transform.forward;
			Vector3 f2 = this.otherShip.transform.forward;
			float direction = Vector3.Cross( f1, f2 ).y;

			if ( direction > 0.0f )
			{
				this.currentAvoidanceState = AVOIDANCE_STATE.DOWN_START;
			}
			else
			{
				this.currentAvoidanceState = AVOIDANCE_STATE.UP_START;
			}
		}
	}

	public void OnAvoidanceAreaExit()
	{
		switch ( this.currentAvoidanceState )
		{
		case AVOIDANCE_STATE.NONE:
			break;
		case AVOIDANCE_STATE.DOWN_RETURN:
			break;
		case AVOIDANCE_STATE.UP_RETURN:
			break;
		case AVOIDANCE_STATE.UP_START:
			this.currentAvoidanceState = AVOIDANCE_STATE.UP_RETURN;
			break;
		case AVOIDANCE_STATE.DOWN_START:
			this.currentAvoidanceState = AVOIDANCE_STATE.DOWN_RETURN;
			break;
		default:
			DebugConsole.Error( "Uncaught state " + this.currentAvoidanceState );
			break;
		}
	}

	public void DisableRotationSegments()
	{
		for ( int i = 0; i < rotationSegmentCount; ++i )
		{
			this.rotationSegmentsActual[i].SetActive( false );
		}
	}

	public bool BeginTurn( float _turnAmount, float _direction )
	{
		this.isTurning = true;
		this.currentTurnDirection = _direction;
		this.currentTurnAmount = _turnAmount;
		return true;
	}

	public void GetTravelPathPositions( ref Vector3[] _pointList,
		Vector3 _position, Vector3 _forward,
		float _moveSpeed,
		float _turnAmount, float _turnDirection )
	{
		// Find the radius of the arc used as part of the turn
		float arcRadius = GetTurnRadiusAtSpeed( _moveSpeed );

		// The length of the line so far
		float lineLength = 0.0f;

		// Every point on the line, ecept the last, is placed ina  circle along the travel curve
		// The last point is extended out in a straight line from the end of the curve
		for ( int i = 0; i < lineVertexCount - 1; ++i )
		{
			float angle = _turnAmount / (float)(lineVertexCount - 1) * (float)i;
			Vector3 pos = GetTravelArcPoint( _position, _forward, arcRadius, angle, _turnDirection );
			_pointList[i] = pos;

			if ( i > 0 )
			{
				lineLength += (pos - _pointList[i-1]).magnitude;
			}
		}

		Vector3 lastDir = _pointList[lineVertexCount-2] - _pointList[lineVertexCount-3];

		// If some of the line is still left, push the last point out in a straight line
		if ( lineLength < pathLength )
		{
			lastDir.Normalize();
			_pointList[lineVertexCount-1] = _pointList[lineVertexCount-2] + lastDir * (pathLength-lineLength);
		}
		else
		{
			_pointList[lineVertexCount-1] = _pointList[lineVertexCount-2] + lastDir;
		}
	}

	public void UpdateRotationSegments( GameObject[] _segments, float _turnAmount, float _turnDirection )
	{
		int minActiveSegments = 0, maxActiveSegments = this.rotationSegmentCount;
		int segmentsToActivate = (int)Mathf.Floor( _turnAmount / Mathf.PI * (float)rotationSegmentCount * 0.5f );
		if ( _turnDirection > 0.0f ) // Turning Right
		{
			minActiveSegments = 0;
			maxActiveSegments = segmentsToActivate;
		}
		else // Turning Left
		{
			minActiveSegments = rotationSegmentCount - segmentsToActivate - 1;
			maxActiveSegments = rotationSegmentCount;
		}

		for ( int i = 0; i < this.rotationSegmentCount; ++i )
		{
			_segments[i].SetActive( i >= minActiveSegments && i <= maxActiveSegments );
		}
	}

	public float GetTurnRadiusAtSpeed( float _moveSpeed )
	{
		return _moveSpeed / this.turnRate;
	}

	/// <summary>
	/// Determines whether the specified point is within the boundary circle
	/// </summary>
	/// <returns><c>true</c> if the point lies within the boundary, otherwise <c>false</c>.</returns>
	/// <param name="_pos">The position to test</param>
	public bool IsPointOutOfBounds( Vector3 _pos )
	{
		return (_pos - GameBoundary.instance.origin).magnitude > GameBoundary.instance.radius;
	}

	/// <summary>
	/// Gets the position of a point in the turning circle
	/// </summary>
	/// <returns>The travel arc point.</returns>
	/// <param name="_position">The start position</param>
	/// <param name="_forward">The direction the ship is facing.</param>
	/// <param name="_arcRadius">The radius of the turning circle</param>
	/// <param name="_angle">The angle of the point (0 = ship position)</param>
	/// <param name="_direction">The direction of the turn( -1=left, 1=right )</param>
	public static Vector3 GetTravelArcPoint( Vector3 _position, Vector3 _forward, float _arcRadius, float _angle, float _direction )
	{
		Vector3 perp = Vector3.Cross( _forward, Vector3.up ) * _direction;
		return _position - perp * _arcRadius
			  + Quaternion.AngleAxis( _angle * Mathf.Rad2Deg * _direction, Vector3.up )
				* perp * _arcRadius;
	}

	private float GetAvoidanceHeightSpeed()
	{
		// See sine rule
		float r1 = Mathf.Deg2Rad * this.currentAvoidAngle;
		float r2 = Mathf.Deg2Rad * ( 90 - this.currentAvoidAngle );
		if ( Mathf.Sin(r2) == 0.0f )
		{
			DebugConsole.Error( "bad argument" );
			return 0.0f;
		}
		return this.currentMovementSpeed * Mathf.Sin(r1) / Mathf.Sin(r2);;
	}

	private void StartAvoidanceCurve( float _targetHeight )
	{
		this.currentAvoidCurvePoint = 0.0f;
		this.isFollowingAvoidanceCurve = true;
		this.avoidanceCurveStartHeight = this.currentAvoidHeight;
		this.avoidanceCurveEndHeight = _targetHeight;
		this.avoidCurveLength = Mathf.Abs( this.avoidanceCurveStartHeight - this.avoidanceCurveEndHeight ) / this.avoidanceCurveDropRate;
	}

	private void EndAvoidanceCurve()
	{
		this.isFollowingAvoidanceCurve = false;
		this.currentAvoidHeight = this.avoidanceCurveEndHeight;
		this.currentAvoidAngle = 0.0f;
	}

	public void OnFinaleSequenceStart()
	{
#if UNITY_EDITOR
		if ( this.master.dummyShip == false )
#endif
		{
			GameObject.Destroy( this.tentativePathLine );
			foreach ( GameObject obj in this.rotationSegmentsActual )
			{
				GameObject.Destroy( obj );
			}

			foreach ( GameObject obj in this.rotationSegmentsTentative )
			{
				GameObject.Destroy( obj );
			}
		}
	}
}
