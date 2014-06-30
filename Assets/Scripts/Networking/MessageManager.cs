using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Message
{
	public int senderID;
	public int receiverID;
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

	public void AddMessage( int _playerID, int _receiverID, string _message )
	{
		// If this message isn't meant for me, don't add it
		if ( _receiverID != -1 && _receiverID != Common.MyNetworkID() )
		{
			return;
		}

		//TODO: This isn't escaping right, revise LM 26/05/14
		//_message = _message.Replace( "<", "&lt;" );
		//_message = _message.Replace( ">", "&gt;" ); 
		_message = _message.Replace( ">", "" ).Replace( "<", "" );

		Message msg = new Message();
		msg.senderID = _playerID;
		msg.message = _message;
		msg.timeReceived = DateTime.Now;
		msg.receiverID = _receiverID;

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
