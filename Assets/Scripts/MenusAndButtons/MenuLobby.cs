﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum PLAYER_TYPE : int
{
	COMMANDER1,
	COMMANDER2,
	FIGHTER1,
	FIGHTER2,
};

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

	public MenuPlayerRow firstPlayerRow;
	public float playerRowOffset;
	
	public string[] playerMessageColours;

	// Private Variables
	private List<LobbyMessage> messages;
	private List<MenuPlayerRow> playerRows;
	
	public Dictionary<int, PLAYER_TYPE> playerDictionary;
	private int? commander1;
	private int? commander2;
	private List<int> team1;
	private List<int> team2;

	private void Awake ()
	{
		MenuLobby.instance = this;
		this.messages = new List<LobbyMessage>();
		this.playerRows = new List<MenuPlayerRow>();
		this.playerDictionary = new Dictionary<int, PLAYER_TYPE>();
		this.team1 = new List<int>();
		this.team2 = new List<int>();
	}

	private void Start()
	{

		for ( int i = 0; i < MenuNetworking.instance.connectionLimit; ++i )
		{
			GameObject rowObj = ( i == 0 ) ? this.firstPlayerRow.gameObject 
				: GameObject.Instantiate( this.firstPlayerRow.gameObject ) as GameObject;
			
			rowObj.transform.parent = this.firstPlayerRow.transform.parent;
			rowObj.transform.Translate( 0.0f, (float)i * this.playerRowOffset, 0.0f );
			rowObj.name = "PlayerRow" + i;
			MenuPlayerRow rowScript = rowObj.GetComponent<MenuPlayerRow>();
			rowScript.playerIndex = i;
			this.playerRows.Add( rowScript );
		}
	}

	private void Update()
	{
		this.UpdatePlayerListGUI();
	}

	public void Reset()
	{
		Debug.Log( "Resetting MenuLobby", this );
		this.playerDictionary.Clear();
		this.messages.Clear();

		for ( int i = 0; i < MenuNetworking.instance.connectionLimit; ++i )
		{
			this.playerRows[i].SetDefaults();
		}
	}

	public void StartLobby()
	{
		this.gameNameText.text = MenuNetworking.instance.gameName;
		this.gameCommentText.text = MenuNetworking.instance.gameComment;

		//this.Reset();
	}

	public void ExitLobby()
	{
		MenuNetworking.instance.QuitLobby();
		MainMenuButtons.instance.ExitLobby();
	}
	
	public void SendLobbyMessage( NetworkViewID _viewID, string _message )
	{
		LobbyMessage message = new LobbyMessage();
		message.message = _message;

		message.playerName = _viewID.owner.ipAddress;
		message.timeReceived = DateTime.Now;
		message.sender = _viewID.owner;
		
		
		this.messages.Add( message );
		this.messages.RemoveAt( this.messages.Count - 1 );

		
		this.UpdateMessageGUI();
	}

	private void UpdateMessageGUI()
	{
		string str = "";
		for ( int i = this.messages.Count - 1; i >= 0; --i )
		{
			LobbyMessage message = this.messages[i];
			str += message.sender.ipAddress + " ("
				+ message.timeReceived + "): " + this.messages[i].message + "\n";
		}
		this.messageText.text = str;
	}



	private void UpdatePlayerListGUI()
	{
		for ( int i = 0; i < this.playerRows.Count; ++i )
		{
			this.playerRows[i].UpdateGUI();
		}
	}
	
	public PLAYER_TYPE AddNewPlayer( int _playerID )
	{
		PLAYER_TYPE type;
		if ( _playerID == Common.NetworkID(Network.player) && Network.isServer )
		{
			type = PLAYER_TYPE.COMMANDER1;
		}
		else if ( this.team2.Count == 0 )
		{
			type = PLAYER_TYPE.COMMANDER2;
		}
		else if ( this.team1.Count > this.team2.Count )
		{
			type = PLAYER_TYPE.FIGHTER2;
		}
		else
		{
			type = PLAYER_TYPE.FIGHTER1;
		}
		this.AddPlayerOfType( _playerID, type );

		return type;
	}

	public void AddPlayerOfType( int _playerID, PLAYER_TYPE _type )
	{
		if ( this.playerDictionary.ContainsKey( _playerID ) )
		{
			Debug.LogWarning( "Player " + _playerID + " already added", this );
		}

		this.playerDictionary.Add( _playerID, _type );
		switch ( _type )
		{
		case PLAYER_TYPE.COMMANDER1:
			if ( this.commander1 != null )
			{
				Debug.LogWarning( "Commander role already set", this );
			}
			this.commander1 = _playerID;
			break;

		case PLAYER_TYPE.COMMANDER2:
			if ( this.commander2 != null )
			{
				Debug.LogWarning( "Commander 2 rol already set", this );
			}
			this.commander2 = _playerID;
			break;
		case PLAYER_TYPE.FIGHTER1:
			this.team1.Add( _playerID );
			break;
		case PLAYER_TYPE.FIGHTER2:
			this.team2.Add( _playerID );
			break;
		default:
			Debug.LogError( "Uncaught player type " + _type );
			break;
		}

		Debug.Log( "Adding player " + _playerID + " to type " + _type );
	}

	public void RemovePlayer( int _playerID )
	{
		PLAYER_TYPE type = this.playerDictionary[_playerID];
		this.playerDictionary.Remove( _playerID );
		Debug.Log( "Removing player of type " + type );
	}

	public void ChangePlayerType( int _playerID, PLAYER_TYPE _type )
	{
		this.RemovePlayer( _playerID );
		this.AddPlayerOfType( _playerID, _type );
		//this.playerDictionary[_playerID] = _type;
	}

	public void CopyInformation( MenuToGameInfo _info )
	{
		//_info.playerTypeMap = new Dictionary<int, PLAYER_TYPE>( this.playerDictionary);
		_info.playerTypeMap = new Dictionary<int, PLAYER_TYPE>();
		foreach ( var pair in this.playerDictionary )
		{
			_info.playerTypeMap.Add( pair.Key, pair.Value );
		}

		_info.Print();
	}

	/************************************************************
	 * CALLBACKS */

	private void OnStartGameDown()
	{
		MenuNetworking.instance.StartGame();
	}
	
	private void OnExitLobbyDown()
	{
		MainMenuButtons.instance.ExitLobby();
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
