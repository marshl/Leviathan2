using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CapitalShipTurretManager : MonoBehaviour
{
	public CapitalShipMaster master;

	//TODO: Transforms might have to be replaced with custom script
	public Transform[] turretPositions;
	public Transform[] missileTurretPositions;
	public GameObject lightLaserPrefab;

	public GameObject missileTurretPrefab;

	public GameObject gaussCannonPrefab;
	public Transform[] gaussCannonPositions;
	public List<WeaponBase> gaussCannons;

	public GameObject componentTowerPrefab;
	[Tooltip( "Don't populate this, it'll be done automatically")]
	public List<ComponentTower> componentTowerList;

	public enum STATE
	{
		NONE,
		PLACING_TURRET,
	}
	public STATE currentState;

	public void CreateTurrets()
	{
		foreach ( Transform pos in this.turretPositions )
		{
#if UNITY_EDITOR
			if ( Network.peerType == NetworkPeerType.Disconnected )
			{
				GameObject turret = GameObject.Instantiate( this.lightLaserPrefab, pos.position, pos.rotation ) as GameObject;
				turret.GetComponent<NetworkOwnerControl>().ownerID = this.master.health.Owner.id;
			}
			else
#endif
			{
				Network.Instantiate( this.lightLaserPrefab, pos.position, pos.rotation, 0 );
			}
		} 

		foreach ( Transform pos in this.missileTurretPositions )
		{
#if UNITY_EDITOR
			GameObject turret;
			if ( Network.peerType == NetworkPeerType.Disconnected )
			{
				turret = GameObject.Instantiate( this.missileTurretPrefab, pos.position, pos.rotation ) as GameObject;
				turret.GetComponent<NetworkOwnerControl>().ownerID = this.master.health.Owner.id;
			}
			else
#endif
			{
				Network.Instantiate( this.missileTurretPrefab, pos.position, pos.rotation, 0 );
			}
		}

		foreach ( Transform pos in this.gaussCannonPositions )
		{
			GameObject gaussCannon;
#if UNITY_EDITOR
			if ( Network.peerType == NetworkPeerType.Disconnected )
			{
				gaussCannon = GameObject.Instantiate( this.gaussCannonPrefab, pos.position, pos.rotation ) as GameObject;
				gaussCannon.GetComponent<NetworkOwnerControl>().ownerID = this.master.health.Owner.id;
			}
			else
#endif
			{
				gaussCannon = Network.Instantiate( this.gaussCannonPrefab, pos.position, pos.rotation, 0 ) as GameObject;
			}
			this.gaussCannons.Add( gaussCannon.GetComponent<WeaponBase>() );
		}


		this.InitialiseComponentTowers();
	}

	private void Update()
	{
#if UNITY_EDITOR
		if ( !this.master.isDummyShip )
#endif
		{
			if ( this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected )
			{
				if ( Input.GetKeyDown( KeyCode.Q ) )
				{
					if ( this.currentState == STATE.NONE )
					{
						this.currentState = STATE.PLACING_TURRET;
						this.master.movement.ToggleTurningDisplay( false );
					}
					else
					{
						this.currentState = STATE.NONE;
						this.master.movement.ToggleTurningDisplay( true );
					}
				}

				if ( this.currentState == STATE.PLACING_TURRET )
				{
					this.TurretPlacementUpdate();
				}
			}
		}
	}

	private void InitialiseComponentTowers()
	{
		ComponentTowerSlot[] componentTowerSlots = this.GetComponentsInChildren<ComponentTowerSlot>();

		this.componentTowerList = new List<ComponentTower>( componentTowerSlots.Length );
		foreach ( ComponentTowerSlot slot in componentTowerSlots )
		{
			GameObject componentTowerObj = GameObject.Instantiate( 
			    this.componentTowerPrefab, slot.transform.position, slot.transform.rotation ) as GameObject;
			componentTowerObj.transform.parent = slot.transform.parent;
			ComponentTower tower = componentTowerObj.GetComponent<ComponentTower>();
			//tower.Deactivate();
			tower.Activate();
			this.componentTowerList.Add( tower );
		}
	}

	private void TurretPlacementUpdate()
	{
		ComponentTower closestTower = null;
		float closestDistance = float.MaxValue;

		foreach ( ComponentTower tower in this.componentTowerList )
		{
			if ( tower.isActive && tower.turret == null )
			{
				Vector2 towerScreenPos = Camera.main.WorldToScreenPoint( tower.transform.position );
				float dist = (new Vector2( Input.mousePosition.x, Input.mousePosition.y ) - towerScreenPos).magnitude;

				if ( dist < closestDistance )
				{
					closestDistance = dist;
					closestTower = tower;
				}
			}

			tower.RemoveHighlight();
		}

		if ( closestTower != null )
		{
			closestTower.Highlight();

			if ( Input.GetMouseButtonDown( 0 ) )
			{
				this.PlaceTurret( closestTower );
				this.master.movement.ToggleTurningDisplay( true );
				this.currentState = STATE.NONE;
			}
		}
	}

	public void PlaceTurret( ComponentTower _tower )
	{
		Vector3 pos = _tower.transform.position;
		Quaternion rot = _tower.transform.rotation;
		GameObject turret;
#if UNITY_EDITOR
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			turret = GameObject.Instantiate( this.lightLaserPrefab, pos, rot ) as GameObject;
			turret.GetComponent<NetworkOwnerControl>().ownerID = this.master.health.Owner.id;
		}
		else
#endif
		{
			turret = Network.Instantiate( this.lightLaserPrefab, pos, rot, 0 ) as GameObject;
		}

		TurretBehavior turretBehaviour = turret.GetComponent<TurretBehavior>();
		turretBehaviour.componentTower = _tower;

		_tower.Occupy( turretBehaviour );
	}
}
