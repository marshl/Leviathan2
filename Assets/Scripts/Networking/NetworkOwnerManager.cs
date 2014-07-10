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
			DebugConsole.Warning( "Cannot add NetworkOwnerControl without a NetworkView (" + _obj.gameObject.name + ")", _obj );
			return;
		}
	
		NetworkView view = _obj.networkView;
		NetworkViewID id = view.viewID;

		if ( this.map.ContainsKey( _obj.networkView.viewID ) )
		{
			DebugConsole.Warning( "NetworkViewID " + id + " aleady exists as " + _obj.gameObject.name,
			                 this.map[id].gameObject );
			DebugConsole.Warning( "Cannot set " + id + " to " + _obj.gameObject.name, _obj.gameObject );
			return;
		}

		if ( view.isMine ) // If it is mine, tell everyone else about it
		{
			int ownerID = Common.MyNetworkID();
			GameNetworkManager.instance.SendSetViewIDMessage( ownerID, id );
			_obj.GetComponent<NetworkOwnerControl>().ownerID = ownerID;
		}
		else // If it isn't mine, store this away and wait for the RPC
		{
			this.map.Add( id, view );
		}
	}
	
	public void ReceiveSetViewID( int _ownerID, NetworkViewID _id )
	{
		if ( this.map.ContainsKey( _id ) == false )
		{
			DebugConsole.Error( "Cannot find NetworkView with ID " + _id );
			return;
		}
		NetworkView view = this.map[_id];
		NetworkOwnerControl ownerControl = view.GetComponent<NetworkOwnerControl>();
		if ( ownerControl == null )
		{
			DebugConsole.Error ( "Cannot find NetworkOwnerControl on " + view.gameObject.name, view.gameObject );
			return;
		}

		this.map.Remove( _id );
		ownerControl.ownerID = _ownerID;
	}
}
