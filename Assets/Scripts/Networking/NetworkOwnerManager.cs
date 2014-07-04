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
	public void RegisterUnknownObject( MonoBehaviour _obj )
	{
		if ( _obj.networkView == null )
		{
			Debug.LogWarning( "Cannot add NetworkOwnerControl without a NetworkView (" + _obj.gameObject.name + ")", _obj );
			return;
		}
	
		NetworkView view = _obj.networkView;
		NetworkViewID id = view.viewID;

		if ( this.map.ContainsKey( _obj.networkView.viewID ) )
		{
			Debug.LogWarning( "NetworkViewID " + id + " aleady exists as " + _obj.gameObject.name,
			                 this.map[id].gameObject );
			Debug.LogWarning( "Cannot set " + id + " to " + _obj.gameObject.name, _obj.gameObject );
			return;
		}

		if ( view.isMine ) // If it is mine, tell everyone else about it
		{
			NetworkViewID newID = Network.AllocateViewID();
			int ownerID = Common.MyNetworkID();
			GameNetworkManager.instance.SendSetViewIDMessage( ownerID, id, newID );
			Debug.Log( "Changing view ID for " + _obj.gameObject.name + " from " + view.viewID + " to " + newID, view );
			view.viewID = newID;

			_obj.GetComponent<NetworkOwnerControl>().ownerID = ownerID;
		}
		else // If it isn't mine, store this away and wait for the RPC
		{
			Debug.Log( "Network view for " + _obj.gameObject.name + " registered as " + id, _obj );
			this.map.Add( id, view );
		}
	}
	
	public void ReceiveSetViewID( int _ownerID, NetworkViewID _oldID, NetworkViewID _newID )
	{
		if ( this.map.ContainsKey( _oldID ) == false )
		{
			Debug.LogError( "Cannot find NetworkView with ID " + _oldID );
			return;
		}
		NetworkView view = this.map[_oldID];

		int oldOwner = Common.NetworkID( view.owner );
		Debug.Log( "Received change notification for " + view.gameObject.name + " from " + _oldID + " to " + _newID, view );
		Debug.Log( "Transferring ownership from " + oldOwner + " to " + _ownerID );
		view.viewID = _newID;
		this.map.Remove( _oldID );

		NetworkOwnerControl ownerControl = view.GetComponent<NetworkOwnerControl>();
		if ( ownerControl != null )
		{
			ownerControl.ownerID = _ownerID;
		}
	}
}
