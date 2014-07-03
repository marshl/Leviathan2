using UnityEngine;
using System.Collections;

public class CapitalShipMaster : MonoBehaviour
{
	public GamePlayer owner;

	public CapitalShipMovement movement;
	public CapitalShipTurretManager turrets;

	private void OnNetworkInstantiate( NetworkMessageInfo _info )   
	{
		NetworkOwnerManager.instance.RegisterUnknownObject( this.networkView );

		Debug.Log( "CapitalShipMovement:OnNetworkInstantiate " + this.networkView.owner + " " + _info.timestamp, this );
		int playerID = Common.NetworkID( this.networkView.owner );
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
	}
}
