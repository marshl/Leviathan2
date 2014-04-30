using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MenuLobby : MonoBehaviour
{
	// Instance
	public static MenuLobby instance;

	// Private Structures
	private class LobbyMessage
	{
		public NetworkPlayer sender;
		public string message;
		public string playerName;
		public DateTime timeReceived;
	};

	// Editor Variables
	public GUIText gameNameText;
	public GUIText gameCommentText;
	public GUITextField messageTextField;
	public GUIText messageText;
	public int messageLimit;

	public MenuPlayerRow firstPlayerRow;
	public float playerRowOffset;

	public string[] playerMessageColours;

	// Private Variables
	private List<LobbyMessage> messages;
	private List<MenuPlayerRow> playerRows;

	public Dictionary<int, PLAYER_TYPE> playerDictionary;
	private int commander1;
	private int commander2;
	private List<int> team1;
	private List<int> team2;

	private bool playerListNeedsUpdating = true;

	private void Awake ()
	{
		MenuLobby.instance = this;
		this.messages = new List<LobbyMessage>();
		this.playerRows = new List<MenuPlayerRow>();
		this.playerDictionary = new Dictionary<int, PLAYER_TYPE>();
		this.team1 = new List<int>();
		this.team2 = new List<int>();
		this.commander1 = this.commander2 = -1;
	}

	private void Start()
	{
		for ( int i = 0; i < MenuNetworking.instance.connectionLimit; ++i )
		{
			GameObject rowObj = ( i == 0 ) ? this.firstPlayerRow.gameObject
				: GameObject.Instantiate( this.firstPlayerRow.gameObject ) as GameObject;

			rowObj.transform.parent = this.firstPlayerRow.transform.parent;

			foreach ( GUIText text in rowObj.GetComponentsInChildren<GUIText>( true ) )
			{
				text.pixelOffset += new Vector2( 0.0f, (float)i * this.playerRowOffset );
			}

			rowObj.name = "PlayerRow" + i;
			MenuPlayerRow rowScript = rowObj.GetComponent<MenuPlayerRow>();
			//rowScript.playerIndex = i;
			this.playerRows.Add( rowScript );
		}
	}

	private void Update()
	{
		if ( this.playerListNeedsUpdating == true )
		{
			this.UpdatePlayerListGUI();
		}
	}

	public void Reset()
	{
		Debug.Log( "Resetting MenuLobby", this );
		this.playerDictionary.Clear();
		this.messages.Clear();

		playerListNeedsUpdating = true;
	}

	public void StartLobby()
	{
		this.gameNameText.text = MenuNetworking.instance.gameName;
		this.gameCommentText.text = MenuNetworking.instance.gameComment;
	}

	public void ExitLobby()
	{
		MenuNetworking.instance.QuitLobby();
		MainMenuButtons.instance.ExitLobby();
	}

	public void ReceiveTextMessage( NetworkViewID _viewID, string _message )
	{
		LobbyMessage message = new LobbyMessage();
		message.message = _message;

		message.playerName = _viewID.owner.ipAddress;
		message.timeReceived = DateTime.Now;
		message.sender = _viewID.owner;

		this.messages.Add( message );
		if ( this.messages.Count > this.messageLimit )
		{
			this.messages.RemoveAt( 0 );
		}

		this.UpdateMessageGUI();
	}

	private void UpdateMessageGUI()
	{
		string str = "";
		for ( int i = this.messages.Count - 1; i >= 0; --i )
		{
			LobbyMessage message = this.messages[i];

			int playerID = Common.NetworkID( message.sender );
			playerID = playerID % this.playerMessageColours.Length;

			string colour = this.playerMessageColours[ playerID ];

			str += "<color=" + colour + ">" + message.sender.ipAddress + " ("
				+ message.timeReceived + "): " + this.messages[i].message + "</color>\n";
			//TODO: Improve this significantly LM 30/4/14
		}
		this.messageText.text = str;
	}

	private void UpdatePlayerListGUI()
	{
		int index = 0;
		foreach ( KeyValuePair<int, PLAYER_TYPE> pair in this.playerDictionary )
		{
			this.playerRows[index].UpdateGUI( pair.Key, pair.Value );
			++index;
		} 

		for ( ; index < this.playerRows.Count; ++index )
		{
			this.playerRows[index].SetDefaults();
		}
		  
		this.playerListNeedsUpdating = false;
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

	public void AddPlayerOfType( int _playerID, PLAYER_TYPE _type )
	{
		Debug.Log( "Adding player " + _playerID + " to " + _type );
		if ( this.playerDictionary.ContainsKey( _playerID ) )
		{
			Debug.LogWarning( "Player " + _playerID + " already added", this );
		}

		this.playerDictionary.Add( _playerID, _type );
		switch ( _type )
		{
		case PLAYER_TYPE.COMMANDER1:
		{
			if ( this.commander1 != -1 )
			{
				Debug.LogWarning( "Commander role already set", this );
			}
			this.commander1 = _playerID;
			Debug.Log( "Set commander 1 to " + _playerID );
			break;
		}
		case PLAYER_TYPE.COMMANDER2:
		{
			if ( this.commander2 != -1 )
			{
				Debug.LogWarning( "Commander 2 rol already set", this );
			}
			this.commander2 = _playerID;
			Debug.Log( "Set commander 2 to " + _playerID );
			break;
		}
		case PLAYER_TYPE.FIGHTER1:
		{
			this.team1.Add( _playerID );
			break;
		}
		case PLAYER_TYPE.FIGHTER2:
		{
			this.team2.Add( _playerID );
			break;
		}
		default:
		{
			Debug.LogError( "Uncaught player type " + _type );
			break;
		}
		}

		this.playerListNeedsUpdating = true;
		Debug.Log( "Added player " + _playerID + " to type " + _type );
	}

	public void RemovePlayer( int _playerID )
	{
		PLAYER_TYPE type = this.playerDictionary[_playerID];
		if ( !this.playerDictionary.Remove( _playerID ) )
		{
			Debug.LogError( "Failed to remove player " + _playerID );
		}

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

		Debug.Log( "Removed player " + _playerID + " from " + type );
		this.playerListNeedsUpdating = true;
	}

	public void ChangePlayerType( int _playerID, PLAYER_TYPE _type )
	{
		this.RemovePlayer( _playerID );
		this.AddPlayerOfType( _playerID, _type );

		this.playerListNeedsUpdating = true;
	}

	public void CopyInformationIntoGameInfo( MenuToGameInfo _info )
	{
		if ( this.playerDictionary.ContainsKey( Common.MyNetworkID() ) )
		{
			_info.playerType = this.playerDictionary[ Common.MyNetworkID() ];
		}
		else
		{
			Debug.LogError( "Could not find player type in map " + Common.MyNetworkID() );
		}
	}

	/************************************************************
	 * BUTTON CALLBACKS */

	private void OnStartGameDown()
	{
		MenuNetworking.instance.StartGame();
	}

	private void OnExitLobbyDown()
	{
		MainMenuButtons.instance.ExitLobby();

		MenuNetworking.instance.DisconnectFromLobby();
	}

	private void OnSendMessageDown()
	{
		string text = this.messageTextField.text;
		if ( text == "" )
		{
			return;
		}

		MenuNetworking.instance.networkView.RPC
		(
			"SendLobbyMessageRPC",
			RPCMode.All,
			MenuNetworking.instance.networkView.viewID,
			text
		);

		this.messageTextField.GetComponent<GUITextField>().text = "";
	}
}
