using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GamePlayerManager : MonoBehaviour 
{
	public static GamePlayerManager instance;

	public Dictionary<int, GamePlayer> playerMap; 

	public GamePlayer commander1;
	public GamePlayer commander2;
	public List<GamePlayer> fighters1;
	public List<GamePlayer> fighters2;

	private void Awake()
	{
		GamePlayerManager.instance = this;

		this.playerMap = new Dictionary<int, GamePlayer>();
		this.commander1 = null;
		this.commander2 = null;

		this.fighters1 = new List<GamePlayer>();
		this.fighters2 = new List<GamePlayer>();
	}

	private void Update()
	{
		if ( Input.GetKeyDown(KeyCode.Alpha0 ) )
		{
			string str = "GamePlayerManager\n";

			str += "Commander 1: " + this.commander1 + "\n";
			str += "Commander 2: " + this.commander2 + "\n";

			str += "Team 1:\n";
			for ( int i = 0; i < this.fighters1.Count; ++i )
			{
				str += "\t" + this.fighters1[i] + "\n";
			}

			str += "Team 2:\n";
			for ( int i = 0; i < this.fighters2.Count; ++i )
			{
				str += "\t" + this.fighters2[i] + "\n";
			} 

			foreach ( KeyValuePair<int, GamePlayer> pair in this.playerMap )
			{
				str += "Player " + pair.Key + ":\n";
				str += "\tTeam: " + pair.Value.team +
					"\n\tType: " + pair.Value.playerType + "\n";
			}
			Debug.Log( str, this );

			foreach ( KeyValuePair<int, GamePlayer> pair in this.playerMap )
			{
				Debug.Log( "Player " + pair.Key + ":" + pair.Value.fighter, pair.Value.fighter );
				Debug.Log( "Player " + pair.Key + ":" + pair.Value.capitalShip, pair.Value.capitalShip  );
			}

			if ( GameNetworkManager.instance != null )
			{
				GameNetworkManager.instance.SendLobbyMessage( str, -1 );
			} 
		}
	}

	public void DisconnectPlayer( int _playerID )
	{
		this.GetPlayerWithID( _playerID ).isConnected = false;
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
			Debug.LogWarning( "Could not find player with id " + _playerID );
			return null;
		}
		return player;
	}

	// To be used by the server only
	public PLAYER_TYPE GetNextFreePlayerType()
	{
		if ( this.commander1 == null )
		{
			return PLAYER_TYPE.COMMANDER1;
		}
		else if ( this.commander2 == null )
		{
			return PLAYER_TYPE.COMMANDER2;
		}
		else if ( this.fighters1.Count > this.fighters2.Count )
		{
			return PLAYER_TYPE.FIGHTER2;
		}
		else
		{
			return PLAYER_TYPE.FIGHTER1;
		}
	}
	
	public void AddPlayerOfType( int _playerID, PLAYER_TYPE _playerType )
	{
		Debug.Log( "Adding player " + _playerID + " to " + _playerType );
		if ( this.playerMap.ContainsKey( _playerID ) )
		{
			Debug.LogWarning( "Player " + _playerID + " already added", this );
		}

		GamePlayer newPlayer = new GamePlayer();
		newPlayer.id = _playerID;

		this.playerMap.Add( newPlayer.id, newPlayer );

		this.ChangePlayerType( newPlayer.id, _playerType );
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
		
		if ( this.commander1 == player )
		{
			this.commander1 = null;
			Debug.Log( "Removing commander1" );
		}
		else if ( this.commander2 == player )
		{
			this.commander2 = null;
			Debug.Log( "Removing commander2" );
		}

		this.playerMap.Remove( _playerID );

		Debug.Log( "Removed player " + _playerID + " from " + type );
		return true;
	}

	public void ChangePlayerType( int _playerID, PLAYER_TYPE _newType )
	{
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
			if ( !this.fighters1.Remove( player ) )
			{
				Debug.LogError( "Error removing " + _playerID + " from team 1" );
			}
			break;
		case PLAYER_TYPE.FIGHTER2:
			if ( !this.fighters2.Remove( player ) )
			{
				Debug.LogError( "Error removing " + _playerID + " from team 2" );
			}
			break;
		case PLAYER_TYPE.COMMANDER1:
			if ( this.commander1 != player )
			{
				Debug.LogError( "Error removing " + _playerID + " from commander 1" );
			}
			this.commander1 = null;
			break;
		case PLAYER_TYPE.COMMANDER2:
			if ( this.commander2 != player )
			{
				Debug.LogError( "Error removing player " + _playerID + " from commander 2" );
			}
			this.commander2 = null;
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
			this.fighters1.Add( player );
			player.team = TEAM.TEAM_1;
			break;
		case PLAYER_TYPE.FIGHTER2:
			this.fighters2.Add( player );
			player.team = TEAM.TEAM_2;
			break;
		case PLAYER_TYPE.COMMANDER1:
			if ( this.commander1 != null )
			{
				Debug.LogError( "Commander 1 is already occupied. Cannot change to " + _playerID );
			}
			this.commander1 = player;
			player.team = TEAM.TEAM_1;
			break;
		case PLAYER_TYPE.COMMANDER2:
			if ( this.commander2 != null )
			{
				Debug.LogError( "Commander 2 is already occupied. Cannot change to " + _playerID );
			}
			this.commander2 = player;
			player.team = TEAM.TEAM_2;
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

		this.commander1 = this.commander2 = null;
		this.fighters1.Clear();
		this.fighters2.Clear();
		 
		this.playerMap.Clear();
	} 

}
