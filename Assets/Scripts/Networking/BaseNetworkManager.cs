using UnityEngine;
using System.Collections;

public abstract class BaseNetworkManager : MonoBehaviour
{
	protected abstract void OnDisconnectedFromServer( NetworkDisconnection _info );

	protected abstract void OnPlayerDisconnected( NetworkPlayer _player );

	public void SendLobbyMessage( string _message, int _receiverID )
	{
		if ( Network.peerType != NetworkPeerType.Disconnected )
		{
			this.networkView.RPC( "OnSendLobbyMessageRPC", RPCMode.Others, Common.MyNetworkID(), _receiverID, _message );
		}
		this.OnSendLobbyMessageRPC( Common.MyNetworkID(), _receiverID, _message ); 
	}
	 
	[RPC]
	public void OnSendLobbyMessageRPC( int _playerID, int _receiverID, string _message )
	{
		MessageManager.instance.AddMessage( _playerID, _receiverID, _message );
	}
}
