using UnityEngine;
using System.Collections;

public abstract class BaseNetworkManager : MonoBehaviour
{
	protected abstract void OnDisconnectedFromServer( NetworkDisconnection _info );

	protected abstract void OnPlayerDisconnected( NetworkPlayer _player );

	public void SendLobbyMessage( string _message, bool _timestamped )
	{
		if ( Network.peerType != NetworkPeerType.Disconnected )
		{
			this.networkView.RPC( "OnSendLobbyMessageRPC", RPCMode.Others, Common.MyNetworkID(), _message, _timestamped );
		}
		this.OnSendLobbyMessageRPC( Common.MyNetworkID(), _message, _timestamped ); 
	}
	 
	[RPC]
	public void OnSendLobbyMessageRPC( int _playerID, string _message, bool _timestamped )
	{
		MessageManager.instance.AddMessage( _playerID, _message, true );
	}
}
