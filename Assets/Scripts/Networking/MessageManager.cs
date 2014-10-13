/// <summary>
/// Message.
/// 
/// "GLA postal service!" - Bomb truck
/// </summary>


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Message
{
	public int senderID;
	public string message;
	public DateTime timeReceived;
	public bool showTimestamp;
};

public class MessageManager : MonoBehaviour
{
	public static MessageManager instance;

	public List<Message> messages;

	public Color[] playerColours;

	private void Awake()
	{
		MessageManager.instance = this;
		 
		this.messages = new List<Message>();
	}

	public void AddMessage( int _playerID, string _message, bool _showTimestamp )
	{
		_message = _message.Replace( ">", "" ).Replace( "<", "" );

		Message msg = new Message();
		msg.senderID = _playerID;
		msg.message = _message;
		msg.timeReceived = DateTime.Now;
		msg.showTimestamp = _showTimestamp;

		this.messages.Add( msg );
	}

	public string GetFormattedMessage( int _messageIndex )
	{
		if ( _messageIndex < 0 && _messageIndex >= this.messages.Count )
		{
			DebugConsole.Error( "bad message index " + _messageIndex );
			return "ERROR";
		}

		Message msg = this.messages[_messageIndex];

		return (msg.showTimestamp ? msg.timeReceived.ToString( "hh:mm tt" ) + " - " : "")
			+ "<color=" + Common.ColorToHex( this.GetPlayerColour( msg.senderID ) ) + ">"
			+ "Player " + msg.senderID + ": "
			+ msg.message + "</color>\n";
	}

	public void Reset()
	{
		this.messages.Clear();
	}

	public Color GetPlayerColour( int _index )
	{
		int colourID = _index > 0 ? _index % this.playerColours.Length : 0;
		return this.playerColours[ colourID ];
	}
}
