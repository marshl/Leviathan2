using UnityEngine;
using System.Collections;

public abstract class BaseNetworkManager : MonoBehaviour
{
	public static BaseNetworkManager baseInstance;

	protected abstract void OnDisconnectedFromServer( NetworkDisconnection _info );

	protected abstract void OnPlayerDisconnected( NetworkPlayer _player );

	public void SendLobbyMessage( string _message, NetworkPlayer _receiver, MESSAGE_TYPE _messageType )
	{
		this.networkView.RPC( "OnSendLobbyMessageRPC", _receiver, Common.MyNetworkID(), _message, (int)_messageType );
	}
	 
	[RPC]
	private void OnSendLobbyMessageRPC( int _playerID, string _message, int _messageType )
	{
		MessageManager.instance.CreateMessageNetworked( _playerID, _message, (MESSAGE_TYPE)_messageType );
	}
}
