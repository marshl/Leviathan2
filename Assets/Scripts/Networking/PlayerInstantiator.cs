﻿using UnityEngine;
using System.Collections;

public class PlayerInstantiator : MonoBehaviour
{
	public static PlayerInstantiator instance;

	public GameObject fighterPrefab;
	public GameObject fighterCameraPrefab;
	public GameObject capitalShipPrefab;
	public GameObject capitalPathLinePrefab;
	public GameObject capitalCameraPrefab;

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
			return this.CreateCapitalShip( _player );
		}
		case PLAYER_TYPE.COMMANDER2:
		{
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
			DebugConsole.Error( "Uncaught player type " + _player.playerType );
			return null;
		}
		}
	}
 
	private GameObject CreateCapitalShip( GamePlayer _player )
	{   
		Vector3 origin = GameBoundary.instance.transform.position;
		Vector3 offset = new Vector3( GameBoundary.instance.radius, 0.0f, 0.0f );
		Vector3 shipPos = _player.team == TEAM.TEAM_1 ? 
			origin + offset : origin - offset;

		Quaternion shipRot = Quaternion.LookRotation( _player.team == TEAM.TEAM_1
			? new Vector3( -1, 0, 0 ) : new Vector3( 1, 0, 0 ) ); 

		//Vector3 shipPos = _player.team == TEAM.TEAM_1 ? new Vector3( -500.0f, 0.0f, 0.0f ) : new Vector3( 500.0f, 0.0f, 0.0f );
		GameObject capitalObj;
		// Used for testing scenes
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			capitalObj = GameObject.Instantiate(
				this.capitalShipPrefab,
				shipPos,
				shipRot ) as GameObject;
		}
		else // Normal operation
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
		masterScript.dummyShip = Network.peerType == NetworkPeerType.Disconnected 
			&& GameNetworkManager.instance.createCapitalShip
			&& _player != GamePlayerManager.instance.myPlayer;

		if ( masterScript.dummyShip == false )
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
		}
		DebugConsole.Log( "Creating capital ship for player " + _player.id + " on team " + _player.team, capitalObj );

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

		FighterMaster fighterScript = fighterObj.GetComponent<FighterMaster>();

		GameObject cameraObj = GameObject.Instantiate(
			this.fighterCameraPrefab,
			fighterObj.transform.position,
			Quaternion.identity ) as GameObject;

		FighterCamera cameraScript = cameraObj.GetComponent<FighterCamera>();
		cameraScript.fighter = fighterScript;

		DebugConsole.Log( "Creating fighter for player " + _player.id + " on team " + _player.team, fighterObj );

		return fighterObj;
	}
}
