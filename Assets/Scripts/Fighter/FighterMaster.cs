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

	public CapitalShipMaster capitalShip;

	private bool ownerInitialised = false;
	
	public float respawnDelay = 5.0f;
	public float respawnTimer = 0.0f;
	
	public DockingBay.DockingSlot currentSlot;

#if UNITY_EDITOR
	protected void Start()
	{
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			this.owner = GamePlayerManager.instance.myPlayer;
			this.health.team = this.owner.team;
			this.owner.fighter = this;
			this.weapons.restrictions.teams = (int)Common.OpposingTeam( this.health.team );

			this.capitalShip = this.owner.team == TEAM.TEAM_1 ? GamePlayerManager.instance.commander1.capitalShip
				: GamePlayerManager.instance.commander2.capitalShip;
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

		if ( this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected)
		{
			if ( Input.GetKeyDown( KeyCode.G ) )
			{
				this.Respawn();
			}

			if ( Input.GetKeyDown( KeyCode.P ) )
			{
				this.Die( 0 );
			}

			switch ( this.state )
			{
			case FIGHTERSTATE.FLYING:
			{
				break;
			}
			case FIGHTERSTATE.DOCKED:
			{
				if ( Input.GetKeyDown( KeyCode.Space ) 
				  && !GameMessages.instance.typing )
				{
					this.Undock();
				}
				break;
			}
			case FIGHTERSTATE.UNDOCKING:
			{
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
					if ( Input.GetKeyDown( KeyCode.Space ) )
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

		this.health.team = this.owner.team;
		this.weapons.restrictions.teams = (int)Common.OpposingTeam( this.health.team );
		this.capitalShip = GamePlayerManager.instance.GetCapitalShip( this.owner.team );
		
		foreach ( Renderer render in GetComponentsInChildren<Renderer>( ))
		{
			if ( owner.team == TEAM.TEAM_1 )
			{
				render.material.color = new Color(1.0f,1.0f,0.2f);
			}
			else
			{
				render.material.color = new Color(1,0f,0.2f,1.0f);
			}
		}
	}

	public void Respawn()
	{
		//Find an empty spot on the friendly capital ship and dock there
		DockingBay[] bays = FindObjectsOfType( typeof(DockingBay) ) as DockingBay[];

		DockingBay bay = null;

		if ( bays.Length > 0 )
		{
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
					bay = bays[index];

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

		this.health.FullHeal();
		this.movement.OnRespawn();
		this.weapons.OnRespawn();

		if ( Network.peerType != NetworkPeerType.Disconnected )
		{
			GameNetworkManager.instance.SendRespawnedFighterMessage( this.networkView.viewID );
		}

		if ( bay != null )
		{
			this.Dock( bay.GetFreeSlot() );
		}
		else
		{

			DebugConsole.Warning( "No docking bays found", this );
			this.transform.position = Vector3.zero;
			this.transform.rotation = Quaternion.identity;
			this.rigidbody.velocity = Vector3.zero;
			this.rigidbody.angularVelocity = Vector3.zero;
			this.state = FIGHTERSTATE.FLYING;
		}
	}
	
	public void Die( int explosionType )
	{
		respawnTimer = respawnDelay; 
		
		this.state = FIGHTERSTATE.DEAD;

		if ( this.health.lastHitBy != NetworkViewID.unassigned )
		{
			BaseHealth hitHealth = TargetManager.instance.GetTargetWithID( this.health.lastHitBy );
			if ( hitHealth != null )
			{
				MessageManager.instance.AddMessage( Common.MyNetworkID(), -1,
				    "Player " + Common.MyNetworkID() + " was killed by " + hitHealth.gameObject.name );
			}

		}

		if ( Network.peerType != NetworkPeerType.Disconnected )
		{
			GameNetworkManager.instance.SendDeadFighterMessage( this.networkView.viewID );
		}
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

		if ( Network.peerType != NetworkPeerType.Disconnected )
		{
			GameNetworkManager.instance.SendDockedMessage( this.networkView.viewID, _slot.slotID );
		}
	}

	public void Undock()
	{
		DebugConsole.Log( "Undocking", this );
		this.rigidbody.constraints = RigidbodyConstraints.None;
	
		this.state = FIGHTERSTATE.UNDOCKING;
		this.currentSlot.occupied = false;
		this.currentSlot.landedFighter = null;

		this.movement.desiredSpeed = this.movement.maxSpeed;
		
		this.transform.parent = null;
		this.transform.localScale = Vector3.one;

		if ( Network.peerType != NetworkPeerType.Disconnected )
		{
			GameNetworkManager.instance.SendUndockedMessage ( this.networkView.viewID, this.currentSlot.slotID );
		}
		this.currentSlot = null;
	}

	public void ExitDock()
	{
		this.state = FIGHTERSTATE.FLYING;
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

	public void FighterDestroyedNetwork()
	{
		this.state = FIGHTERSTATE.DEAD;
		this.gameObject.collider.enabled = false;
	}
	
	public void FighterRespawnedNetwork()
	{
		this.state = FIGHTERSTATE.FLYING;
		this.gameObject.collider.enabled = true;
		this.health.FullHeal();
	}
}
