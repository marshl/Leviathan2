/// <summary>
/// The game player manager stores every GamePlayer in the game or lobby
/// 
/// "Who are they?" - Helix
/// </summary>

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

	public GamePlayer myPlayer;

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
			DebugConsole.Log( str, this );

			foreach ( KeyValuePair<int, GamePlayer> pair in this.playerMap )
			{
				DebugConsole.Log( "Player " + pair.Key + ":" + pair.Value.fighter, pair.Value.fighter );
				DebugConsole.Log( "Player " + pair.Key + ":" + pair.Value.capitalShip, pair.Value.capitalShip  );
			}

			if ( GameNetworkManager.instance != null )
			{
				MessageManager.instance.CreateMessageLocal( str, MESSAGE_TYPE.TO_ALL );
			} 
		}

		foreach ( KeyValuePair<int, GamePlayer> pair in this.playerMap )
		{
			GamePlayer player = pair.Value;

			if ( player == this.myPlayer )
			{
				continue;
			}

			if ( player.ping == null )
			{
				player.ping = new Ping( player.networkPlayer.ipAddress );
			}
			else if ( player.ping.isDone )
			{
				player.pingMS = player.ping.time;
				player.ping = null;
			}
		}
	}

	public void DisconnectPlayer( int _playerID )
	{
		DebugConsole.Log( "Player (" + _playerID + ") disconnected" );
		this.GetPlayerWithID( _playerID ).isConnected = false;

		if ( Network.isServer )
		{
			GamePlayer player = GamePlayerManager.instance.GetPlayerWithID( _playerID );
			if ( player.fighter != null )
			{
				TargetManager.instance.RemoveTarget( player.fighter.health );
				DebugConsole.Log( "Destroyed fighter object" );
				Network.Destroy( player.fighter.networkView.viewID );
			}
		}
	}

	public GamePlayer GetNetworkPlayer( NetworkPlayer _player )
	{
		int id = Common.NetworkID( _player );
		return this.GetPlayerWithID( id );
	}

	public GamePlayer GetCommander( TEAM _team )
	{
		switch ( _team )
		{
		case TEAM.TEAM_1:
		{
			return this.commander1;
		}
		case TEAM.TEAM_2:
		{
			return this.commander2;
		}
		default:
		{
			Debug.LogError( "Cannot fetch commander of team type " + _team );
			return null;
		}
		}
	}

	public GamePlayer GetPlayerWithID( int _playerID )
	{
		GamePlayer player;
		if ( !this.playerMap.TryGetValue( _playerID, out player ) )
		{
			DebugConsole.Warning( "Could not find player with ID " + _playerID );
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
	
	public GamePlayer AddPlayerOfType( int _playerID, PLAYER_TYPE _playerType )
	{
		DebugConsole.Log( "Adding player " + _playerID + " to " + _playerType );
		if ( this.playerMap.ContainsKey( _playerID ) )
		{
			DebugConsole.Warning( "Player " + _playerID + " already added", this );
			return null;
		}

		GamePlayer newPlayer = new GamePlayer();
		newPlayer.id = _playerID;

		this.playerMap.Add( newPlayer.id, newPlayer );

		this.ChangePlayerType( newPlayer.id, _playerType );

		if ( _playerID == Common.MyNetworkID() )
		{
			Debug.Log( "Setting my player to " + _playerID );
			this.myPlayer = newPlayer;
		}

		if ( Network.peerType != NetworkPeerType.Disconnected )
		{
			foreach ( NetworkPlayer netPlayer in Network.connections )
			{
				if ( Common.NetworkID( netPlayer ) == _playerID )
				{
					newPlayer.networkPlayer = netPlayer;
					break;
				}
			}
		}

		return newPlayer;
	}

	public bool RemovePlayer( int _playerID )
	{
		if ( !this.playerMap.ContainsKey( _playerID ) )
		{
			DebugConsole.Error( "Cannot remove player " + _playerID );
			return false;
		}
		GamePlayer player = this.playerMap[_playerID];
		PLAYER_TYPE type = player.playerType;
		
		if ( this.commander1 == player )
		{
			this.commander1 = null;
			DebugConsole.Log( "Removing commander1" );
		}
		else if ( this.commander2 == player )
		{
			this.commander2 = null;
			DebugConsole.Log( "Removing commander2" );
		}

		this.playerMap.Remove( _playerID );

		DebugConsole.Log( "Removed player " + _playerID + " from " + type );
		return true;
	}

	public void ChangePlayerType( int _playerID, PLAYER_TYPE _newType )
	{
		if ( !this.playerMap.ContainsKey( _playerID ) )
		{
			DebugConsole.Error( "Unknown player ID " + _playerID );
			return;
		}

		GamePlayer player = this.playerMap[_playerID];

		if ( player.playerType == _newType )
		{
			DebugConsole.Warning( "Useless change request changing player " + _playerID + " to " + _newType );
		}

		switch ( player.playerType )
		{
		case PLAYER_TYPE.UNDEFINED:
			break;
		case PLAYER_TYPE.FIGHTER1:
			if ( !this.fighters1.Remove( player ) )
			{
				DebugConsole.Error( "Error removing " + _playerID + " from team 1" );
			}
			break;
		case PLAYER_TYPE.FIGHTER2:
			if ( !this.fighters2.Remove( player ) )
			{
				DebugConsole.Error( "Error removing " + _playerID + " from team 2" );
			}
			break;
		case PLAYER_TYPE.COMMANDER1:
			if ( this.commander1 != player )
			{
				DebugConsole.Error( "Error removing " + _playerID + " from commander 1" );
			}
			this.commander1 = null;
			break;
		case PLAYER_TYPE.COMMANDER2:
			if ( this.commander2 != player )
			{
				DebugConsole.Error( "Error removing player " + _playerID + " from commander 2" );
			}
			this.commander2 = null;
			break;
		
		default:
			DebugConsole.Error( "Uncaught player type " + _playerID + ":" + player.playerType );
			break;
		}

		player.playerType = _newType;

		switch ( player.playerType )
		{
		case PLAYER_TYPE.UNDEFINED:
			DebugConsole.Warning( "You shouldn't be changing the player type to UNDEFINED " + player.id );
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
				DebugConsole.Error( "Commander 1 is already occupied. Cannot change to " + _playerID );
			}
			this.commander1 = player;
			player.team = TEAM.TEAM_1;
			break;
		case PLAYER_TYPE.COMMANDER2:
			if ( this.commander2 != null )
			{
				DebugConsole.Error( "Commander 2 is already occupied. Cannot change to " + _playerID );
			}
			this.commander2 = player;
			player.team = TEAM.TEAM_2;
			break;
			
		default:
			DebugConsole.Error( "Uncaught player type " + _playerID + ":" + player.playerType );
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
			DebugConsole.Error( "Could not find player type in map " + id );
		}
	}

	public void Reset() // protip: don't call this in the game
	{
		DebugConsole.Log( "Resetting GamePlayerManager" );

		this.commander1 = this.commander2 = null;
		this.fighters1.Clear();
		this.fighters2.Clear();
		 
		this.playerMap.Clear();
	} 

	public CapitalShipMaster GetCapitalShip( TEAM _team )
	{
		switch ( _team )
		{
		case TEAM.TEAM_1:
			if ( this.commander1 == null )
			{
				DebugConsole.Warning( "Commander 1 not found" );
				return null;
			}
			return this.commander1.capitalShip;
		case TEAM.TEAM_2:
			if ( this.commander2 == null )
			{
				DebugConsole.Warning( "Commander 2 not found" );
				return null;
			}
			return this.commander2.capitalShip;
		default:
			DebugConsole.Error( "Could not find capital ship for team " + _team );
			return null;
		}
	}

	public bool ValidTypeChange( int _playerID, PLAYER_TYPE _playerType )
	{
		GamePlayer player = this.GetPlayerWithID( _playerID );
		if ( player == null )
		{
			return false;
		}

		if ( player.playerType == _playerType )
		{
			return false;
		}

		switch ( _playerType )
		{
		case PLAYER_TYPE.COMMANDER1:
		{
			return this.commander1 == null;
		}
		case PLAYER_TYPE.COMMANDER2:
		{
			return this.commander2 == null;
		}
		//TODO: Fighter list size check
		default:
		{
			return true;
		}
		}
	}

	public void AddKill( int _playerID, bool _broadcast )
	{
		GamePlayer player = this.GetPlayerWithID( _playerID );
		++player.kills;


		if ( _broadcast && Network.peerType != NetworkPeerType.Disconnected )
		{
			GameNetworkManager.instance.SendAddKillMessage( _playerID );
		}
	}

	public void AddDeath( int _playerID, bool _broadcast )
	{
		GamePlayer player = this.GetPlayerWithID( _playerID );
		++player.deaths;

		if ( _broadcast && Network.peerType != NetworkPeerType.Disconnected )
		{
			GameNetworkManager.instance.SendAddDeathMessage( _playerID );
		}
	}

}
