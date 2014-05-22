using UnityEngine;
using System.Collections;

public class FighterMaster : MonoBehaviour 
{
	public GamePlayer owner;
	public Fighter fighter; //TODO: Rename Fighter.cs into FighterFlight or something LM:22/05/14
	public FighterHealth health;
	public FighterWeapons weapons; 

	// Unity Callback: Do not modify signature 
	private void OnNetworkInstantiate( NetworkMessageInfo _info )
	{
		int id = Common.NetworkID( this.networkView.owner );
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
