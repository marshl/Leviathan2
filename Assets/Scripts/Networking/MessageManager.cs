/// <summary>
/// Message.
/// 
/// "GLA postal service!" - Bomb truck
/// </summary>


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum MESSAGE_TYPE
{
	TO_ALL,
	TO_TEAM,
	PRIVATE,
	LOCAL,
}

public class Message
{
	public int senderID;
	public string message;
	public DateTime timeReceived;
	public MESSAGE_TYPE messageType;
};

public class MessageManager : MonoBehaviour
{
	public static MessageManager instance;

	public List<Message> messages;

	public Color[] playerColours;

	public bool newMessages = false;

	private void Awake()
	{
		MessageManager.instance = this;
		 
		this.messages = new List<Message>();
	}
	
	public void CreateMessageLocal( string _message, MESSAGE_TYPE _messageType, int? _receiverID = null )
	{
		DebugConsole.Log( "Creating message \"" + _message + "\"" );
		_message = _message.Replace( ">", "\\>" ).Replace( "<", "\\<" );

		if ( Network.peerType != NetworkPeerType.Disconnected )
		{
			if ( _messageType == MESSAGE_TYPE.PRIVATE )
			{
				if ( _receiverID == null )
				{
					throw new Exception( "Receiver ID required for private messages." );
				}
				DebugConsole.Log( "Sending private message to " + _receiverID.Value );
				GamePlayer receiver = GamePlayerManager.instance.GetPlayerWithID( _receiverID.Value );
				BaseNetworkManager.baseInstance.SendLobbyMessage( _message, receiver.networkPlayer, _messageType );
			}
			else if ( _messageType == MESSAGE_TYPE.TO_ALL || _messageType == MESSAGE_TYPE.TO_TEAM )
			{
				foreach ( KeyValuePair<int, GamePlayer> pair in GamePlayerManager.instance.playerMap )
				{
					if ( pair.Value == GamePlayerManager.instance.myPlayer )
					{
						continue;
					}
					if ( !pair.Value.sendMessagesTo )
					{
						DebugConsole.Log( "Ignoring player on ignore list " + pair.Key );
						continue;
					}
				
					if ( _messageType == MESSAGE_TYPE.TO_TEAM
					  && pair.Value.team != GamePlayerManager.instance.myPlayer.team )
					{
						DebugConsole.Log( "Ignoring player on opposing team " + pair.Key );
						continue;
					}
					DebugConsole.Log( "Sending to " + pair.Key );
					BaseNetworkManager.baseInstance.SendLobbyMessage( _message, pair.Value.networkPlayer, _messageType );
				}
			}
		}

		// Send to myself
		this.CreateMessageNetworked( Common.MyNetworkID(), _message, _messageType );
	}

	public void CreateMessageNetworked( int _senderID, string _message, MESSAGE_TYPE _messageType )
	{
		DebugConsole.Log( "Received message \"" + _senderID + "\" from " + _senderID );
		GamePlayer sender = GamePlayerManager.instance.GetPlayerWithID( _senderID );
		if ( !sender.receiveMessagesFrom )
		{
			DebugConsole.Log( "Ignoring message" );
			return;
		}

		Message msg = new Message();
		msg.senderID = _senderID;
		msg.message = _message;
		msg.timeReceived = DateTime.Now;
		msg.messageType = _messageType;
		this.messages.Add( msg );

		if ( _senderID != Common.MyNetworkID() )
		{
			this.newMessages = true;
		}
	}

	public string GetFormattedMessage( int _messageIndex )
	{
		if ( _messageIndex < 0 && _messageIndex >= this.messages.Count )
		{
			DebugConsole.Error( "bad message index " + _messageIndex );
			return "ERROR";
		}

		Message msg = this.messages[_messageIndex];

		return msg.timeReceived.ToString( "hh:mm tt" ) + " - "
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
