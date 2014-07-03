using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkOwnerManager : MonoBehaviour
{
	public static NetworkOwnerManager instance;

	private void Awake()
	{
		NetworkOwnerManager.instance = this;

		this.map = new Dictionary<NetworkViewID, NetworkView>();
	}

	private Dictionary<NetworkViewID, NetworkView> map;

	// Called by OnNetworkInstantiate
	public void RegisterUnknownObject( NetworkView _view )
	{
		if ( this.map.ContainsKey( _view.viewID ) )
		{
			Debug.LogWarning( "NetworkViewID " + _view.viewID + " aleady exists as " + this.map[_view.viewID].gameObject.name,
			                 this.map[_view.viewID].gameObject );
			Debug.LogWarning( "Cannot set " + _view.viewID + " to " + _view.gameObject.name, _view.gameObject );
			return;
		}

		if ( _view.isMine )
		{
			NetworkViewID newID = Network.AllocateViewID();
			GameNetworkManager.instance.SendSetViewIDMessage( _view.viewID, newID );
			_view.viewID = newID;
			Debug.Log( "Changing view ID for " + _view.gameObject.name + " from " + _view.viewID + " to " + newID, _view );
		}
		else
		{
			Debug.Log( "Network view for " + _view.gameObject.name + " registered as " + _view.viewID, _view.gameObject );
			this.map.Add( _view.viewID, _view );
		}
	}
	
	public void ReceiveSetViewID( NetworkViewID _oldID, NetworkViewID _newID )
	{
		if ( this.map.ContainsKey( _oldID ) == false )
		{
			Debug.LogError( "Cannot find NetworkView with ID " + _oldID );
			return;
		}
		int oldOwner = Common.NetworkID( this.map[_oldID].owner );
		Debug.Log( "Received change notification for " + this.map[_oldID].gameObject.name + " from " + _oldID + " to " + _newID, this.map[_oldID] );
		Debug.Log( "Transferring ownership from " + oldOwner + " to " + Common.NetworkID( this.map[_oldID].owner ) );
		this.map[_oldID].viewID = _newID;
		this.map.Remove( _oldID );


	}
}
