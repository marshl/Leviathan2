using UnityEngine;
using System.Collections;

public abstract class BaseNetworkManager : MonoBehaviour
{
	protected abstract void OnDisconnectedFromServer( NetworkDisconnection _info );

	protected abstract void OnPlayerDisconnected( NetworkPlayer _player );
}
