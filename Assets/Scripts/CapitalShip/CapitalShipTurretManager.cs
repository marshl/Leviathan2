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
			if ( Network.peerType == NetworkPeerType.Disconnected )
			{
				GameObject turret = GameObject.Instantiate( this.lightLaserPrefab, pos.position, pos.rotation ) as GameObject;
				turret.GetComponent<NetworkOwnerControl>().ownerID = this.master.owner.id;
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
				turret.GetComponent<NetworkOwnerControl>().ownerID = this.master.owner.id;
			}
			else
			{
				turret = Network.Instantiate( this.missileTurretPrefab, pos.position, pos.rotation, 0 ) as GameObject;
			}
		}

		foreach ( Transform pos in this.gaussCannonPositions )
		{
			GameObject gaussCannon;
			if ( Network.peerType == NetworkPeerType.Disconnected )
			{
				gaussCannon = GameObject.Instantiate( this.gaussCannonPrefab, pos.position, pos.rotation ) as GameObject;
				gaussCannon.GetComponent<NetworkOwnerControl>().ownerID = this.master.owner.id;
			}
			else
			{
				gaussCannon = Network.Instantiate( this.gaussCannonPrefab, pos.position, pos.rotation, 0 ) as GameObject;
			}
			this.gaussCannons.Add( gaussCannon.GetComponent<WeaponBase>() );
		}
	}
}
