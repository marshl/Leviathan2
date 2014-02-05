using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum PLAYER_TYPE
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
	
	private Dictionary<NetworkPlayer, PLAYER_TYPE> playerDictionary;
	private NetworkPlayer commander1;
	private NetworkPlayer commander2;

	private void Awake ()
	{
		MenuLobby.instance = this;
		this.messages = new List<LobbyMessage>();
		this.playerRows = new List<MenuPlayerRow>( MenuNetworking.instance.connections );

		this.playerDictionary = new Dictionary<NetworkPlayer, PLAYER_TYPE>();

		for ( int i = 0; i < MenuNetworking.instance.connections; ++i )
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
		this.playerDictionary.Clear();
		this.messages.Clear();

		for ( int i = 0; i < MenuNetworking.instance.connections; ++i )
		{
			this.playerRows[i].SetDefaults();
		}
	}

	public void StartLobby()
	{
		this.gameNameText.text = MenuNetworking.instance.gameName;
		this.gameCommentText.text = MenuNetworking.instance.gameComment;

		this.Reset();
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
	
	public void AddNewPlayer( NetworkPlayer _player )
	{
		this.playerDictionary.Add( _player, 0 );
	}

	public void RemovePlayer( NetworkPlayer _player )
	{
		this.playerDictionary.Remove( _player );
	}

	public void ChangePlayerType( NetworkPlayer _player, PLAYER_TYPE _type )
	{
		this.playerDictionary[_player] = _type;
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
