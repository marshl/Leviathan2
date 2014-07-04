using UnityEngine;
using System.Collections;

public class CapitalShipMaster : MonoBehaviour
{
	public GamePlayer owner;

	public CapitalShipMovement movement;
	public NetworkOwnerControl ownerControl;
	public CapitalShipTurretManager turrets;

	private bool ownerInitialised = false;

	private void OnNetworkInstantiate( NetworkMessageInfo _info )   
	{
		NetworkOwnerManager.instance.RegisterUnknownObject( this );
	}

	private void Update()
	{
		if ( this.ownerInitialised == false 
		    && this.ownerControl.ownerID != -1 )
		{
			this.OwnerInitialise();
		}
	}

	private void OwnerInitialise()
	{
		this.ownerInitialised = true;
		
		int playerID = this.ownerControl.ownerID;
		this.owner = GamePlayerManager.instance.GetPlayerWithID( playerID );
		if ( this.owner.capitalShip != null ) 
		{
			Debug.LogWarning( "Capital ship already set for " + playerID, this );
		}
		else
		{
			this.owner.capitalShip = this; 
			Debug.Log( "Set player " + playerID + " to own capital ship", this.gameObject ); 
		}

		if ( this.networkView.isMine == false )
		{
			this.enabled = false;
			this.movement.enabled = false;
		}  
		else
		{
			this.turrets.CreateTurrets();
		}
	}
}
