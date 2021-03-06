﻿using UnityEngine;
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

	public bool placingAutoTurret = false;
	public bool heightCheck = false;

	public GameObject autoTurretPrefab;
	public GameObject placementMarkerPrefab;
	public GameObject placementSpherePrefab;
	private GameObject placementSphereObj;
	private GameObject placementMarkerObj;

#if UNITY_EDITOR
	public bool isDummyShip = false;
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

		if ( this.GetComponent<NetworkView>().isMine || Network.peerType == NetworkPeerType.Disconnected )
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
					if ( this.isDummyShip == false )
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
			else // Alive
			{
#if UNITY_EDITOR
				if ( !this.isDummyShip )
#endif
				{
					if ( Input.GetKeyDown( KeyCode.A ) )
					{
						this.placingAutoTurret = true;
						this.placementSphereObj.SetActive( true );

						this.movement.ToggleTurningControls( false );
					}

					if ( this.placingAutoTurret )
					{
						if ( !heightCheck )
						{
							Vector3 pos;
							Common.MousePositionToPlanePoint( out pos, Vector3.zero, Vector3.up );
							this.placementSphereObj.transform.position = pos;
							this.placementSphereObj.transform.Rotate( Vector3.up, Time.deltaTime * 180.0f  );
							if ( Input.GetMouseButtonDown( 0 ) )
							{
								this.heightCheck = true;
								this.placementMarkerObj.SetActive( true );
								this.placementMarkerObj.transform.position = pos;
							}
						}
						else
						{
							Vector3 targetPos = this.placementSphereObj.transform.position;
							// Make a vertical plane that is placed at the target point and faces the camera
							Vector3 normal = -Camera.main.transform.forward;
							normal.y = 0.0f;
							normal.Normalize();
							Vector3 planePos;
							// Use the Y point of where the mouse is on that plane to determine height
							Common.MousePositionToPlanePoint( out planePos, targetPos, normal );
							placementSphereObj.transform.position = new Vector3( targetPos.x, planePos.y, targetPos.z );
							
							if ( Input.GetMouseButtonDown( 0 ) )
							{
								GameObject autoTurret;
#if UNITY_EDITOR
								if ( Network.peerType == NetworkPeerType.Disconnected )
								{
									autoTurret = GameObject.Instantiate( this.autoTurretPrefab, planePos, Quaternion.identity ) as GameObject;
								}
								else
#endif
								{
									autoTurret = Network.Instantiate( this.autoTurretPrefab, planePos, Quaternion.identity, 0 ) as GameObject;
								}

								autoTurret.GetComponent<NetworkOwnerControl>().ownerID = this.health.Owner.id;


								autoTurret.GetComponent<Rigidbody>().AddTorque( Common.RandomDirection() * 100.0f );
								this.placingAutoTurret = false;
								this.heightCheck = false;
								this.placementMarkerObj.SetActive( false );
								this.placementSphereObj.SetActive( false );
							}
						}

						if ( Input.GetKeyDown( KeyCode.Escape ) )
						{
							this.placingAutoTurret = false;
							this.heightCheck = false;
							this.placementSphereObj.SetActive( false );
							this.placementMarkerObj.SetActive( false );
							this.movement.ToggleTurningControls( true );
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Called when the owner ID is found on the NetworkOwnerControl
	/// Sets up owner specific information
	/// </summary>
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
	
		if ( this.GetComponent<NetworkView>().isMine || Network.peerType == NetworkPeerType.Disconnected )
		{
			this.turrets.CreateTurrets();
#if UNITY_EDITOR
			if ( !this.isDummyShip )
#endif
			{
				this.placementSphereObj = GameObject.Instantiate( this.placementSpherePrefab ) as GameObject;
				this.placementSphereObj.SetActive( false );
				this.placementMarkerObj = GameObject.Instantiate( this.placementMarkerPrefab ) as GameObject;
				this.placementMarkerObj.SetActive( false );
				
				WEAPON_TYPE autoTurretWeapon = this.autoTurretPrefab.GetComponent<TurretBehavior>().weapon.weaponType;
				BulletDescriptor bulletDesc = BulletDescriptorManager.instance.GetDescOfType( autoTurretWeapon );
				this.placementMarkerObj.transform.localScale = this.placementSphereObj.transform.localScale = Vector3.one * bulletDesc.maxDistance;
			}
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
