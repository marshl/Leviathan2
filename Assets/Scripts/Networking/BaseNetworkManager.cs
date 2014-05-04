using UnityEngine;
using System.Collections;

public abstract class BaseNetworkManager : MonoBehaviour
{
	protected abstract void OnDisconnectedFromServer( NetworkDisconnection _info );

	protected abstract void OnPlayerDisconnected( NetworkPlayer _player );

	public void SendLobbyMessage( string _message )
	{
		this.networkView.RPC( "OnSendLobbyMessageRPC", RPCMode.All, Common.MyNetworkID(), _message );
	}
	 
	[RPC]
	public void OnSendLobbyMessageRPC( int _playerID, string _message )
	{
		MessageManager.instance.AddMessage( _playerID, _message );
	}
}
