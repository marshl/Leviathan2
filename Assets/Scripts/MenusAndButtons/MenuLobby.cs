using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MenuLobby : MonoBehaviour
{
	// Instance
	public static MenuLobby instance;

	// Editor Variables
	public GUIText gameNameText;
	public GUIText gameCommentText;
	public GUITextField messageTextField;
	public GUIText messageText;
	public int messageLimit;

	public MenuPlayerRow firstPlayerRow;
	public float playerRowOffset;

	private List<MenuPlayerRow> playerRows;


	private void Awake ()
	{
		MenuLobby.instance = this;
		this.playerRows = new List<MenuPlayerRow>();
	}

	private void Start()
	{
		// Create a player row for each possible player
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
		this.Reset();
	}

	private void UpdateMessageGUI()
	{
		string str = "";

		for ( int i = MessageManager.instance.messages.Count - 1; i >= 0; --i )
		{
			str += MessageManager.instance.GetFormattedMessage( i );
		}
		this.messageText.text = str;
	}

	private void UpdatePlayerListGUI()
	{
		int index = 0;

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

		MenuNetworking.instance.SendLobbyMessage( text, -1 );

		this.messageTextField.GetComponent<GUITextField>().text = "";
	}
}
