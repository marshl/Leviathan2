using UnityEngine;
using System.Collections;

public class CapitalShipMaster : MonoBehaviour
{
	public CapitalShipMovement capitalShipMovement;
	public CapitalShipTurretManager turretManager;

	private void OnNetworkInstantiate( NetworkMessageInfo _info )   
	{
		Debug.Log( "CapitalShipMovement:OnNetworkInstantiate " + _info.sender + " " + _info.timestamp, this );
		int playerID = Common.NetworkID( this.networkView.owner );
		GamePlayer owner = GamePlayerManager.instance.GetPlayerWithID( playerID );
		if ( owner.capitalShip != null ) 
		{
			Debug.LogWarning( "Capital ship already set for " + playerID, this );
		}
		else
		{
			owner.capitalShip = this; 
			Debug.Log( "Set player " + playerID + " to own capital ship", this.gameObject ); 
		}
		
		if ( this.networkView.isMine == false )
		{
			this.enabled = false;
		} 
	}
}
