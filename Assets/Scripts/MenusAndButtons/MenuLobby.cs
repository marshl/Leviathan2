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

	public MenuPlayerRow firstPlayerRow;
	public float playerRowOffset;

	public int messageLimit;

	public string[] playerMessageColours;

	// Private Variables
	//private List<NetworkPlayer> networkPlayers;
	private List<LobbyMessage> messages;
	private List<MenuPlayerRow> playerRows;

	private void Awake ()
	{
		MenuLobby.instance = this;
		this.messages = new List<LobbyMessage>();
		this.playerRows = new List<MenuPlayerRow>( MenuNetworking.instance.connections );
		//this.networkPlayers = new List<NetworkPlayer>( MenuNetworking.instance.connections );

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
	
	public void SendLobbyMessage( NetworkViewID _viewID, string _message )
	{
		LobbyMessage message = new LobbyMessage();
		message.message = _message;
		//message.playerName = _sender.ipAddress;
		message.playerName = _viewID.owner.ipAddress;
		message.timeReceived = DateTime.Now;
		message.sender = _viewID.owner;
		
		
		this.messages.Add( message );
		if ( this.messages.Count > this.messageLimit )
		{
			this.messages.RemoveAt( this.messages.Count - 1 );
		}
		
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

	private void UpdatePlayerListGUI()
	{
		for ( int i = 0; i < this.playerRows.Count; ++i )
		{
			this.playerRows[i].UpdateGUI();
		}
	}

	private void OnStartGameDown()
	{
		MenuNetworking.instance.StartGame();
	}

	private void OnExitLobbyDown()
	{
		MainMenuButtons.instance.ExitLobby();
	}
}
