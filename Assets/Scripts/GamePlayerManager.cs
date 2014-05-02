using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GamePlayerManager : MonoBehaviour 
{
	public static GamePlayerManager instance;

	public Dictionary<int, GamePlayer> playerMap; 

	public int commander1;
	public int commander2;
	public List<int> team1;
	public List<int> team2;

	private void Awake()
	{
		GamePlayerManager.instance = this;

		this.playerMap = new Dictionary<int, GamePlayer>();
		this.commander1 = -1;
		this.commander2 = -1;

		this.team1 = new List<int>();
		this.team2 = new List<int>();
	}

	/*public void AddPlayer( int _id, PLAYER_TYPE _playerType )
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
	}*/

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

	// To be used by the server only
	public PLAYER_TYPE GetNextFreePlayerType()
	{
		if ( this.commander1 == -1 )
		{
			return PLAYER_TYPE.COMMANDER1;
		}
		else if ( this.commander2 == -1 )
		{
			return PLAYER_TYPE.COMMANDER2;
		}
		else if ( this.team1.Count > this.team2.Count )
		{
			return PLAYER_TYPE.FIGHTER2;
		}
		else
		{
			return PLAYER_TYPE.FIGHTER1;
		}
	}

	//public void AddPlayerOfType( NetworkPlayer _netPlayer, PLAYER_TYPE _type )
	public void AddPlayerOfType( int _playerID, PLAYER_TYPE _playerType )
	{
		//int playerID = Common.NetworkID( _netPlayer );
		Debug.Log( "Adding player " + _playerID + " to " + _playerType );
		if ( this.playerMap.ContainsKey( _playerID ) )
		{
			Debug.LogWarning( "Player " + _playerID + " already added", this );
		}

		GamePlayer newPlayer = new GamePlayer();
		newPlayer.id = _playerID;
		//newPlayer.playerType = _type;

		this.playerMap.Add( newPlayer.id, newPlayer );

		/*switch ( _type )
		{
		case PLAYER_TYPE.COMMANDER1:
		{
			if ( this.commander1 != -1 )
			{
				Debug.LogWarning( "Commander role already set", this );
			}
			this.commander1 = playerID;
			Debug.Log( "Set commander 1 to " + playerID );
			break;
		}
		case PLAYER_TYPE.COMMANDER2:
		{
			if ( this.commander2 != -1 )
			{
				Debug.LogWarning( "Commander 2 rol already set", this );
			}
			this.commander2 = playerID;
			Debug.Log( "Set commander 2 to " + playerID );
			break;
		}
		case PLAYER_TYPE.FIGHTER1:
		{
			this.team1.Add( playerID );
			break;
		}
		case PLAYER_TYPE.FIGHTER2:
		{
			this.team2.Add( playerID );
			break;
		}
		default:
		{
			Debug.LogError( "Uncaught player type " + _type );
			break;
		}
		}*/

		this.ChangePlayerType( newPlayer.id, _playerType );
		Debug.Log( "Added player " + newPlayer.id + " to type " + _playerType );
	}

	public bool RemovePlayer( int _playerID )
	{
		if ( !this.playerMap.ContainsKey( _playerID ) )
		{
			Debug.LogError( "Cannot remove player " + _playerID );
			return false;
		}
		GamePlayer player = this.playerMap[_playerID];
		PLAYER_TYPE type = player.playerType;
		
		if ( this.commander1 == _playerID )
		{
			this.commander1 = -1;
			Debug.Log( "Removing commander1" );
		}
		else if ( this.commander2 == _playerID )
		{
			this.commander2 = -1;
			Debug.Log( "Removing commander2" );
		}

		this.playerMap.Remove( _playerID );

		Debug.Log( "Removed player " + _playerID + " from " + type );
		return true;
	}

	public void ChangePlayerType( int _playerID, PLAYER_TYPE _newType )
	{
		/*if ( this.RemovePlayer( _playerID, ref netPlayer ) )
		{
			this.AddPlayerOfType( netPlayer, _type );
		}*/

		if ( !this.playerMap.ContainsKey( _playerID ) )
		{
			Debug.LogError( "Unknown player ID " + _playerID );
			return;
		}

		GamePlayer player = this.playerMap[_playerID];

		if ( player.playerType == _newType )
		{
			Debug.LogWarning( "Useless change request changing player " + _playerID + " to " + _newType );
		}

		switch ( player.playerType )
		{
		case PLAYER_TYPE.UNDEFINED:
			break;
		case PLAYER_TYPE.FIGHTER1:
			if ( !this.team1.Remove( _playerID ) )
			{
				Debug.LogError( "Error removing " + _playerID + " from team 1" );
			}
			break;
		case PLAYER_TYPE.FIGHTER2:
			if ( !this.team2.Remove( _playerID ) )
			{
				Debug.LogError( "Error removing " + _playerID + " from team 2" );
			}
			break;
		case PLAYER_TYPE.COMMANDER1:
			if ( this.commander1 != _playerID )
			{
				Debug.LogError( "Error removing " + _playerID + " from commander 1" );
			}
			this.commander1 = -1;
			break;
		case PLAYER_TYPE.COMMANDER2:
			if ( this.commander2 != _playerID )
			{
				Debug.LogError( "Error removing player " + _playerID + " from commander 2" );
			}
			this.commander2 = -1;
			break;
		
		default:
			Debug.LogError( "Uncaught player type " + _playerID + ":" + player.playerType );
			break;
		}

		player.playerType = _newType;

		switch ( player.playerType )
		{
		case PLAYER_TYPE.UNDEFINED:
			Debug.LogWarning( "You shouldn't be changing the player type to UNDEFINED " + player.id );
			break;
		case PLAYER_TYPE.FIGHTER1:
			this.team1.Add( _playerID );
			break;
		case PLAYER_TYPE.FIGHTER2:
			this.team2.Add( _playerID );
			break;
		case PLAYER_TYPE.COMMANDER1:
			if ( this.commander1 != -1 )
			{
				Debug.LogError( "Commander 1 is already occupied. Cannot change to " + _playerID );
			}
			this.commander1 = _playerID;
			break;
		case PLAYER_TYPE.COMMANDER2:
			if ( this.commander2 != -1 )
			{
				Debug.LogError( "Commander 2 is already occupied. Cannot change to " + _playerID );
			}
			this.commander2 = _playerID;
			break;
			
		default:
			Debug.LogError( "Uncaught player type " + _playerID + ":" + player.playerType );
			break;
		}
	}

	public void CopyInformationIntoGameInfo( MenuToGameInfo _info )
	{
		int id = Common.MyNetworkID();
		if ( this.playerMap.ContainsKey( id ) )
		{
			_info.playerType = this.playerMap[ id ].playerType;
		}
		else
		{
			Debug.LogError( "Could not find player type in map " + id );
		}
	}

	public void Reset() // protip: don't call this in the game
	{
		Debug.Log( "Resetting GamePlayerManager" );

		this.commander1 = this.commander2 = -1;
		this.team1.Clear();
		this.team2.Clear();

		this.playerMap.Clear();
	}

}
