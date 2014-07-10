using UnityEngine;
using System.Collections;

public class CapitalShipTurretManager : MonoBehaviour
{
	//TODO: Transforms might have to be replaced with custom script
	public Transform[] turretPositions;
	public Transform[] missileTurretPositions;
	public GameObject lightLaserPrefab;

	public GameObject missileTurretPrefab;

	public void CreateTurrets()
	{
		foreach ( Transform pos in this.turretPositions )
		{

			GameObject obj = Network.Instantiate( this.lightLaserPrefab, pos.position, pos.rotation, 0 ) as GameObject;
			// Turret itself will handle parenting

			DebugConsole.Log( "Created " + obj );
		} 

		foreach ( Transform pos in this.missileTurretPositions )
		{
			Network.Instantiate( this.missileTurretPrefab, pos.position, pos.rotation, 0 );
		}
	}
}
