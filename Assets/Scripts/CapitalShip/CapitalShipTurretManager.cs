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

	public void CreateTurrets()
	{
		foreach ( Transform pos in this.turretPositions )
		{
			GameObject turret;
			if ( Network.peerType == NetworkPeerType.Disconnected )
			{
				this.lightLaserPrefab.GetComponent<BaseHealth>().team = TEAM.TEAM_2;
				turret = GameObject.Instantiate( this.lightLaserPrefab, pos.position, pos.rotation ) as GameObject;
				//turret.GetComponent<BaseHealth>().team = this.master.health.team;
				Debug.Log( turret.GetComponent<BaseHealth>().team );
			}
			else
			{
				Network.Instantiate( this.lightLaserPrefab, pos.position, pos.rotation, 0 );
			}
		} 

		foreach ( Transform pos in this.missileTurretPositions )
		{
			GameObject turret;
			if ( Network.peerType == NetworkPeerType.Disconnected )
			{
				turret = GameObject.Instantiate( this.missileTurretPrefab, pos.position, pos.rotation ) as GameObject;
				turret.GetComponent<BaseHealth>().team = this.master.health.team;
			}
			else
			{
				Network.Instantiate( this.missileTurretPrefab, pos.position, pos.rotation, 0 );
			}
		}

		foreach ( Transform pos in this.gaussCannonPositions )
		{
			GameObject obj;
			if ( Network.peerType == NetworkPeerType.Disconnected )
			{
				obj = GameObject.Instantiate( this.gaussCannonPrefab, pos.position, pos.rotation ) as GameObject;
			}
			else
			{
				obj = Network.Instantiate( this.gaussCannonPrefab, pos.position, pos.rotation, 0 ) as GameObject;
			}
			this.gaussCannons.Add( obj.GetComponent<WeaponBase>() );
		}
	}
}
