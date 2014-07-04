using UnityEngine;
using System.Collections;

public class FighterMaster : MonoBehaviour 
{
	public GamePlayer owner;
	public Fighter fighter; //TODO: Rename Fighter.cs into FighterFlight or something LM:22/05/14
	public FighterHealth health;
	public FighterWeapons weapons; 
	public NetworkOwnerControl ownerControl;

	private bool ownerInitialised = false;

	// Unity Callback: Do not modify signature 
	private void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		NetworkOwnerManager.instance.RegisterUnknownObject( this );

		if ( this.ownerInitialised == false 
		    && this.ownerControl.ownerID != -1 )
		{
			this.OwnerInitialise();
		}
	}

	private void OwnerInitialise()
	{
		this.ownerInitialised = true;

		int id = this.ownerControl.ownerID;
		this.owner = GamePlayerManager.instance.GetPlayerWithID( id );
		this.owner.fighter = this;
		Debug.Log( "Set player " + id + " to own fighter", this.gameObject );
		if ( this.networkView.isMine == false )
		{
			this.enabled = false;
			this.fighter.enabled = false;
			this.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		} 
	}
}
