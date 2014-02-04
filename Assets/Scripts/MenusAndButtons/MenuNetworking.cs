using UnityEngine;
using System.Collections;

public class MenuNetworking : MonoBehaviour
{
	// Static Variables
	public static MenuNetworking instance;

	// Editor Variables
	public string gameTypeName = "Leviathan2Game";
	public int connections = 16;

	// Public Variables
	public string gameName;
	public string gameComment;
	public HostData connectionHost = null;
	
	// Private Variables
	private HostData[] gameHosts;

	private int lastLevelPrefix;
	
	private void Awake()
	{
		//DontDestroyOnLoad( this );
		MenuNetworking.instance = this;
		MasterServer.RequestHostList( this.gameTypeName );
	}

	public void UpdateHostList()
	{
		//MasterServer.ClearHostList();
		MasterServer.RequestHostList( this.gameTypeName );
		this.gameHosts = MasterServer.PollHostList();
		/*Debug.Log( "Games found: " + this.gameHosts.Length );
		foreach ( HostData hostData in this.gameHosts )
		{
			Debug.Log( hostData.gameName + " " + hostData.ip + " " + hostData.port + " " + hostData.useNat );
		}*/
	}
	
	public NetworkConnectionError StartServer( int _port, string _gameName, string _comment )
	{
		this.gameName = _gameName;
		this.gameComment = _comment;

		Debug.Log( "Starting server Port:" + _port + " GameName:" + this.gameName + " Comment:"+ this.gameComment );
		NetworkConnectionError result = NetworkConnectionError.NoError;
		try
		{ 
			result = Network.InitializeServer( this.connections, _port, Network.HavePublicAddress() );
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
		if ( _index < 0 )
		{
			Debug.LogError( "Cannot use negative index into game hosts" );
			return null;
		}
		if ( this.gameHosts == null || _index >= this.gameHosts.Length )
		{
			return null;
		}
		return this.gameHosts[_index];
	}

	// Unity Callback: Do not change signature
	private void OnConnectedToServer()
	{
		Debug.Log( "Successfully connected to server." );
		MainMenuButtons.instance.OpenGameLobby();
	}

	// Unity Callback: Do not change signature
	private void OnDisconnectedFromServer( NetworkDisconnection _info )
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
		case NetworkConnectionError.AlreadyConnectedToAnotherServer:
		{
			Debug.Log( "Connection Failure: Already connect to another server." );
			break;
		}
		case NetworkConnectionError.AlreadyConnectedToServer:
		{
			Debug.Log( "Connection Failure: Already connected to server." );
			break;
		}
		case NetworkConnectionError.ConnectionBanned:
		{
			Debug.Log( "Connection Failure: Banned from server." );
			break;
		}
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
	}

	// Unity Callback: Do not change signature
	private void OnPlayerConnected( NetworkPlayer _player )
	{
		Debug.Log( "Player connected IP:" + _player.ipAddress + " Port;" + _player.port
		          + " ExtIP:" + _player.externalIP + " ExtPort:" + _player.externalPort );

		//MenuLobby.instance.AddPlayer( _player );
	}

	// Unity Callback: Do not change signature
	private void OnPlayerDisconnected( NetworkPlayer _player )
	{
		Debug.Log( "Player has disconnected IP:" + _player.ipAddress + " Port:" + _player.port );
		//Network.RemoveRPCs( _player );
		//Network.DestroyPlayerObjects( _player ); 
		//MenuLobby.instance.RemovePlayer( _player );
	}

	// Unity Callback: Do not change signature
	private void OnServerInitialized()
	{
		Debug.Log( "Server initialised." );
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
		this.networkView.RPC( "LoadLevel", RPCMode.All, "GameTest", this.lastLevelPrefix + 1 );
	}

	[RPC]
	private void LoadLevel( string _level, int _prefix )
	{
		//this.lastLevelPrefix = _prefix;
		
		// There is no reason to send any more data over the network on the default channel,
		// because we are about to load the level, thus all those objects will get deleted anyway
		//Network.SetSendingEnabled( 0, false );	
		
		// We need to stop receiving because first the level must be loaded first.
		// Once the level is loaded, rpc's and other state update attached to objects in the level are allowed to fire
		//Network.isMessageQueueRunning = false;
		
		// All network views loaded from a level will get a prefix into their NetworkViewID.
		// This will prevent old updates from clients leaking into a newly created scene.
		//Network.SetLevelPrefix( _prefix );
		Application.LoadLevel( _level );
		//yield return new WaitForSeconds( 1.0f );
		//yield return new WaitForSeconds( 1.0f );
		
		// Allow receiving data again
		//Network.isMessageQueueRunning = true;
		// Now the level has been loaded and we can start sending out data to clients
		//Network.SetSendingEnabled( 0, true );
	}

	[RPC]
	private void SendLobbyMessageRPC( NetworkViewID _viewID, string _message )
	{
		MenuLobby.instance.SendLobbyMessage( _viewID, _message );
		
	}

	// Unity Callback: Do not modify signature
	private void OnFailedToConnectToMasterServer( NetworkConnectionError _info )
	{
		Debug.Log( "Could not connect to master server: " + _info );
	}
}
