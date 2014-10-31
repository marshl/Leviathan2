/// <summary>
/// Player instantiator.
/// 
/// "Is it done Yuri?"
/// "No comrade Premier, it has only begun"
/// </summary>


using UnityEngine;
using System.Collections;

public class PlayerInstantiator : MonoBehaviour
{
	public static PlayerInstantiator instance;

	public GameObject speedFighterPrefab;
	public GameObject agileFighterPrefab;
	public GameObject heavyFighterPrefab;
	public GameObject fighterCameraPrefab;
	public GameObject capitalShipPrefab;
	public GameObject capitalPathLinePrefab;
	public GameObject capitalCameraPrefab;

	private void Awake()
	{
		PlayerInstantiator.instance = this;
	}

	public GameObject CreatePlayerObject( GamePlayer _player, bool _dummyPlayer )
	{
		switch ( _player.playerType )
		{
		case PLAYER_TYPE.COMMANDER1:
		case PLAYER_TYPE.COMMANDER2:
		{
			return this.CreateCapitalShip( _player, _dummyPlayer);
		}
		case PLAYER_TYPE.FIGHTER1:
		case PLAYER_TYPE.FIGHTER2:
		{
			return this.CreateFighter( _player, _dummyPlayer );
		}
		default:
		{
			DebugConsole.Error( "Uncaught player type " + _player.playerType );
			return null;
		}
		}
	}
 
	private GameObject CreateCapitalShip( GamePlayer _player, bool _dummyShip )
	{   
		Vector3 origin = GameBoundary.instance.transform.position;
		Vector3 offset = new Vector3( GameBoundary.instance.radius, 0.0f, 0.0f );
		Vector3 shipPos = _player.team == TEAM.TEAM_1 ? 
			origin + offset : origin - offset;

		Quaternion shipRot = Quaternion.LookRotation( _player.team == TEAM.TEAM_1
			? new Vector3( -1, 0, 0 ) : new Vector3( 1, 0, 0 ) ); 

		//Vector3 shipPos = _player.team == TEAM.TEAM_1 ? new Vector3( -500.0f, 0.0f, 0.0f ) : new Vector3( 500.0f, 0.0f, 0.0f );
		GameObject capitalObj;
#if UNITY_EDITOR
		// Used for testing scenes
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			capitalObj = GameObject.Instantiate(
				this.capitalShipPrefab,
				shipPos,
				shipRot ) as GameObject;
		}
		else // Normal operation
#endif
		{
			capitalObj = Network.Instantiate(
				this.capitalShipPrefab,
				shipPos,
				shipRot,
				0 ) as GameObject;
		}

		CapitalShipMaster masterScript = capitalObj.GetComponent<CapitalShipMaster>();
		CapitalShipMovement movementScript = masterScript.movement;

#if UNITY_EDITOR
		masterScript.isDummyShip = _dummyShip;

		masterScript.ownerControl.ownerID = _player.id;

		if ( masterScript.isDummyShip == false )
#endif
		{
			GameObject lineObj = GameObject.Instantiate( this.capitalPathLinePrefab, Vector3.zero, Quaternion.identity ) as GameObject;
			LineRenderer lineScript = lineObj.GetComponent<LineRenderer>();
			movementScript.tentativePathLine = lineScript;

			GameObject cameraObj = GameObject.Instantiate(
				this.capitalCameraPrefab,
				Vector3.zero,
				Quaternion.identity ) as GameObject;

			CapitalShipCamera cameraScript = cameraObj.GetComponent<CapitalShipCamera>();
			cameraScript.masterScript = masterScript;
			masterScript.capitalCamera = cameraScript;
		}
		DebugConsole.Log( "Creating capital ship for player " + _player.id + " on team " + _player.team, capitalObj );

		return capitalObj;
	}

	private GameObject CreateFighter( GamePlayer _player, bool _dummyShip )
	{
		Vector3 fighterPos = Common.RandomVector3( -25.0f, 25.0f );

		GameObject fighterPrefab = this.GetFighterPrefab( _player.fighterType );

		GameObject fighterObj;
		// Used for testing scenes
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
#if UNITY_EDITOR
			fighterPrefab.GetComponent<FighterMaster>().dummyShip = _dummyShip;
			fighterPrefab.GetComponent<FighterMaster>().ownerID = _player.id;
#endif
			fighterObj = GameObject.Instantiate( 
			    fighterPrefab, 
			    fighterPos, 
			    Quaternion.identity ) as GameObject;
		}
		else
		{
			fighterObj = Network.Instantiate(
				fighterPrefab,
				fighterPos,
				Quaternion.identity,
				0 ) as GameObject;
		}

		FighterMaster fighterScript = fighterObj.GetComponent<FighterMaster>();

		if ( !_dummyShip )
		{
			GameObject cameraObj = GameObject.Instantiate(
				this.fighterCameraPrefab,
				fighterObj.transform.position,
				Quaternion.identity ) as GameObject;

			FighterCamera cameraScript = cameraObj.GetComponent<FighterCamera>();
			cameraScript.fighter = fighterScript;
			fighterScript.fighterCamera = cameraScript;
		}
		DebugConsole.Log( "Creating fighter for player " + _player.id + " on team " + _player.team, fighterObj );

		return fighterObj;
	}

	private GameObject GetFighterPrefab( FIGHTER_TYPE _fighterType )
	{
		switch ( _fighterType )
		{
		case FIGHTER_TYPE.AGILE:
			return this.agileFighterPrefab;
		case FIGHTER_TYPE.HEAVY:
			return this.heavyFighterPrefab;
		case FIGHTER_TYPE.SPEED:
			return this.speedFighterPrefab;
		default:
			DebugConsole.Error( "Uncaught fighter type " + _fighterType );
			return null;
		}
	}
}
