using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MenuLobby : MonoBehaviour
{
	// Instance
	public static MenuLobby instance;

	// Private Structures
	/*private class LobbyMessage
	{
		public NetworkPlayer sender;
		public string message;
		public string playerName;
		public DateTime timeReceived;
	};*/

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
	//private List<LobbyMessage> messages;
	private List<MenuPlayerRow> playerRows;

	//public Dictionary<int, PLAYER_TYPE> playerDictionary;

	private void Awake ()
	{
		MenuLobby.instance = this;
		//this.messages = new List<LobbyMessage>();
		this.playerRows = new List<MenuPlayerRow>();
		//this.playerDictionary = new Dictionary<int, PLAYER_TYPE>();
		//this.team1 = new List<int>();
		//this.team2 = new List<int>();
		//this.commander1 = this.commander2 = -1;
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
		if ( MainMenuButtons.instance.currentState == MainMenuButtons.STATE.LOBBY )
		{
			this.UpdatePlayerListGUI();
			this.UpdateMessageGUI();
		}
	}

	public void Reset()
	{
		Debug.Log( "Resetting MenuLobby", this );
		//this.playerDictionary.Clear();
		//this.messages.Clear();

		GamePlayerManager.instance.Reset();
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

	/*public void ReceiveTextMessage( NetworkViewID _viewID, string _message )
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
	}*/

	private void UpdateMessageGUI()
	{
		string str = "";
		//for ( int i = this.messages.Count - 1; i >= 0; --i )
		for ( int i = MessageManager.instance.messages.Count - 1; i >= 0; --i )
		{
			//LobbyMessage message = this.messages[i];
			Message msg = MessageManager.instance.messages[i];

			//GamePlayer player = GamePlayerManager.instance.GetPlayerWithID( msg.senderID );

			//int playerID = Common.NetworkID( message.sender );
			//playerID = playerID % this.playerMessageColours.Length;
			int colourID = msg.senderID % this.playerMessageColours.Length;
			string colour = this.playerMessageColours[ colourID ];

			str += "<color=" + colour + "> Player " + msg.senderID + " ("
				+ msg.timeReceived + "): " + msg.message + "</color>\n";
			//TODO: Improve this significantly LM 30/4/14
			//TODO: Escape formatting tags LM 02/05/14
		}
		this.messageText.text = str;
	}

	private void UpdatePlayerListGUI()
	{
		int index = 0;
		//foreach ( KeyValuePair<int, PLAYER_TYPE> pair in this.playerDictionary )
		foreach ( KeyValuePair<int, GamePlayer> pair in GamePlayerManager.instance.playerMap )
		{
			this.playerRows[index].UpdateGUI( pair.Value );
			++index;
		} 

		for ( ; index < this.playerRows.Count; ++index )
		{
			this.playerRows[index].SetDefaults();
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

		MenuNetworking.instance.networkView.RPC (
			"SendLobbyMessageRPC",
			RPCMode.All,
			Common.MyNetworkID(),
			//MenuNetworking.instance.networkView.viewID,
			text
		);

		this.messageTextField.GetComponent<GUITextField>().text = "";
	}
}
