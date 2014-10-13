using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuNetworking : BaseNetworkManager
{
	public static MenuNetworking instance;
	
	public string gameTypeName;
	public int connectionLimit;
	
	public string gameName;
	public string gameComment;
	public int portNumber;

	public HostData connectionHost = null;

	private float hostRefreshTimer;
	public float hostRefreshRate;
	
	public HostData[] gameHosts;

	protected void Awake()
	{
		MenuNetworking.instance = this;
		MasterServer.RequestHostList( this.gameTypeName );
	}

	private void Update()
	{
		this.hostRefreshTimer += Time.deltaTime;
		if ( this.hostRefreshTimer >= this.hostRefreshRate )
		{
			this.hostRefreshTimer = 0.0f;
			MasterServer.RequestHostList( this.gameTypeName );
			this.gameHosts = MasterServer.PollHostList();
		}
	}

	public NetworkConnectionError StartServer( int _port, string _gameName, string _comment, string _password )
	{
		this.gameName = _gameName;
		this.gameComment = _comment;
		this.portNumber = _port;

		DebugConsole.Log( "Starting server Port:" + this.portNumber + " GameName:" + this.gameName + " Comment:"+ this.gameComment + " Password:" + _password );
		NetworkConnectionError result = NetworkConnectionError.NoError;
		try
		{
			result = Network.InitializeServer( this.connectionLimit, _port, Network.HavePublicAddress() );
		}
		catch
		{
			DebugConsole.Log( "Error starting server: " + result.ToString() );
			return result;
		}
		Network.incomingPassword = _password;

		DebugConsole.Log( "Start Server result: " + result.ToString() );
		DebugConsole.Log( "Registering master server: " + this.gameTypeName );
		MasterServer.RegisterHost( this.gameTypeName, this.gameName, this.gameComment );

		return result;
	}

	public NetworkConnectionError Connect( string _ip, int _port, string _password )
	{
		DebugConsole.Log( "Connecting IP:" + _ip + " Port:" + _port + " Password:" + _password );
		NetworkConnectionError result = Network.Connect( _ip, _port, _password );
		DebugConsole.Log( "Connection result " + _ip + ":" + _port + " " + result.ToString() );

		return result;
	}

	public bool IsValidHostIndex( int _index )
	{
		return this.gameHosts != null && _index >= 0 && _index < this.gameHosts.Length;
	}

	public NetworkConnectionError ConnectToHostIndex( int _index )
	{
		if ( _index < 0 || _index >= this.gameHosts.Length )
		{
			DebugConsole.Error( "Index out of range: " + _index, this );
			return NetworkConnectionError.ConnectionFailed;
		}

		this.connectionHost = this.gameHosts[ _index ];
		this.gameName = this.connectionHost.gameName;
		this.gameComment = this.connectionHost.comment;
		return this.Connect( this.connectionHost.ip[0], this.connectionHost.port, "" );
	}

	public HostData GetHostData( int _index )
	{
		if ( this.IsValidHostIndex( _index ) )
		{
			return this.gameHosts[_index];
		}
		else
		{
			DebugConsole.Warning( "Invalid host index" );
			return null;
		}
	}

	public void DisconnectFromLobby()
	{
		DebugConsole.Log( "Disconnecting from lobby" );
		Network.Disconnect();
	}

	// Unity Callback: Do not change signature
	private void OnConnectedToServer()
	{
		DebugConsole.Log( "Connected to server." );
		MenuGUI.instance.OpenGameLobby();
		this.networkView.RPC( "OnSendConnectedInfoRPC", RPCMode.Others, Common.MyNetworkID(), PlayerOptions.instance.options.playerName );
	}

	[RPC]
	private void OnSendConnectedInfoRPC( int _playerID, string _playerName )
	{
		GamePlayerManager.instance.GetPlayerWithID( _playerID ).name = _playerName;
		MessageManager.instance.AddMessage( _playerID, _playerName + " has connected", true );
	}

	// Unity Callback: Do not change signature
	protected override void OnDisconnectedFromServer( NetworkDisconnection _info )
	{
		if ( Network.isServer )
		{
			DebugConsole.Log( "Server has been shut down." );
		}
		else if ( _info == NetworkDisconnection.LostConnection )
		{
			DebugConsole.Log( "Unexpected connection loss." );
		}
		else if ( _info == NetworkDisconnection.Disconnected )
		{
			DebugConsole.Log( "Diconnected from server." );
		}
		MenuGUI.instance.OnDisconnectedFromServer( _info );
	}

	// Unity Callback: Do not change signature
	private void OnFailedToConnect( NetworkConnectionError _info )
	{
		/*switch ( _info )
		{
		case NetworkConnectionError.ConnectionFailed:
		{
			DebugConsole.Log( "Connection Failure: General connection failure" );
			break;
		}
		default:
		{
			DebugConsole.Error( "Uncaught connection failure \"" + _info.ToString() + "\"" );
			break;
		}
		}*/

		MenuGUI.instance.OnFailedToConnect( _info );
	}

	// Unity Callback: Do not change signature (only called on the server)
	private void OnPlayerConnected( NetworkPlayer _player )
	{
		DebugConsole.Log( "Player connected IP:" + _player.ipAddress + " Port;" + _player.port
		          + " ExtIP:" + _player.externalIP + " ExtPort:" + _player.externalPort );

		int playerID = Common.NetworkID( _player );

		PLAYER_TYPE playerType = GamePlayerManager.instance.GetNextFreePlayerType();//MenuLobby.instance.GetNextFreePlayerType();//( playerID );

		// If this is the server, tell the new guy who is in each of the teams
		foreach ( KeyValuePair<int, GamePlayer> pair in GamePlayerManager.instance.playerMap )
		{
			if ( pair.Key != playerID ) // Info about the player is sent further down
			{
				DebugConsole.Log( "Telling " + playerID + " about " + pair.Key + ":" + pair.Value.playerType );
				this.networkView.RPC( "SendPlayerTeamInfo", _player, pair.Key, (int)pair.Value.playerType );
			}
		}

		DebugConsole.Log( "Telling everyone else about " + playerID + ":" + playerType );
		// Then tell everyone about the new guy
		this.networkView.RPC( "SendPlayerTeamInfo", RPCMode.All, playerID, (int)playerType );

		this.networkView.RPC( "OnSendConnectedInfoRPC", _player, Common.MyNetworkID(), GamePlayerManager.instance.myPlayer.name );

	}

	// Unity Callback: Do not change signature
	protected override void OnPlayerDisconnected( NetworkPlayer _player )
	{
		DebugConsole.Log( "Player has disconnected IP:" + _player.ipAddress + " Port:" + _player.port );

		int playerID = Common.NetworkID( _player );
		GamePlayerManager.instance.RemovePlayer( playerID );

		this.networkView.RPC( "OnRemovePlayerRPC", RPCMode.Others, playerID );

		MessageManager.instance.AddMessage( playerID, "Player " + playerID + " has disconnected", false );
	}

	// Unity Callback: Do not change signature
	private void OnServerInitialized()
	{
		DebugConsole.Log( "Server initialised." );
		PLAYER_TYPE playerType = GamePlayerManager.instance.GetNextFreePlayerType();
		GamePlayerManager.instance.AddPlayerOfType( Common.MyNetworkID(), playerType );
		GamePlayerManager.instance.myPlayer.name = PlayerOptions.instance.options.playerName;
	}

	// Unity Callback: Do not modify signature
	private void OnFailedToConnectToMasterServer( NetworkConnectionError _info )
	{
		DebugConsole.Warning( "Could not connect to master server: " + _info );
	}

	private void OnMasterServerEvent( MasterServerEvent _event )
	{
		DebugConsole.Log( "Master Server event " + _event.ToString() );
		//TODO: Implement cases found here http://docs.unity3d.com/Documentation/ScriptReference/MasterServerEvent.html LM 01/05/14
	}

	public void QuitLobby()
	{
		if ( Network.isServer )
		{
			Network.Disconnect();
			MasterServer.UnregisterHost();
		}
		else
		{
			Network.Disconnect();
		}
	}

	public void StartGame()
	{
		MasterServer.UnregisterHost();
		this.networkView.RPC( "OnLoadGameSceneRPC", RPCMode.All );
	}

	[RPC]
	private void OnLoadGameSceneRPC()
	{
		GameObject menuInfoObj = new GameObject();
		DontDestroyOnLoad( menuInfoObj );
		MenuToGameInfo infoScript = menuInfoObj.AddComponent<MenuToGameInfo>();
		MenuToGameInfo.instance = infoScript;
		GamePlayerManager.instance.CopyInformationIntoGameInfo( infoScript );

		Application.LoadLevel( "GameScene" );
	}

	public void SendChangePlayerTypeMessage( int _playerID, PLAYER_TYPE _playerType )
	{
		this.networkView.RPC( "OnChangePlayerTypeRPC", RPCMode.All, _playerID, (int)_playerType );
	}

	[RPC]
	private void OnChangePlayerTypeRPC( int _playerID, int _playerType )
	{
		if ( !System.Enum.IsDefined( typeof(PLAYER_TYPE), _playerType ) )
		{
			DebugConsole.Error( "Player type " + _playerType + " not defined" );
		}
		GamePlayerManager.instance.ChangePlayerType( _playerID, (PLAYER_TYPE)_playerType );
	}

	public void SendValidatePlayerTypeChange( int _playerID, PLAYER_TYPE _playerType )
	{
		this.networkView.RPC( "OnValidatePlayerTypeChangeRPC", RPCMode.Server, _playerID, (int)_playerType );
	}

	[RPC]
	private void OnValidatePlayerTypeChangeRPC( int _playerID, int _playerType )
	{
		if ( GamePlayerManager.instance.ValidTypeChange( _playerID, (PLAYER_TYPE)_playerType ) )
		{
			this.SendChangePlayerTypeMessage( _playerID, (PLAYER_TYPE)_playerType );
		}
		else
		{
			DebugConsole.Warning( "Cannot change " + _playerID + " to " + _playerType );
			//Send request denial
		}
	}

	[RPC]
	private void SendPlayerTeamInfo( int _playerID, int _playerType )
	{
		if ( !System.Enum.IsDefined( typeof(PLAYER_TYPE), _playerType ) )
		{
			DebugConsole.Error( "Player type " + _playerType + " not defined" );
		}

		GamePlayer newPlayer = GamePlayerManager.instance.AddPlayerOfType( _playerID, (PLAYER_TYPE)_playerType );

		if ( newPlayer == GamePlayerManager.instance.myPlayer )
		{
			newPlayer.name = PlayerOptions.instance.options.playerName;
		}
	}

	[RPC]
	private void SendPlayerTeamChangeRPC( int _playerID, int _playerType )
	{
		if ( !System.Enum.IsDefined( typeof(PLAYER_TYPE), _playerType ) )
		{
			DebugConsole.Error( "Player type " + _playerType + " not defined" );
		}

		GamePlayerManager.instance.ChangePlayerType( _playerID, (PLAYER_TYPE)_playerType );
	}

	[RPC]
	private void OnRemovePlayerRPC( int _playerID )
	{
		GamePlayerManager.instance.RemovePlayer( _playerID );
	}
}
