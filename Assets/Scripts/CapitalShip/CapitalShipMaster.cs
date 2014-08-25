using UnityEngine;
using System.Collections;

public class CapitalShipMaster : MonoBehaviour
{
	public GamePlayer owner;

	public CapitalShipMovement movement;
	public NetworkOwnerControl ownerControl;
	public CapitalShipTurretManager turrets;
	public Transform depthControl;
	public CapitalHealth health;

	private bool ownerInitialised = false;

#if UNITY_EDITOR
	public bool dummyShip = false;

	protected void Start()
	{
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			if ( this.dummyShip )
			{
				this.owner = GamePlayerManager.instance.commander1;
			}
			else
			{
				this.owner = GamePlayerManager.instance.myPlayer;
			}

			this.health.team = this.owner.team;
			this.owner.capitalShip = this;

			this.turrets.CreateTurrets();
		}
	}
#endif

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
			DebugConsole.Warning( "Capital ship already set for " + playerID, this );
		}
		else
		{
			this.owner.capitalShip = this; 
			DebugConsole.Log( "Set player " + playerID + " to own capital ship", this.gameObject ); 
		}

		this.health.team = this.owner.team;

		if ( this.networkView.isMine )
		{
			this.turrets.CreateTurrets();
		}  
		else
		{
			this.enabled = false;
			this.movement.enabled = false;
		}

		
		foreach (Renderer render in GetComponentsInChildren<Renderer>())
		{
			if(render.gameObject.name == "Port" || 
			   render.gameObject.name == "Bow" ||
			   render.gameObject.name == "Starboard" )
			{
				if(owner.team == TEAM.TEAM_1)
					render.material.color = new Color(1.0f,1.0f,0.5f);
				else
					render.material.color = new Color(1,0f,0.2f,1.0f);
			}
		}
	}
}
