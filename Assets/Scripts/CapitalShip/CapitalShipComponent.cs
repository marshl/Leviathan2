using UnityEngine;
using System.Collections;

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
		this.health.owner = GamePlayerManager.instance.GetPlayerWithID( ownerID );
#if UNITY_EDITOR
		this.health.ownerID = ownerID;
#endif

		if ( this.health.owner.capitalShip == null )
		{
			DebugConsole.Warning( "Owner (" + ownerID + ") does not have capital ship to attach to", this );
		}
		else
		{
			this.ParentToOwnerShip( this.health.owner );
		}
	
		if ( !this.networkView.isMine )
		{
			this.enabled = false;
		}
	}

	private void ParentToOwnerShip( GamePlayer _player )
	{
		this.transform.parent = _player.capitalShip.depthControl;
	}
}
