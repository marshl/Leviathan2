using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuNetworking : BaseNetworkManager
{
	// Static Variables
	public static MenuNetworking instance;

	// Editor Variables
	public string gameTypeName;
	public int connectionLimit;

	// Public Variables
	[HideInInspector]
	public string gameName;

	[HideInInspector]
	public string gameComment;

	[HideInInspector]
	public HostData connectionHost = null;

	[HideInInspector]
	public int portNumber; 
	
	// Private Variables
	private HostData[] gameHosts;

	protected void Awake()
	{
		MenuNetworking.instance = this;
		MasterServer.RequestHostList( this.gameTypeName );
	}

	public void UpdateHostList()
	{
		MasterServer.RequestHostList( this.gameTypeName );
		this.gameHosts = MasterServer.PollHostList();
	}
	
	public NetworkConnectionError StartServer( int _port, string _gameName, string _comment )
	{
		this.gameName = _gameName;
		this.gameComment = _comment;
		this.portNumber = _port;

		Debug.Log( "Starting server Port:" + this.portNumber + " GameName:" + this.gameName + " Comment:"+ this.gameComment );
		NetworkConnectionError result = NetworkConnectionError.NoError;
		try
		{ 
			result = Network.InitializeServer( this.connectionLimit, _port, Network.HavePublicAddress() );
		}
		catch 
		{
			Debug.Log( "Error starting server: " + result.ToString() );
			return result;
		}
		Debug.Log( "Start Server result: " + result.ToString() ); 
		Debug.Log( "Registering master server: " + this.gameTypeName );
		MasterServer.RegisterHost( this.gameTypeName, this.gameName, this.gameComment );

		return result;
	}

	public NetworkConnectionError Connect( string _ip, int _port )
	{
		Debug.Log( "Connecting IP:" + _ip + " Port:" + _port );
		NetworkConnectionError result = Network.Connect( _ip, _port );
		Debug.Log( "Connection result " + _ip + ":" + _port + " " + result.ToString() );

		return result;
	}

	public bool IsValidHostIndex( int _index )
	{
		return _index >= 0 && _index < this.gameHosts.Length;
	}

	public NetworkConnectionError ConnectToHostIndex( int _index )
	{
		if ( _index < 0 || _index >= this.gameHosts.Length )
		{
			Debug.LogError( "Index out of range: " + _index, this );
			return NetworkConnectionError.ConnectionFailed;
		}

		this.connectionHost = this.gameHosts[ _index ];
		this.gameName = this.connectionHost.gameName;
		this.gameComment = this.connectionHost.comment;
		return this.Connect( this.connectionHost.ip[0], this.connectionHost.port );
	}

	public HostData GetHostData( int _index )
	{
		if ( this.IsValidHostIndex( _index ) )
		{
			return this.gameHosts[_index];
		}
		else
		{
			Debug.LogWarning( "Invalid host index" );
			return null;  
		} 
	}

	public void DisconnectFromLobby()
	{
		Debug.Log( "Disconnecting from lobby" );
		Network.Disconnect();
	}

	// Unity Callback: Do not change signature
	private void OnConnectedToServer()
	{
		Debug.Log( "Connected to server." );
		MainMenuButtons.instance.OpenGameLobby();
	}

	// Unity Callback: Do not change signature
	protected override void OnDisconnectedFromServer( NetworkDisconnection _info )
	{
		if ( Network.isServer )
		{
			Debug.Log( "Server has been shut down." );
		}
		else if ( _info == NetworkDisconnection.LostConnection )
		{
			Debug.Log( "Unexpected connection loss." );
			MenuLobby.instance.ExitLobby();
		}
		else if ( _info == NetworkDisconnection.Disconnected )
		{
			Debug.Log( "Diconnected from server." );
		}

		MainMenuButtons.instance.ExitLobby();
	}

	// Unity Callback: Do not change signature
	private void OnFailedToConnect( NetworkConnectionError _info )
	{
		switch ( _info )
		{
		case NetworkConnectionError.ConnectionFailed:
		{
			Debug.Log( "Connection Failure: General connection failure" );
			break;
		}
		default:
		{
			Debug.LogError( "Uncaught connection failure \"" + _info.ToString() + "\"" );
			break;
		}
		}

		MainMenuButtons.instance.OnConnectionFailure( _info );
	}

	// Unity Callback: Do not change signature
	private void OnPlayerConnected( NetworkPlayer _player )
	{
		Debug.Log( "Player connected IP:" + _player.ipAddress + " Port;" + _player.port
		          + " ExtIP:" + _player.externalIP + " ExtPort:" + _player.externalPort );

		if ( Network.isServer == true )
		{
			int playerID = Common.NetworkID( _player );

			PLAYER_TYPE playerType = GamePlayerManager.instance.GetNextFreePlayerType();//MenuLobby.instance.GetNextFreePlayerType();//( playerID );

			// If this is the server, tell the new guy who is in each of the teams
			//for ( int i = 0; i < Network.connections.Length; ++i )
			//foreach ( KeyValuePair<int, PLAYER_TYPE> pair in MenuLobby.instance.playerDictionary )
			foreach ( KeyValuePair<int, GamePlayer> pair in GamePlayerManager.instance.playerMap )
			{
				//int id = Common.NetworkID( Network.connections[i] );
				//if ( id != playerID )
				//if ( pair.Value != playerID )
				if ( pair.Key != playerID ) // Info about the player is sent further down
				{
					Debug.Log( "Telling " + playerID + " about " + pair.Key + ":" + pair.Value.playerType );
					//this.networkView.RPC( "SendPlayerTeamInfo", _player, Common.NetworkID( Network.connections[i]), (int)playerType );
					this.networkView.RPC( "SendPlayerTeamInfo", _player, pair.Key, (int)pair.Value.playerType );
					//this.networkView.RPC( "SendPlayerTeamInfo", _player, pair.Value.netPlayer, (int)pair.Value.playerType );
				}
			}
		
			Debug.Log( "Telling everyone else about " + playerID + ":" + playerType );
			// Then tell everyone about the new guy
			this.networkView.RPC( "SendPlayerTeamInfo", RPCMode.All, playerID, (int)playerType );
			//this.networkView.RPC( "SendPlayerTeamInfo", RPCMode.All, _player, (int)playerType );
		}
	}

	// Unity Callback: Do not change signature
	protected override void OnPlayerDisconnected( NetworkPlayer _player )
	{
		Debug.Log( "Player has disconnected IP:" + _player.ipAddress + " Port:" + _player.port );

		int playerID = Common.NetworkID( _player );
		//MenuLobby.instance.RemovePlayer( playerID );
		//GamePlayerManager.instance.RemovePlayer( _player );
		GamePlayerManager.instance.RemovePlayer( playerID );

		this.networkView.RPC( "OnRemovePlayerRPC", RPCMode.Others, playerID );
	}

	// Unity Callback: Do not change signature
	private void OnServerInitialized()
	{
		Debug.Log( "Server initialised." );
		//PLAYER_TYPE playerType = MenuLobby.instance.GetNextFreePlayerType();
		PLAYER_TYPE playerType = GamePlayerManager.instance.GetNextFreePlayerType();
		//MenuLobby.instance.AddPlayerOfType( Common.NetworkID( Network.player ), playerType );
		//GamePlayerManager.instance.AddPlayerOfType( Network.player, playerType );
		GamePlayerManager.instance.AddPlayerOfType( Common.MyNetworkID(), playerType );
	}

	// Unity Callback: Do not modify signature
	private void OnFailedToConnectToMasterServer( NetworkConnectionError _info )
	{
		Debug.LogWarning( "Could not connect to master server: " + _info );
	}

	private void OnMasterServerEvent( MasterServerEvent _event )
	{
		Debug.Log( "Master Server event " + _event.ToString() );
		//TODO: Implement cases found here http://docs.unity3d.com/Documentation/ScriptReference/MasterServerEvent.html LM 01/05/14
	}

	public void QuitLobby()
	{
		if ( Network.isServer )
		{
			Network.Disconnect();
			//TODO: Send message of some description
		}
		else
		{
			Network.Disconnect();
		}
	}

	public void StartGame()
	{
		this.networkView.RPC( "LoadLevel", RPCMode.All, "GameTest" );
	}

	[RPC]
	private void LoadLevel( string _level )
	{
		GameObject menuInfoObj = new GameObject();
		DontDestroyOnLoad( menuInfoObj );
		MenuToGameInfo infoScript = menuInfoObj.AddComponent<MenuToGameInfo>();
		MenuToGameInfo.instance = infoScript;
		//MenuLobby.instance.CopyInformationIntoGameInfo( infoScript );
		GamePlayerManager.instance.CopyInformationIntoGameInfo( infoScript );

		Application.LoadLevel( _level );
	}

	[RPC]
	private void SendPlayerTeamInfo( int _playerID, int _playerType )
	{
		if ( !System.Enum.IsDefined( typeof(PLAYER_TYPE), _playerType ) )
		{
			Debug.LogError( "Player type " + _playerType + " not defined" );
		}
		//MenuLobby.instance.AddPlayerOfType( _playerID, (PLAYER_TYPE)_playerType );
		GamePlayerManager.instance.AddPlayerOfType( _playerID, (PLAYER_TYPE)_playerType );
		//GamePlayerManager.instance.AddPlayerOfType( _netPlayer, (PLAYER_TYPE)_playerType );
	}

	[RPC]
	private void SendLobbyMessageRPC( int _playerID, string _message )
	{
		//MenuLobby.instance.ReceiveTextMessage( _viewID, _message );
		MessageManager.instance.AddMessage( _playerID, _message );
	}
	
	[RPC]
	private void SendPlayerTeamChangeRPC( int _playerID, int _playerType )
	{
		if ( !System.Enum.IsDefined( typeof(PLAYER_TYPE), _playerType ) )
		{
			Debug.LogError( "Player type " + _playerType + " not defined" );
		}
		//MenuLobby.instance.ChangePlayerType( _playerID, _type );
		GamePlayerManager.instance.ChangePlayerType( _playerID, (PLAYER_TYPE)_playerType );
	}

	/*
	[RPC]
	private void RequestPlayerListRPC( NetworkPlayer _player )
	{
		Debug.Log( "Player " + _player + " has requested player list" );

		foreach ( KeyValuePair<int, PLAYER_TYPE> pair in MenuLobby.instance.playerDictionary )
		{
			this.networkView.RPC( "SendPlayerTeamInfo", _player, pair.Key, (int)pair.Value );
		}
	}
*/
	[RPC]
	private void OnRemovePlayerRPC( int _playerID )
	{
		//MenuLobby.instance.RemovePlayer( _playerID );
		GamePlayerManager.instance.RemovePlayer( _playerID );
	}
}
