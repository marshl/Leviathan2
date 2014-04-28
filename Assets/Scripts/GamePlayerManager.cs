using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GamePlayerManager : MonoBehaviour 
{
	public static GamePlayerManager instance;

	public Dictionary<int, GamePlayer> playerMap; 

	private void Awake()
	{
		GamePlayerManager.instance = this;

		this.playerMap = new Dictionary<int, GamePlayer>();
	}

	public void AddPlayer( int _id, PLAYER_TYPE _playerType )
	{
		GamePlayer gamePlayer = new GamePlayer();
		gamePlayer.playerType = _playerType;

		if ( this.playerMap.ContainsKey( _id ) )
		{
			Debug.LogError( "Duplicate player " + _id);
			return; 
		}
		else
		{
			this.playerMap.Add( _id, gamePlayer );
		}
	}

	public void DisconnectPlayer( NetworkPlayer _player )
	{
		this.GetNetworkPlayer( _player ).isConnected = false;

		//TODO: Blow up their ship etc LM 24/04/14
	}

	public GamePlayer GetNetworkPlayer( NetworkPlayer _player )
	{
		int id = Common.NetworkID( _player );
		return this.GetPlayerWithID( id );
	}

	public GamePlayer GetPlayerWithID( int _playerID )
	{
		GamePlayer player;
		if ( !this.playerMap.TryGetValue( _playerID, out player ) )
		{
			Debug.LogError( "Could not find player with id " + _playerID );
			return null;
		}
		return player;
	}


}
