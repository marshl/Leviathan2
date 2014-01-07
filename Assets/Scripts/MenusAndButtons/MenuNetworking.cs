using UnityEngine;
using System.Collections;

public class MenuNetworking : MonoBehaviour
{
	// Static Variables
	public static MenuNetworking instance;

	// Editor Variables
	public string gameTypeName = "Leviathan2Game";

	private void Awake()
	{
		MenuNetworking.instance = this;
	}
	
	public void StartServer( int _port, string _gameName, string _comment )
	{
		Debug.Log ( "Has public address: " + Network.HavePublicAddress().ToString() );
		NetworkConnectionError result = 
			Network.InitializeServer( 16, _port, Network.HavePublicAddress() );
		
		MasterServer.RegisterHost( this.gameTypeName, _gameName, _comment );
		Debug.Log( result.ToString() ); 
	}
	
	public void Connect( string _ip, int _port )
	{
		NetworkConnectionError result = Network.Connect( _ip, _port );
		Debug.Log( "Connecting to " + _ip + ":" + _port + " " + result.ToString() );
	}
	
	private void OnConnectedToServer()
	{
		Debug.Log( "Connected to server." );
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
		Debug.Log( "Player connected from " + _player.ipAddress + ":" + _player.port );
	}
	
	private void OnPlayerDisconnected( NetworkPlayer _player )
	{
		Debug.Log( "Player (" + _player.ipAddress + ":" + _player.port + ") has disconnected" );
		Network.RemoveRPCs( _player );
		Network.DestroyPlayerObjects( _player ); 
	}
	
	private void OnServerInitialized()
	{
		Debug.Log( "Server is now initialised." );
		//GameObject.Instantiate( cubePrefab ); 
		//Network.Instantiate( cubePrefab, Vector3.zero, Quaternion.identity, 0 );
	}
}
