using UnityEngine;
using System.Collections;

public class FighterMaster : MonoBehaviour 
{
	public enum FIGHTERSTATE
	{
		DOCKED,
		UNDOCKING,
		FLYING,
		OUT_OF_CONTROL,
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
	
	public float respawnDelay;
	public float respawnTimer;

	public float outOfControlDuration;
	
	public DockingBay.DockingSlot currentSlot;
	public Renderer model;

#if UNITY_EDITOR
	public bool dummyShip = false;
	public int ownerID;

	protected void Start()
	{
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			this.owner = GamePlayerManager.instance.GetPlayerWithID( ownerID );
			//this.owner = GamePlayerManager.instance.myPlayer;
			this.health.team = this.owner.team;
			this.owner.fighter = this;
			this.weapons.restrictions.teams = (int)Common.OpposingTeam( this.health.team );

			GamePlayer commanderPlayer = this.owner.team == TEAM.TEAM_1 ? GamePlayerManager.instance.commander1
				: GamePlayerManager.instance.commander2;

			if ( commanderPlayer != null )
			{
				this.capitalShip = commanderPlayer.capitalShip;
			}
			Debug.Log( this.owner );
			DebugConsole.Log( "Player " + this.owner.id + " now owns " + this.name, this );
		}
		else
		{
			this.dummyShip = false;
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
			#if UNITY_EDITOR
			if ( this.dummyShip == false )
				#endif
			{
			if ( Input.GetKeyDown( KeyCode.G ) )
			{
				this.Respawn();
			}

			if ( Input.GetKeyDown( KeyCode.P ) )
			{
				this.OnLethalDamage();
			}

			if ( this.state == FIGHTERSTATE.DOCKED )
			{
				if ( Input.GetKeyDown( KeyCode.Space ) 
				    && !GameMessages.instance.typing )
				{
					this.Undock();
				}
			}
			}

			if ( this.state == FIGHTERSTATE.OUT_OF_CONTROL
			       || this.state == FIGHTERSTATE.DEAD )
			{
				this.respawnTimer -= Time.deltaTime;

				if ( this.respawnDelay - this.respawnTimer >= this.outOfControlDuration )
				{
					this.OnOutOfControlExpiration();
				}

				if ( this.respawnTimer <= 0.0f )
				{
					this.respawnTimer = 0.0f;
					if ( Input.GetKeyDown( KeyCode.Space ) )
					{
						this.Respawn();
					}
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

		this.ToggleEnabled( true, true );

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
	
	public void OnLethalDamage()
	{
		respawnTimer = respawnDelay; 

		this.state = FIGHTERSTATE.OUT_OF_CONTROL;

		if ( this.health.lastHitBy != NetworkViewID.unassigned )
		{
			BaseHealth hitHealth = TargetManager.instance.GetTargetWithID( this.health.lastHitBy );
			if ( hitHealth != null )
			{
				string msg = "Player " + Common.MyNetworkID() + " was killed by " + hitHealth.gameObject.name;
				MessageManager.instance.AddMessage( Common.MyNetworkID(), msg, true );
			}
		}

		if ( Network.peerType != NetworkPeerType.Disconnected )
		{
			GameNetworkManager.instance.SendDeadFighterMessage( Common.MyNetworkID() );
		}

		this.rigidbody.AddTorque( Common.RandomDirection() );
	}

	private void OnOutOfControlExpiration()
	{
		this.state = FIGHTERSTATE.DEAD;
		this.ToggleEnabled( false, true );
	}

	private void ToggleEnabled( bool _enabled, bool _local )
	{
		this.collider.enabled = _enabled;
		this.model.enabled = _enabled;

		if ( _enabled )
		{
			this.health.FullHeal();
			this.movement.OnRespawn();
			this.weapons.OnRespawn();
			this.health.lastHitBy = NetworkViewID.unassigned;
		}
		else
		{
			this.rigidbody.velocity = this.rigidbody.angularVelocity = Vector3.zero;
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
		this.rigidbody.constraints = RigidbodyConstraints.None;
	
		this.state = FIGHTERSTATE.UNDOCKING;
		this.currentSlot.occupied = false;
		this.currentSlot.landedFighter = null;

		//this.movement.desiredSpeed = this.movement.maxSpeed;
		
		this.transform.parent = null;

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
	
	private void OnCollisionEnter( Collision _collision )
	{
		if ( Network.peerType == NetworkPeerType.Disconnected || this.networkView.isMine )
		{
			if ( this.state == FIGHTERSTATE.OUT_OF_CONTROL )
			{
				this.OnOutOfControlExpiration();
			}
		}
	}

	public void OnOutOfControlNetworkMessage()
	{
		this.state = FIGHTERSTATE.OUT_OF_CONTROL;
	}

	public void OnDestroyFighterNetworkMessage()
	{
		this.state = FIGHTERSTATE.OUT_OF_CONTROL;
		this.ToggleEnabled( false, false );
	}
	
	public void OnRespawnFighterNetworkMessage()
	{
		this.state = FIGHTERSTATE.FLYING;
		this.ToggleEnabled( true, false );
	}
}
