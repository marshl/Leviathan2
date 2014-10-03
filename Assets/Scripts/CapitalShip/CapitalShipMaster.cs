using UnityEngine;
using System.Collections;

public class CapitalShipMaster : MonoBehaviour
{
	public CapitalShipMovement movement;
	public NetworkOwnerControl ownerControl;
	public CapitalShipTurretManager turrets;
	public Transform depthControl;
	public CapitalHealth health;

	private bool ownerInitialised = false;

#if UNITY_EDITOR
	public bool dummyShip = false;

	private void Awake()
	{
		this.health.owner = GameNetworkManager.instance.lastCreatedDummy;
		this.health.ownerID = this.health.owner.id;
	}

	private void Start()
	{
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			if ( this.dummyShip )
			{
				//this.owner = GamePlayerManager.instance.commander1;
			}
			else
			{
				this.health.owner = GamePlayerManager.instance.myPlayer;
			}
			this.health.ownerID = GamePlayerManager.instance.myPlayer.id;
			this.health.owner.capitalShip = this;
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
		  && this.ownerControl.ownerID != null )
		{
			this.OwnerInitialise();
		}
	}

	private void OwnerInitialise()
	{
		this.ownerInitialised = true;
		
		int playerID = this.ownerControl.ownerID.Value;
		this.health.owner = GamePlayerManager.instance.GetPlayerWithID( playerID );
#if UNITY_EDITOR
		this.health.ownerID = playerID;
#endif

		if ( this.health.owner.capitalShip != null ) 
		{
			DebugConsole.Warning( "Capital ship already set for " + playerID, this );
		}
		else
		{
			this.health.owner.capitalShip = this; 
			DebugConsole.Log( "Set player " + playerID + " to own capital ship", this.gameObject ); 
		}
	
		if ( this.networkView.isMine )
		{
			this.turrets.CreateTurrets();
		}  
		else
		{
			this.enabled = false;
			this.movement.enabled = false;
		}

		//TODO: Colouring doesn't work with the new model yet
		foreach ( Renderer render in GetComponentsInChildren<Renderer>() )
		{
			if( render.gameObject.name == "Port" || 
			    render.gameObject.name == "Bow" ||
			    render.gameObject.name == "Starboard" )
			{
				if ( this.health.owner.team == TEAM.TEAM_1 )
					render.material.color = new Color(1.0f,1.0f,0.5f);
				else
					render.material.color = new Color(1,0f,0.2f,1.0f);
			}
		}
	}
}
