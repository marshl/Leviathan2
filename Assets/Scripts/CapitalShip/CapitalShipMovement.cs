using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CapitalShipMovement : MonoBehaviour
{
	/// <summary>
	/// The transform that is used to mark where the mouse currently is
	/// </summary>
	public Transform markerTransform;

	/// <summary>
	/// The prefab used to mark the turning angle around the ship
	/// </summary>
	public GameObject rotationSegmentPrefab;

	/// <summary>
	/// The line used to show where the ship will travel
	/// </summary>
	public LineRenderer tentativePathLine;

	/// <summary>
	/// The number of triangles displayed around the ship in a full circle
	/// </summary>
	public int rotationSegmentCount; 

	/// <summary>
	/// The increase in scale for the tentative rotation segments
	///    (the segments that many degrees the ship would turn if the user clicked)
	/// </summary>
	public float tentativeSegmentUpscale;

	/// <summary>
	/// The radians that ship still has to turn
	/// </summary>
	private float currentTurnAmount = 0.0f;

	/// <summary>
	/// Whether the ship is currently turning
	/// </summary>
	private bool isTurning = false;

	/// <summary>
	/// The direction that the ship is currently turning (-1.0f = left, 1.0f = right)
	/// </summary>
	private float currentTurnDirection = 0.0f;

	/// <summary>
	/// The last position that the player selected
	/// </summary>
	private Vector3 lastTargetPosition;

	/// <summary>
	/// The rotational segments that the show where the ship is turning
	/// </summary>
	private GameObject[] rotationSegmentsActual;
	/// <summary>
	/// The rotational segments that shows where the ship would turn
	/// </summary>
	private GameObject[] rotationSegmentsTentative;

	/// <summary>
	/// The origin point of the positional boundary
	/// </summary>
	public Vector3 boundaryOrigin;
	/// <summary>
	/// The radius of the boundary circle
	/// </summary>
	public float boundaryRadius;

	/// <summary>
	/// The speed at which the ship is currently moving
	/// </summary>
	private float currentMovementSpeed = 5.0f;


	/// <summary>
	/// The vertex count of the path lines
	/// </summary>
	public int lineVertexCount;

	/// <summary>
	/// The maximum length of the display path
	/// </summary>
	public float pathLength;

	/// <summary>
	/// The radians that the ship turns per second
	/// </summary>
	public float turnRate;

	/// <summary>
	/// The maximum speed that the ship can move
	/// </summary>
	public float maxMoveSpeed;

	/// <summary>
	/// The minimum speed that the ship can move
	/// </summary>
	public float minMoveSpeed;

	/// <summary>
	/// The rate of change in speed per second
	/// </summary>
	public float moveAcceleration;

	private void Start()
	{
		this.tentativePathLine.SetVertexCount( lineVertexCount );

		this.CreateRotationSegments();

		GameObject obj = new GameObject();
		LineRenderer line = obj.AddComponent<LineRenderer>();
		line.SetVertexCount( 100 );
		for ( int i = 0; i < 100; ++i )
		{
			float angle = (float)i * Mathf.PI * 2.0f / 100.0f;
			Vector3 offset = new Vector3( Mathf.Sin( angle ),  0.0f, Mathf.Cos( angle ) );
			line.SetPosition( i, offset * boundaryRadius + boundaryOrigin );
		}
	}
	
	private void Update()
	{
		this.UpdateAccelerationInput();
		this.UpdateTurnLine();

		// If the current position + the radius of a turn would place this ship out of bounds,
		//   start an emergency turn
		if ( IsPointOutOfBounds(this.transform.position + this.transform.forward * GetTurnRadiusAtSpeed( this.currentMovementSpeed )) )
		{
			float angle = Vector3.Dot( transform.right, (boundaryOrigin - this.transform.position).normalized );
			float direction = angle > 0.0f ? 1.0f : -1.0f;
			
			Debug.Log( "OUT OF RANGE: " + Time.frameCount + " Turn " + (direction > 0.0f ? "right" : "left") );
			this.BeginTurn( angle, direction );
		}

		if ( this.isTurning == true )
		{
			this.currentTurnAmount -= Time.deltaTime * turnRate;
			this.transform.Rotate( Vector3.up, turnRate * Time.deltaTime * this.currentTurnDirection * Mathf.Rad2Deg);

			UpdateRotationSegments( this.rotationSegmentsActual, this.currentTurnAmount, this.currentTurnDirection );
			
			if ( this.currentTurnAmount <= 0.0f )
			{
				this.DisableRotationSegments();
				this.isTurning = false;	
			}	
		}
	


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
			this.lastTargetPosition = targetPos;
			this.markerTransform.position = this.lastTargetPosition;
		}
		
		Vector3 vectorToTarget = this.lastTargetPosition - this.transform.position;
		if ( vectorToTarget.sqrMagnitude == 0.0f )
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
			this.currentTurnDirection = directionToTurn;
			
			this.currentTurnAmount = amountToTurn;
			this.isTurning = true;
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
		if ( _moveSpeed == 0.0f )
		{
			Debug.Log ( "Cannot crete travel path with zero movement speed." );;
			return;
		}

		// If the ship isn't turning, then draw a straight line out in front
		if ( _turnAmount == 0.0f )
		{
			for ( int i = 0; i < this.lineVertexCount; ++i )
			{
				_pointList[i] = _position + _forward * (float)i * this.pathLength / (float)lineVertexCount;
			}
			return;
		}

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

				if ( lineLength > pathLength )
				{
					Debug.LogError( "Not enough path length to cover curve. Need at least " + arcRadius * Mathf.PI );
				}
			}
		}

		Vector3 lastDir = _pointList[lineVertexCount-2] - _pointList[lineVertexCount-3];

		// If some of the line is still left, push the last point out in a straight line
		if ( lineLength < pathLength ) 
		{
			lastDir.Normalize();
			_pointList[lineVertexCount-1] = _pointList[lineVertexCount-2] + lastDir * (pathLength-lineLength);
		}
		else //Otherwise 
		{
			_pointList[lineVertexCount-1] = _pointList[lineVertexCount-2] + lastDir;
		}
	}
	
	public void UpdateRotationSegments( GameObject[] _segments, float _turnAmount, float _turnDirection )
	{
		int minActiveSegments = 0, maxActiveSegments = this.rotationSegmentCount;
		int segmentsToActivate =  (int)Mathf.Floor( _turnAmount / Mathf.PI * (float)rotationSegmentCount * 0.5f );
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
		
		for ( int i = 0; i < rotationSegmentCount; ++i )
		{
			_segments[i].SetActive( i >= minActiveSegments && i <= maxActiveSegments );	
		}
	}
	
	public float GetTurnRadiusAtSpeed( float _moveSpeed )
	{
		return _moveSpeed / this.turnRate;
	}

	public bool IsPointOutOfBounds( Vector3 _pos )
	{
		return (_pos - boundaryOrigin).magnitude > boundaryRadius;
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
		                   + Quaternion.AngleAxis( _angle * Mathf.Rad2Deg * _direction, Vector3.up ) * perp * _arcRadius;
	}
}
