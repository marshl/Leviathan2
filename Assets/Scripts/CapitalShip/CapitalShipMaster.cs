using UnityEngine;
using System.Collections;

/// <summary>
/// The master script for capital ship. As much code as possible should be written in separate scripts, all linking to this
/// </summary>
public class CapitalShipMaster : MonoBehaviour
{
	public CapitalShipMovement movement;
	public NetworkOwnerControl ownerControl;
	public CapitalShipTurretManager turrets;
	public Transform depthControl;
	public CapitalHealth health;
	public DockingBay dockingBay;

	public CapitalShipCamera capitalCamera;

	private bool ownerInitialised = false;

	public float shipLength;

	public bool isDying = false;
	public float deathDuration;
	private float dyingTimer = 0.0f;

	private float explosionTimer;
	public float finaleExplosionInterval;

	public GameObject explosionPrefab;
	public LayerMask explosionCollisionLayers;

#if UNITY_EDITOR
	public bool dummyShip = false;
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

		if ( this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected)
		{
			if ( this.health.currentHealth <= 0.0f )
			{
				if ( this.isDying == false )
				{
					this.isDying = true;
					this.movement.OnFinaleSequenceStart();
					GameNetworkManager.instance.SendCapitalShipDeathMessage( this );
				}

				this.dyingTimer += Time.deltaTime;

				this.UpdateFinaleExplosions();

				if ( this.dyingTimer > this.deathDuration )
				{
					GameNetworkManager.instance.SendCapitalShipExplodedMessage( this );

#if UNITY_EDITOR
					if ( this.dummyShip == false )
#endif
					{
						GameObject.Destroy( this.capitalCamera.gameObject );
					}


					if ( Network.peerType == NetworkPeerType.Disconnected )
					{
						GameObject.Destroy( this.gameObject );
					}
					else
					{
						Network.Destroy( this.gameObject );
					}
				}
			}
		}
	}

	private void OwnerInitialise()
	{
		this.ownerInitialised = true;
		
		int playerID = this.ownerControl.ownerID.Value;
		this.health.Owner = GamePlayerManager.instance.GetPlayerWithID( playerID );

		if ( this.health.Owner.capitalShip != null ) 
		{
			DebugConsole.Warning( "Capital ship already set for " + playerID, this );
			return;
		}

		this.health.Owner.capitalShip = this; 
		DebugConsole.Log( "Set player " + playerID + " to own capital ship", this.gameObject ); 
	
		if ( this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected )
		{
			this.turrets.CreateTurrets();
		}  
		else
		{
			this.enabled = false;
			this.movement.enabled = false;
		}

		foreach ( CapitalShipSubHealth subHealth in this.GetComponentsInChildren<CapitalShipSubHealth>() )
		{
			subHealth.Owner = this.health.Owner;
		}

		this.SetTeamColours();
	}

	public void UpdateFinaleExplosions()
	{
		this.explosionTimer += Time.deltaTime;

		if ( this.explosionTimer >= this.finaleExplosionInterval )
		{
			this.explosionTimer -= this.finaleExplosionInterval;

			// Raycast from a random point around the ship to the ship and put an explosion at the contact point
			float dist = 1000.0f;
			Vector3 endPos = this.transform.position + this.transform.forward * Random.Range( -this.shipLength/2, this.shipLength/2 );
			Vector3 startPos = this.transform.position + Common.RandomDirection() * dist;
			RaycastHit hitInfo;

			if ( Physics.Linecast( startPos, endPos, out hitInfo, explosionCollisionLayers ) )
			{
				GameObject explosionObj = GameObject.Instantiate( this.explosionPrefab, hitInfo.point, Common.RandomRotation() ) as GameObject; 
				GameObject.Destroy( explosionObj, 5.0f );
				explosionObj.transform.parent = this.depthControl;
			}
		}
	}

	private void SetTeamColours()
	{
		foreach ( Renderer render in GetComponentsInChildren<Renderer>() )
		{
			if ( this.health.Owner.team == TEAM.TEAM_1 )
			{
				render.material.color = new Color( 1.0f,1.0f,0.5f );
			}
			else
			{
				render.material.color = new Color( 1,0f,0.2f,1.0f );
			}
		}
	}
}
