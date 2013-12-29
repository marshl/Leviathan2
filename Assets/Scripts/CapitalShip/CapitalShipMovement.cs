using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CapitalShipMovement : MonoBehaviour
{
	// Editor Variables
	public Transform markerTransform;
	
	public GameObject rotationSegmentPrefab;
	
	public LineRenderer rotationLine;
	public LineRenderer tentativePathLine;
	public LineRenderer actualPathLine;

	public Transform rollSection;

	/// <summary>
	/// The number of triangles displayed around the ship in a full circle
	/// </summary>
	private const int ROTATION_SEGMENT_COUNT = 16; 
	private const float TENTATIVE_SEGMENT_UPSCALE = 1.5f;
	
	private float currentTurnAmount = 0.0f;
	private bool isTurning = false;
	private float currentTurnDirection = 0.0f;
	
	private Vector3 lastTargetPosition;
	
	private GameObject[] rotationSegmentArrayActual;
	private GameObject[] rotationSegmentsTentative;
	
	private Vector3 boundaryOrigin = new Vector3( 0.0f, 0.0f, 0.0f );
	private const float BOUNDARY_DISTANCE = 50.0f;
	private float currentMovementSpeed = 5.0f;

	// Constants
	private const int VERTEX_COUNT = 25;
	private const float MAX_PATH_DISTANCE = 50.0f;
	
	private const float MAXIMUM_ANGLE_PER_METRE = 5.0f;
	private const float TURN_SPEED = 0.5f; 
	private const float MOVEMENT_SPEED_MAX = 7.5f;
	private const float MOVEMENT_SPEED_MIN = 1.0f;
	private const float MOVEMENT_ACCELERATION = 3.0f;

	private const float ROLL_MAXIMUM = Mathf.PI / 3;
	private const float ROLL_RATE = 10.0f;
	
	private void Start()
	{
		this.tentativePathLine.SetVertexCount( VERTEX_COUNT );
		this.actualPathLine.SetVertexCount( VERTEX_COUNT );

		this.CreateRotationSegments();

		GameObject obj = new GameObject();
		LineRenderer line = obj.AddComponent<LineRenderer>();
		line.SetVertexCount( 100 );
		for ( int i = 0; i < 100; ++i )
		{
			float angle = (float)i * Mathf.PI * 2.0f / 100.0f;
			Vector3 offset = new Vector3( Mathf.Sin( angle ),  0.0f, Mathf.Cos( angle ) );
			line.SetPosition( i, offset * BOUNDARY_DISTANCE + boundaryOrigin );
		}
	}
	
	private void Update()
	{
		this.UpdateAccelerationInput();

		this.UpdateTurnLine();

		if ( IsPointOutOfBounds(this.transform.position + this.transform.forward * GetTurnRadiusAtSpeed( this.currentMovementSpeed )) )
		{
			float angle = Vector3.Dot( transform.right, (boundaryOrigin - this.transform.position).normalized );
			float direction = angle > 0.0f ? 1.0f : -1.0f;
			
			Debug.Log( "OUT OF RANGE: " + Time.frameCount + " Turn " + (direction > 0.0f ? "right" : "left") );
			this.BeginTurn( angle, direction );
		}

		if ( this.isTurning == true )
		{
			this.currentTurnAmount -= Time.deltaTime * TURN_SPEED;
			this.transform.Rotate( Vector3.up, TURN_SPEED * Time.deltaTime * this.currentTurnDirection * Mathf.Rad2Deg);

			UpdateRotationSegments( this.rotationSegmentArrayActual, this.currentTurnAmount, this.currentTurnDirection );
			
			if ( this.currentTurnAmount <= 0.0f )
			{
				this.DisableRotationSegments();
				this.isTurning = false;	
			}	
		}

		this.UpdateRoll();
	
		if ( Input.GetKeyDown( KeyCode.Escape ) )
			Debug.Break();

		this.transform.position += this.transform.forward * this.currentMovementSpeed * Time.deltaTime;
	}

	private void CreateRotationSegments()
	{
		this.rotationSegmentArrayActual = new GameObject[ROTATION_SEGMENT_COUNT];
		this.rotationSegmentsTentative = new GameObject[ROTATION_SEGMENT_COUNT];
		for ( int j = 0; j < 2; ++j )
		{
			for ( int i = 0; i < ROTATION_SEGMENT_COUNT; ++i )
			{
				GameObject segmentObj = GameObject.Instantiate( this.rotationSegmentPrefab ) as GameObject;	
				segmentObj.transform.Rotate( Vector3.up, (float)i / (float)ROTATION_SEGMENT_COUNT * 360.0f );
				segmentObj.transform.parent = this.transform;
				segmentObj.transform.localPosition = Vector3.zero;
				segmentObj.SetActive( false );
				
				if ( j == 0 )
				{
					this.rotationSegmentArrayActual[i] = segmentObj;
					segmentObj.transform.localScale = segmentObj.transform.localScale * TENTATIVE_SEGMENT_UPSCALE;
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
			this.currentMovementSpeed += Time.deltaTime * MOVEMENT_ACCELERATION;	
		}
		else if ( Input.GetKey( KeyCode.S ) )
		{
			this.currentMovementSpeed -= Time.deltaTime * MOVEMENT_ACCELERATION;	
		}
		this.currentMovementSpeed = Mathf.Clamp( this.currentMovementSpeed, MOVEMENT_SPEED_MIN, MOVEMENT_SPEED_MAX );
	}

	private void UpdateTurnLine()
	{
		Vector3 targetPos;
		bool posFound = Common.MousePositionToPlanePoint( out targetPos, Vector3.zero, Vector3.up );
		
		if ( posFound == true )
		{
			this.lastTargetPosition = targetPos;
			this.markerTransform.position = this.lastTargetPosition;
		}
		
		Vector3 vectorToTarget = this.lastTargetPosition - this.transform.position;
		if ( vectorToTarget.magnitude == 0.0f )
		{
			Debug.Log( "Preventing div/0" );
			return;
		}
		Vector3 rayToTarget = vectorToTarget.normalized;
		
		float angleToTarget = Vector3.Angle( transform.forward, rayToTarget);
		float amountToTurn = Mathf.Abs( angleToTarget ) * Mathf.Deg2Rad;

		// Left = -1.0f Right = 1.0f 
		float directionToTurn = Vector3.Dot( this.transform.right, rayToTarget ) > 0 ? 1.0f : -1.0f;
		
		UpdateRotationSegments( this.rotationSegmentsTentative, amountToTurn, directionToTurn );

		// Set the positions of the line renderer for teh tentative path

		Vector3[] pointArray = new Vector3[VERTEX_COUNT];
		GetTravelPathPositions( ref pointArray, 
		                       this.transform.position, this.transform.forward, 
		                       this.currentMovementSpeed, amountToTurn, directionToTurn );
		
		this.tentativePathLine.enabled = true;
		for ( int i = 0; i < VERTEX_COUNT; ++i )
		{
			this.tentativePathLine.SetPosition( i, pointArray[i] );	
		}

		// If the mouse button is pressed down, set the actual path to the tentative one
		if ( Input.GetMouseButtonDown( 0 ) == true )
		{
			this.currentTurnDirection = directionToTurn;
			
			this.currentTurnAmount = amountToTurn;
			this.isTurning = true;
		}
	}

	private void UpdateRoll()
	{
		float roll = this.rollSection.localRotation.z;
		if ( this.isTurning == true )
		{
			roll = Mathf.Lerp ( roll, ROLL_MAXIMUM * this.currentTurnDirection, ROLL_RATE * Time.deltaTime );
		}
		else //if ( this.rollSection.localRotation.z > ROLL_RATE * Time.deltaTime ) 
		{
			roll = Mathf.Lerp ( roll, 0.0f, ROLL_RATE * Time.deltaTime );
		}
		this.rollSection.Rotate ( this.rollSection.forward, roll - this.rollSection.localRotation.z, Space.World );
	}
	
	public void DisableRotationSegments()
	{
		for ( int i = 0; i < ROTATION_SEGMENT_COUNT; ++i )
		{
			this.rotationSegmentArrayActual[i].SetActive( false );	
		}	
	}

	public bool BeginTurn( float _turnAmount, float _direction )
	{
		this.isTurning = true;
		this.currentTurnDirection = _direction;
		this.currentTurnAmount = _turnAmount;
		return true; 
	}
	
	public static void GetTravelPathPositions( ref Vector3[] _pointList,
		Vector3 _position, Vector3 _forward,
		float _moveSpeed,
		float _turnAmount, float _turnDirection )
	{
		if ( _moveSpeed == 0.0f )
		{
			Debug.Log ( "Cannot crete travel path with zero movement speed." );;
			return;
		}

		// If the ship isn't turning, then draw a straight line out in front
		if ( _turnAmount == 0.0f )
		{
			for ( int i = 0; i < VERTEX_COUNT; ++i )
			{
				_pointList[i] = _position + _forward * (float)i * MAX_PATH_DISTANCE / (float)VERTEX_COUNT;
			}
			return;
		}

		// Find the radius of the arc used as part of the turn
		float arcRadius = GetTurnRadiusAtSpeed( _moveSpeed );

		// The length of the line so far
		float lineLength = 0.0f;

		// Every point on the line, ecept the last, is placed ina  circle along the travel curve
		// The last point is extended out in a straight line from the end of the curve
		for ( int i = 0; i < VERTEX_COUNT - 1; ++i )
		{
			float angle = _turnAmount / (float)(VERTEX_COUNT - 1) * (float)i;
			Vector3 pos = GetTravelArcPoint( _position, _forward, arcRadius, angle, _turnDirection );
			_pointList[i] = pos;

			if ( i > 0 )
			{
				lineLength += (pos - _pointList[i-1]).magnitude;

				if ( lineLength > MAX_PATH_DISTANCE )
				{
					Debug.LogError( "Not enough path length to cover curve. Need at least " + arcRadius * Mathf.PI );
				}
			}
		}

		Vector3 lastDir = _pointList[VERTEX_COUNT-2] - _pointList[VERTEX_COUNT-3];

		// If some of the line is still left, push the last point out in a straight line
		if ( lineLength < MAX_PATH_DISTANCE ) 
		{
			lastDir.Normalize();
			_pointList[VERTEX_COUNT-1] = _pointList[VERTEX_COUNT-2] + lastDir * (MAX_PATH_DISTANCE-lineLength);
		}
		else //Otherwise 
		{
			_pointList[VERTEX_COUNT-1] = _pointList[VERTEX_COUNT-2] + lastDir;
		}
	}
	
	public static void UpdateRotationSegments( GameObject[] _segments, float _turnAmount, float _turnDirection )
	{
		int minActiveSegments = 0, maxActiveSegments = ROTATION_SEGMENT_COUNT;
		int segmentsToActivate =  (int)Mathf.Floor( _turnAmount / Mathf.PI * (float)ROTATION_SEGMENT_COUNT * 0.5f );
		if ( _turnDirection > 0.0f ) // Turning Right
		{
			minActiveSegments = 0;	
			maxActiveSegments = segmentsToActivate;
		}
		else // Turning Left
		{
			minActiveSegments = ROTATION_SEGMENT_COUNT - segmentsToActivate - 1;
			maxActiveSegments = ROTATION_SEGMENT_COUNT;	
		}
		
		for ( int i = 0; i < ROTATION_SEGMENT_COUNT; ++i )
		{
			_segments[i].SetActive( i >= minActiveSegments && i <= maxActiveSegments );	
		}
	}
	
	public static float GetTurnRadiusAtSpeed( float _moveSpeed )
	{
		return _moveSpeed / TURN_SPEED;
	}

	public bool IsPointOutOfBounds( Vector3 _pos )
	{
		return (_pos - boundaryOrigin).magnitude > BOUNDARY_DISTANCE;
	}

	public static Vector3 GetTravelArcPoint( Vector3 _position, Vector3 _forward, float _arcRadius, float _angle, float _direction )
	{
		Vector3 perp = Vector3.Cross( _forward, Vector3.up ) * _direction;
		return _position - perp * _arcRadius
		                   + Quaternion.AngleAxis( _angle * Mathf.Rad2Deg * _direction, Vector3.up ) * perp * _arcRadius;
	}
}
