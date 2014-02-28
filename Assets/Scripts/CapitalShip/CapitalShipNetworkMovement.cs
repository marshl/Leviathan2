using UnityEngine;
using System.Collections;

public class CapitalShipNetworkMovement : MonoBehaviour {

	//Movement script that runs on other computers than our own. Liam's going to kill me for this.

	public float turnRate;
	protected float currentTurnAmount = 0.0f;
	protected bool isTurning = false;
	protected float currentTurnDirection = 0.0f;
	protected float currentMovementSpeed = 5.0f;
	//protected float targetMovementSpeed

	public Transform avoidanceTransform;
	private CapitalShipNetworkMovement otherShip;
	public float avoidanceAngleRate;
	public float avoidanceAngleMax;

	private CapitalShipMovement.AVOIDANCE_STATE currentAvoidanceState = CapitalShipMovement.AVOIDANCE_STATE.NONE;
	public float avoidanceCeiling;
	public float avoidanceFloor;
	public float avoidanceCurveDropRate;
	private bool followingAvoidanceCurve;
	private float avoidanceCurveStartHeight;
	private float avoidanceCurveEndHeight;
	private float avoidCurveLength;
	private float currentAvoidCurvePoint;
	
	private float currentAvoidHeight;
	private float currentAvoidAngle;

	// Use this for initialization
	void Start () {

	//	this.avoidanceTransform.localPosition = new Vector3( 0.0f, this.currentAvoidHeight, 0.0f );
	//	this.avoidanceTransform.localRotation = Quaternion.Euler( this.currentAvoidAngle, 0.0f, 0.0f );
		CapitalShipNetworkMovement[] otherScripts = GameObject.FindObjectsOfType<CapitalShipNetworkMovement>() as CapitalShipNetworkMovement[];
		foreach ( CapitalShipNetworkMovement other in otherScripts )
		{
			if ( other != this )
			{
				this.otherShip = other;
			}
		}
	
	}

	private void Awake()
	{
		if ( Network.peerType != NetworkPeerType.Disconnected
		    || this.networkView != null
		    || this.networkView.isMine == true )
		{
			this.enabled = false;
			return;
		}
	}
	
	// Update is called once per frame
	void Update () {

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
			;
			
			if ( this.currentTurnAmount <= 0.0f )
			{
				this.isTurning = false;	
			}	
		}
		
		this.UpdateAvoidanceControl();
		
