using UnityEngine;
using System.Collections;

public abstract class BaseNetworkManager : MonoBehaviour
{
	protected void Awake()
	{

	}

	protected void Start()
	{

	}

	protected void Update()
	{

	}

	//protected abstract void OnConnectedToServer();
	    
	protected abstract void OnDisconnectedFromServer( NetworkDisconnection _info );

	//protected abstract void OnFailedToConnect( NetworkConnectionError _error );

	//protected abstract void OnFailedToConnectToMasterServer( NetworkConnectionError _error );

	//protected abstract void OnMasterServerEvent( MasterServerEvent _event );

	//protected abstract void OnPlayerConnected( NetworkPlayer _player );

	protected abstract void OnPlayerDisconnected( NetworkPlayer _player );

	//protected abstract void OnServerInitialized();
}
