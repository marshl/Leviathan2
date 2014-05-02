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

	//public bool messagesUpdated;

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

		//this.messagesUpdated = true;
	}

	public void Reset()
	{
		this.messages.Clear();
	}
	
}