		this.transform.position += this.transform.forward * this.currentMovementSpeed * Time.deltaTime;
	
	}

	private void UpdateAvoidanceControl()
	{
		if ( this.followingAvoidanceCurve == true )
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
				
				float d1 = Common.SmoothLerp( t1, this.avoidanceCurveStartHeight, this.avoidanceCurveEndHeight );
				float d2 = Common.SmoothLerp( t2, this.avoidanceCurveStartHeight, this.avoidanceCurveEndHeight );
				this.currentAvoidHeight = d1;
				this.currentAvoidAngle = Mathf.Atan2( d2-d1, t1-t2 ) * Mathf.Rad2Deg / 4;
			}
		}
		else
		{
			switch ( this.currentAvoidanceState )
			{
			case CapitalShipMovement.AVOIDANCE_STATE.NONE:
			{
				break;
			}
			case CapitalShipMovement.AVOIDANCE_STATE.DOWN_RETURN:
			{
				if ( this.currentAvoidHeight >= 0 )
				{
					// Flatten off and finish up
					this.currentAvoidanceState = CapitalShipMovement.AVOIDANCE_STATE.NONE;
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
			case CapitalShipMovement.AVOIDANCE_STATE.DOWN_START:
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
			case CapitalShipMovement.AVOIDANCE_STATE.UP_RETURN:
			{
				// If we are back below normal height
				if ( this.currentAvoidHeight <= 0 )
				{
					// Flatten off and finish up
					this.currentAvoidanceState = CapitalShipMovement.AVOIDANCE_STATE.NONE;
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
			case CapitalShipMovement.AVOIDANCE_STATE.UP_START:
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
				Debug.LogError( "Uncaught state " + this.currentAvoidanceState, this );
				return;
			}
			}
		}
		this.avoidanceTransform.localPosition = new Vector3( 0.0f, this.currentAvoidHeight, 0.0f );
		this.avoidanceTransform.localRotation = Quaternion.Euler( this.currentAvoidAngle, 0.0f, 0.0f );
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
	
	public void OnAvoidanceAreaEnter( CapitalShipNetworkMovement _otherShip )
	{
		this.otherShip = _otherShip;
		
		if ( this.currentAvoidanceState == CapitalShipMovement.AVOIDANCE_STATE.UP_RETURN
		    || this.currentAvoidanceState == CapitalShipMovement.AVOIDANCE_STATE.UP_START )
		{
			this.currentAvoidanceState = CapitalShipMovement.AVOIDANCE_STATE.UP_START;
		}
		else if ( this.currentAvoidanceState == CapitalShipMovement.AVOIDANCE_STATE.DOWN_RETURN
		         || this.currentAvoidanceState == CapitalShipMovement.AVOIDANCE_STATE.DOWN_START )
		{
			this.currentAvoidanceState = CapitalShipMovement.AVOIDANCE_STATE.DOWN_START;
		}
		else
			//if ( this.currentAvoidanceState != AVOIDANCE_STATE.DOWN_START
			//  && this.currentAvoidanceState != AVOIDANCE_STATE.UP_START )
		{
			// Determine which ship is on the left or right of the intercept line
			
			Vector3 f1 = this.transform.forward;
			Vector3 f2 = this.otherShip.transform.forward;
			float direction = Vector3.Cross( f1, f2 ).y;
			
			if ( direction > 0.0f )
			{
				this.currentAvoidanceState = CapitalShipMovement.AVOIDANCE_STATE.DOWN_START;
			}
			else
			{
				this.currentAvoidanceState = CapitalShipMovement.AVOIDANCE_STATE.UP_START;
			}
		}
	}
	
	public void OnAvoidanceAreaExit()
	{
		switch ( this.currentAvoidanceState )
		{
		case CapitalShipMovement.AVOIDANCE_STATE.NONE:
			break;
		case CapitalShipMovement.AVOIDANCE_STATE.DOWN_RETURN:
			break;
		case CapitalShipMovement.AVOIDANCE_STATE.UP_RETURN:
			break;
		case CapitalShipMovement.AVOIDANCE_STATE.UP_START:
			this.currentAvoidanceState = CapitalShipMovement.AVOIDANCE_STATE.UP_RETURN;
			break;
		case CapitalShipMovement.AVOIDANCE_STATE.DOWN_START:
			this.currentAvoidanceState = CapitalShipMovement.AVOIDANCE_STATE.DOWN_RETURN;
			break;
		default:
			Debug.LogError( "Uncaught state " + this.currentAvoidanceState );
			break;
		}
	}

	private float GetAvoidanceHeightSpeed()
	{
		// See sine rule
		float r1 = Mathf.Deg2Rad * this.currentAvoidAngle;
		float r2 = Mathf.Deg2Rad * ( 90 - this.currentAvoidAngle );
		if ( Mathf.Sin(r2) == 0.0f )
		{
			Debug.LogError( "bad argument" );
			return 0.0f;
		}
		return this.currentMovementSpeed * Mathf.Sin(r1) / Mathf.Sin(r2);;
	}

	private void StartAvoidanceCurve( float _targetHeight )
	{
		Debug.Log( "Starting curve" );
		this.currentAvoidCurvePoint = 0.0f;
		this.followingAvoidanceCurve = true;
		this.avoidanceCurveStartHeight = this.currentAvoidHeight;
		this.avoidanceCurveEndHeight = _targetHeight;
		this.avoidCurveLength = Mathf.Abs( this.avoidanceCurveStartHeight - this.avoidanceCurveEndHeight ) / this.avoidanceCurveDropRate;
	}
	
	private void EndAvoidanceCurve()
	{
		Debug.Log( "Ending avoidance curve" );
		this.followingAvoidanceCurve = false;
		this.currentAvoidHeight = this.avoidanceCurveEndHeight;
		this.currentAvoidAngle = 0.0f;
	} 

	public bool BeginTurn( float _turnAmount, float _direction )
	{
		this.isTurning = true;
		this.currentTurnDirection = _direction;
		this.currentTurnAmount = _turnAmount;
		return true; 
	}
	public bool IsPointOutOfBounds( Vector3 _pos )
	{
		return (_pos - GameBoundary.instance.origin).magnitude > GameBoundary.instance.radius;
	}
	public float GetTurnRadiusAtSpeed( float _moveSpeed )
	{
		return _moveSpeed / this.turnRate;
	}

	//This is called via networking.

	public void StartNewTurnWithInfo(float amount, bool turning, float direction, Vector3 start, Quaternion rotation)
	{
		this.currentTurnDirection = direction;
		
		this.currentTurnAmount = amount;
		this.isTurning = turning;
		this.transform.position = start;
		this.transform.rotation = rotation;
	}
}
