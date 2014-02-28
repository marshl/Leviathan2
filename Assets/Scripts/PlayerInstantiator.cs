using UnityEngine;
using System.Collections;

public class PlayerInstantiator : MonoBehaviour
{
	public static PlayerInstantiator instance;

	public GameObject fighterPrefab;
	public GameObject fighterCameraPrefab;
	public GameObject capitalShipPrefab;
	public GameObject capitalPathLinePrefab;
	public GameObject capitalCameraPrefab;

	public bool testingFighters;

	private void Awake()
	{
		PlayerInstantiator.instance = this;
	}

	public GameObject CreatePlayerObject( PLAYER_TYPE _playerType )
	{
		switch ( _playerType )
		{
			case PLAYER_TYPE.COMMANDER1:
			{
				if ( this.testingFighters )
				{
				    return this.CreateFighter( 1 );
			    }
				return this.CreateCapitalShip( 1 );
			}
			case PLAYER_TYPE.COMMANDER2:
			{
				if ( this.testingFighters )
				{
					return this.CreateFighter( 2 );
				}
				return this.CreateCapitalShip( 2 );
			}
			case PLAYER_TYPE.FIGHTER1:
			{
				return this.CreateFighter( 1 );
			}
			case PLAYER_TYPE.FIGHTER2:
			{
				return this.CreateFighter( 2 );
			}
			default:
			{
				Debug.LogError( "Uncaught player type " + _playerType );
				return null;
			}
		}
	}

	private GameObject CreateCapitalShip( int _teamID )
	{
		Vector3 shipPos = _teamID == 1 ? new Vector3( -10.0f, 0.0f, 0.0f ) : new Vector3( 10.0f, 0.0f, 0.0f );
		GameObject capitalObj;
		// Used for testing scenes
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			capitalObj = GameObject.Instantiate(
				this.capitalShipPrefab,
				shipPos,
				Quaternion.identity ) as GameObject;
		}
		else // Normal operation
		{
			capitalObj = Network.Instantiate(
				this.capitalShipPrefab,
				shipPos,
				Quaternion.identity,
				0 ) as GameObject;
		}

		CapitalShipMovement movementScript = capitalObj.GetComponent<CapitalShipMovement>();

		GameObject lineObj = GameObject.Instantiate( this.capitalPathLinePrefab, Vector3.zero, Quaternion.identity ) as GameObject;
		LineRenderer lineScript = lineObj.GetComponent<LineRenderer>();
		movementScript.tentativePathLine = lineScript;

		GameObject cameraObj = GameObject.Instantiate(
			this.capitalCameraPrefab,
			Vector3.zero,
			Quaternion.identity ) as GameObject;

		CapitalShipCamera cameraScript = cameraObj.GetComponent<CapitalShipCamera>();
		cameraScript.targetTransform = capitalObj.transform;

		capitalObj.GetComponent<TargettableObject>().teamID = _teamID;

		return capitalObj;
	}

	private GameObject CreateFighter( int _teamID )
	{
		Vector3 fighterPos = Common.RandomVector3( -25.0f, 25.0f );
		GameObject fighterObj;
		// Used for testing scenes
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			fighterObj = GameObject.Instantiate( 
			    this.fighterPrefab, 
			    fighterPos, 
			    Quaternion.identity ) as GameObject;
		}
		else
		{
			fighterObj = Network.Instantiate(
				this.fighterPrefab,
				fighterPos,
				Quaternion.identity,
				0 ) as GameObject;
		}

		GameObject cameraObj = GameObject.Instantiate(
			this.fighterCameraPrefab,
			fighterObj.transform.position,
			Quaternion.identity ) as GameObject;

		FighterCamera cameraScript = cameraObj.GetComponent<FighterCamera>();
		cameraScript.fighter = fighterObj; 
		return fighterObj;
	}
}
