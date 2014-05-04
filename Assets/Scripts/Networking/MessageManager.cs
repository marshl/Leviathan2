using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Message
{
	public int senderID;
	public int receiverID; // TODO: Stuff with this
	public string message;
	public DateTime timeReceived;


};

public class MessageManager : MonoBehaviour
{
	public static MessageManager instance;

	public List<Message> messages;

	private string[] playerMessageColours = { "#FFFFFFFF", "#FF00FFFF", "#FFFF00FF", "#00FFFFFF" };

	private void Awake()
	{
		MessageManager.instance = this;
		 
		this.messages = new List<Message>();
	}

	public void AddMessage( int _playerID, string _message )
	{
		Message msg = new Message();
		msg.senderID = _playerID;
		msg.message = _message;
		msg.timeReceived = DateTime.Now;

		this.messages.Add( msg );
	}

	public string GetFormattedMessage( int _messageIndex )
	{
		if ( _messageIndex < 0 && _messageIndex >= this.messages.Count )
		{
			Debug.LogError( "bad message index " + _messageIndex );
			return "ERROR";
		}

		Message msg = this.messages[_messageIndex];
		int colourID = msg.senderID > 0 ? msg.senderID % this.playerMessageColours.Length : 0;
		string colour = this.playerMessageColours[ colourID ];
		
		return "<color=" + colour + "> Player " + msg.senderID + " ("
			+ msg.timeReceived + "): " + msg.message + "</color>\n";
	}

	public void Reset()
	{
		this.messages.Clear();
	}
	
}
