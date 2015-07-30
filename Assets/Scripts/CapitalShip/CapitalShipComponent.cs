using UnityEngine;
using System.Collections;

/// <summary>
/// When a component of the capital ship is Network.Instantiated, this script then attaches it to its
/// owner using a NetworkOwnerControl
/// </summary>
public class CapitalShipComponent : MonoBehaviour
{
	public NetworkOwnerControl ownerControl;
	public BaseHealth health;

	private bool ownerInitialised = false;

	private void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		NetworkOwnerManager.instance.RegisterUnknownObject( this );
	}

	private void Update()
	{
		if ( this.ownerInitialised == false 
		  && this.ownerControl.ownerID != null )
		{
			this.OwnerInitialise();
		}
	}

	private void OwnerInitialise()
	{
		this.ownerInitialised = true;
		
		int ownerID = this.ownerControl.ownerID.Value;
		this.health.Owner = GamePlayerManager.instance.GetPlayerWithID( ownerID );

		if ( this.health.Owner.capitalShip == null )
		{
			DebugConsole.Warning( "Owner (" + ownerID + ") does not have capital ship to attach to", this );
			return;
		}

		this.transform.parent = this.health.Owner.capitalShip.depthControl;
	
		if ( Network.peerType != NetworkPeerType.Disconnected && !this.GetComponent<NetworkView>().isMine )
		{
			this.enabled = false;
		}
	}
}
