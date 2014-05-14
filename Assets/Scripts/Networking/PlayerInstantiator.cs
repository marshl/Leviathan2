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
	public bool overrideSelection;

	private void Awake()
	{
		PlayerInstantiator.instance = this;
	}

	public GameObject CreatePlayerObject( GamePlayer _player )
	{
		switch ( _player.playerType )
		{
		case PLAYER_TYPE.COMMANDER1:
		{
			if ( this.testingFighters && this.overrideSelection )
			{
				return this.CreateFighter( _player );
		    }
			return this.CreateCapitalShip( _player );
		}
		case PLAYER_TYPE.COMMANDER2:
		{
			if ( this.testingFighters && this.overrideSelection )
			{
				return this.CreateFighter( _player );
			}
			return this.CreateCapitalShip( _player );
		}
		case PLAYER_TYPE.FIGHTER1:
		{
			return this.CreateFighter( _player );
		}
		case PLAYER_TYPE.FIGHTER2:
		{
			return this.CreateFighter( _player );
		}
		default:
		{
			Debug.LogError( "Uncaught player type " + _player.playerType );
			return null;
		}
		}
	}
 
	private GameObject CreateCapitalShip( GamePlayer _player )
	{   
		Vector3 shipPos = _player.team == TEAM.TEAM_1 ? new Vector3( -500.0f, 0.0f, 0.0f ) : new Vector3( 500.0f, 0.0f, 0.0f );
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
		Debug.Log( "Creating capital ship for player " + _player.id + " on team " + _player.team, capitalObj );
		 
		return capitalObj;
	}

	private GameObject CreateFighter( GamePlayer _player )
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

		//_player.fighter = fighterObj.GetComponent<Fighter>();

		FighterCamera cameraScript = cameraObj.GetComponent<FighterCamera>();
		cameraScript.fighter = fighterObj;

		Debug.Log( "Creating fighter for player " + _player.id + " on team " + _player.team, fighterObj );

		return fighterObj;
	}
}
