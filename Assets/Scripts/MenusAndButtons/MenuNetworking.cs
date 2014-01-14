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
	
	// Private Variables
	private HostData[] gameHosts;

	private void Awake()
	{
		MenuNetworking.instance = this;
		Debug.Log( "Requesting host list for \"" + this.gameTypeName + "\"" );
		MasterServer.RequestHostList( this.gameTypeName );
	}

	public void UpdateHostList()
	{
		MasterServer.ClearHostList();
		this.gameHosts = MasterServer.PollHostList();
		Debug.Log( "Games found: " + this.gameHosts.Length );
		foreach ( HostData hostData in this.gameHosts )
		{
			Debug.Log( hostData.gameName + " " + hostData.ip + " " + hostData.port + " " + hostData.useNat );
		}
	}
	
	public void StartServer( int _port, string _gameName, string _comment )
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
			return;
		}
		Debug.Log( "Start Server result: " + result.ToString() ); 
		Debug.Log( "Registering master server: " + this.gameTypeName );
		MasterServer.RegisterHost( this.gameTypeName, this.gameName, this.gameComment );
	}

	public void Connect( string _ip, int _port )
	{
		Debug.Log( "Connecting IP:" + _ip + " Port:" + _port );
		NetworkConnectionError result = Network.Connect( _ip, _port );
		Debug.Log( "Connection result " + _ip + ":" + _port + " " + result.ToString() );
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

	private void OnConnectedToServer()
	{
		Debug.Log( "Successfully connected to server." );
	}
	
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
	}
	
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
		default:
		{
			Debug.LogError( "Uncaught connection failure \"" + _info.ToString() + "\"" );
			break;
		}
		}
	}
	
	private void OnPlayerConnected( NetworkPlayer _player )
	{
		Debug.Log( "Player connected IP:" + _player.ipAddress + " Port;" + _player.port
		          + " ExtIP:" + _player.externalIP + " ExtPort:" + _player.externalPort );
	}
	
	private void OnPlayerDisconnected( NetworkPlayer _player )
	{
		Debug.Log( "Player has disconnected IP:" + _player.ipAddress + " Port:" + _player.port );
		Network.RemoveRPCs( _player );
		Network.DestroyPlayerObjects( _player ); 
	}
	
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
}
