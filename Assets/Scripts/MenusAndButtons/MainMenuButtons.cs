using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenuButtons : MonoBehaviour
{
	public GameObject cubePrefab;
	/*public enum MAIN_MENU_BUTTON_TYPE : int
	{
		HOST,
		JOIN,
		QUIT,
	};
	public MAIN_MENU_BUTTON_TYPE buttonType;

	public float scaleIncrease;

	private void OnMouseEnter()
	{
		this.transform.localScale = Vector3.one * this.scaleIncrease;
	}

	private void OnMouseExit()
	{
		this.transform.localScale = Vector3.one;
	}

	private void OnMouseDown()
	{
		Debug.Log ("!");
		switch ( this.buttonType )
		{
		case MAIN_MENU_BUTTON_TYPE.HOST:
		{
			MenuHost.instance.InitialiseServer();
			break;
		}
		case MAIN_MENU_BUTTON_TYPE.JOIN:
		{

			break;
		}
		case MAIN_MENU_BUTTON_TYPE.QUIT:
		{
			Application.Quit();
			break;
		}
		default:
		{
			Debug.LogError( "Uncaught button type \"" + this.buttonType + "\"", this );
			break;
		}
		}
	}*/

	public Rect hostRect;
	public Rect connectRect;
	public Rect portRect;
	public Rect ipRect;
	
	public string portString;
	public string ipString;
	public string messageString;

	private List<string> messageList;

	private void Awake()
	{
		this.messageList = new List<string>();
	}

	private void OnGUI()
	{


		switch ( Network.peerType )
		{
		case NetworkPeerType.Client:
		{
			GUI.Label( new Rect( 200, 50, 100, 25), "Client" );
			this.ShowPlayers();
			this.DisplayMessageInput();
			this.DisplayMessages();
			break;
		}
		case NetworkPeerType.Connecting:
		{
			GUI.Label( new Rect( 200, 50, 100, 25), "Connecting" );
			break;
		}
		case NetworkPeerType.Disconnected:
		{
			GUI.Label( new Rect( 200, 50, 100, 25), "Disconnected" );

			this.portString = GUI.TextField( portRect, this.portString );
			this.ipString = GUI.TextField( ipRect, this.ipString );
			if ( GUI.Button ( this.hostRect, "Host" ) )
			{
				int port;
				if ( int.TryParse( this.portString, out port ) )
				{
					this.StartServer( port );
				}
				
			}
			else if ( GUI.Button( connectRect, "Connect" ) )
			{
				int port;
				if ( int.TryParse( this.portString, out port ) )
				{
					this.Connect( this.ipString, port );
				}
			}
			break;
		}
		case NetworkPeerType.Server:
		{
			GUI.Label( new Rect( 200, 50, 100, 25), "Server" );

			this.ShowPlayers();
			this.DisplayMessageInput();
			this.DisplayMessages();
			break;
		}
		default:
		{
			Debug.Log( "Uncaught network peer type \"" + Network.peerType + "\"" );
			break;
		}
		}
	}

	private void ShowPlayers()
	{
		for ( int i = 0; i < Network.connections.Length; ++i )
		{
			NetworkPlayer player = Network.connections[i];
			GUI.Label( new Rect( 300, (i+1)*100, 100, 25), player.ipAddress );
		}
	}

	private void StartServer( int _port )
	{
		NetworkConnectionError result = 
			Network.InitializeServer( 16, _port, true );
		Debug.Log( result.ToString() ); 
	}

	private void Connect( string _ip, int _port )
	{
		NetworkConnectionError result = Network.Connect( _ip, _port );
		Debug.Log( result.ToString() );
	}

	private void OnConnectedToServer()
	{
		Debug.Log( "Connected to server." );
		//GameObject.Instantiate( cubePrefab ); 
		//Network.Instantiate( cubePrefab, Vector3.zero, Quaternion.identity, 0 );
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
		//GameObject.Instantiate( cubePrefab ); 
		//Network.Instantiate( cubePrefab, Vector3.zero, Quaternion.identity, 0 );
	}

	private void DisplayMessages()
	{
		for ( int i = 0; i < this.messageList.Count; ++i )
		{
			GUI.Label( new Rect( 300, 75 + i*25, 100, 25 ), this.messageList[i]);
		}
	}

	private void DisplayMessageInput()
	{
		this.messageString = GUI.TextField( new Rect( 300, 50, 100, 25 ), this.messageString );

		if ( GUI.Button( new Rect( 400, 50, 50, 25 ), "Send" ) )
		{
			//this.AddMessage( this.messageString );
			networkView.RPC( "AddMessage", RPCMode.All, this.messageString );
			this.messageString = "";
		}
	}

	[RPC]
	private void AddMessage( string _message )
	{
		this.messageList.Add( _message );
	}
}
