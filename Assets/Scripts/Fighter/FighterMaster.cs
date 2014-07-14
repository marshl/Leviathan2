using UnityEngine;
using System.Collections;

public class FighterMaster : MonoBehaviour 
{
	public enum FIGHTERSTATE
	{
		DOCKED,
		UNDOCKING,
		FLYING,
		DEAD
	};
	public FIGHTERSTATE state = FIGHTERSTATE.FLYING;

	public GamePlayer owner;
	public FighterMovement movement;
	public FighterHealth health;
	public FighterWeapons weapons; 
	public NetworkOwnerControl ownerControl;

	private bool ownerInitialised = false;
	
	public float undockingTimer = 0.0f;
	public float undockingDelay = 3.0f;
	public float undockingLaunchSpeed = 100.0f;
	
	public float respawnDelay = 5.0f;
	
	public float respawnTimer = 0.0f;
	
	public DockingBay.DockingSlot currentSlot;

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

		if ( this.networkView.isMine )
		{
			if ( Input.GetKeyDown( KeyCode.G ) )
			{
				this.Respawn();
			}

			if ( Input.GetKeyDown( KeyCode.LeftBracket ) )
			{
				this.Die( 0 );
			}

			switch ( this.state )
			{
			case FighterMaster.FIGHTERSTATE.DOCKED:
			{
				if ( Input.GetKeyDown( KeyCode.Space ) 
				  && !GameMessages.instance.typing )
				{
					DebugConsole.Log( "Space was pressed", this );
					this.Undock();
				}
				break;
			}
			case FighterMaster.FIGHTERSTATE.UNDOCKING:
			{
				if ( this.undockingTimer > 0 )
				{
					this.undockingTimer -= Time.deltaTime;
					if ( this.undockingTimer < 0 )
					{
						this.undockingTimer = 0;
						this.state = FighterMaster.FIGHTERSTATE.FLYING;
					}
				}
				break;
			}
			case FIGHTERSTATE.DEAD:
			{
				if ( this.respawnTimer > 0 )
				{
					this.respawnTimer -= Time.deltaTime;
				}
				else
				{
					this.respawnTimer = 0.0f;
					if ( Input.GetKeyDown (KeyCode.Space) )
					{
						this.Respawn();
					}
				}
				break;
			}
			default:
			{
				break;
			}
			}
		}
	}

	private void OwnerInitialise()
	{
		this.ownerInitialised = true;

		int id = this.ownerControl.ownerID;
		this.owner = GamePlayerManager.instance.GetPlayerWithID( id );
		this.owner.fighter = this;
		DebugConsole.Log( "Set player " + id + " to own fighter", this.gameObject );
		if ( this.networkView.isMine == false )
		{
			this.enabled = false;
			this.movement.enabled = false;
			this.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		} 

		//TODO: Shouls team be removed from health in favour of GamePlayer pointer? LM:7/7/14
		this.health.team = this.owner.team;

		
		foreach (Renderer render in GetComponentsInChildren<Renderer>())
		{
			if(owner.team == TEAM.TEAM_1)
				render.material.color = new Color(1.0f,1.0f,0.2f);
			else
				render.material.color = new Color(1,0f,0.2f,1.0f);
		}
	}

	public void Respawn()
	{
		//Find an empty spot on the friendly capital ship and dock there
		
		DockingBay[] bays = FindObjectsOfType( typeof(DockingBay) ) as DockingBay[];

		if ( bays.Length == 0 )
		{
			DebugConsole.Warning( "No docking bays found", this );
			return;
		}

		int index = (int)Random.Range( 0, bays.Length );
		int startingIndex = index;

		while ( true )
		{
			if ( index == bays.Length )
			{
				index = 0;
			}
			
			if( bays[index].capitalShip.health.team == this.health.team )
			{
				this.health.FullHeal();
				GameNetworkManager.instance.SendRespawnedFighterMessage( this.networkView.viewID );
				this.Dock( bays[index].GetFreeSlot() );
				break;
			}
			++index;
			if ( index == startingIndex )
			{
				DebugConsole.Warning("Could not find an empty docking bay (somehow)");
				break;
			}
		}
	}
	
	public void Die( int explosionType )
	{
		respawnTimer = respawnDelay; 
		
		this.state = FIGHTERSTATE.DEAD;
		GameNetworkManager.instance.SendDeadFighterMessage( this.networkView.viewID );
	}

	public void Dock( DockingBay.DockingSlot _slot )
	{
		if ( state == FIGHTERSTATE.UNDOCKING )
		{
			DebugConsole.Log( "Skipping dock", this );
			return;
		}
		
		DebugConsole.Log( "Proceeding with dock", this );

		this.movement.desiredSpeed = 0;
		this.rigidbody.velocity = Vector3.zero;
		this.rigidbody.angularVelocity = Vector3.zero;
		this.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		this.transform.position = _slot.landedPosition.position;
		this.transform.rotation = _slot.landedPosition.rotation;

		this.currentSlot = _slot;
		this.state = FIGHTERSTATE.DOCKED;
		this.transform.parent = _slot.landedPosition;

		GameNetworkManager.instance.SendDockedMessage( this.networkView.viewID, _slot.slotID );
		this.GetComponent<FighterWeapons>().enabled = false;
	}

	public void Undock()
	{
		DebugConsole.Log( "Undocking", this );
		this.rigidbody.constraints = RigidbodyConstraints.None;
		
		Vector3 inheritedVelocity = Vector3.zero;
		if(this.GetComponent<NetworkPositionControl>() != null)
		{
			inheritedVelocity = this.GetComponent<NetworkPositionControl>().CalculateVelocity();
			inheritedVelocity += this.transform.forward * undockingLaunchSpeed;
		}

		this.state = FIGHTERSTATE.UNDOCKING;
		this.currentSlot.occupied = false;
		this.currentSlot.landedFighter = null;

		this.movement.desiredSpeed = ( this.movement.maxSpeed * 0.75f );
		this.undockingTimer = this.undockingDelay;
		
		this.transform.parent = null;
		this.transform.localScale = Vector3.one;
		
		//Apply force of the capital ship so we don't move relative
		GameNetworkManager.instance.SendUndockedMessage ( this.networkView.viewID, this.currentSlot.slotID );
		this.currentSlot = null;
		this.weapons.enabled = true;
		
		if ( inheritedVelocity == Vector3.zero )
		{
			inheritedVelocity.x = 10;
			inheritedVelocity.y = 0;
			inheritedVelocity.z = 10;
		}
		
		DebugConsole.Log( "Inherited Velocity: " + inheritedVelocity, this );

		this.rigidbody.AddForce( inheritedVelocity * 95 );
	}
	
	private void OnGUI()
	{
		if ( this.state == FIGHTERSTATE.DEAD )
		{
			if(respawnTimer > 0)
			{
				GUI.Label (new Rect((Screen.width / 2) - 200, Screen.height / 2, 300, 50), "Respawn available in " + respawnTimer + " seconds");
			}
			else
			{
				GUI.Label (new Rect((Screen.width / 2) - 200, Screen.height / 2, 300, 50), "Press Space to respawn");
			}
		}
	}
}
