using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MenuLobby : MonoBehaviour
{
	public static MenuLobby instance;

	public GUIText gameNameText;
	public GUIText gameCommentText;

	private class LobbyMessage
	{
		public string message;
		public string playerName;
		public DateTime timeReceived;
	};

	private List<LobbyMessage> messages;

	private void Awake ()
	{
		MenuLobby.instance = this;
		this.messages = new List<LobbyMessage>();
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

	private void SendLobbyMessage( string _message, NetworkPlayer _sender )
	{
		LobbyMessage message = new LobbyMessage();
		message.message = _message;
		message.playerName = _sender.ipAddress;
		message.timeReceived = DateTime.Now;

		this.messages.Add( message );
	}
}
