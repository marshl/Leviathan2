using UnityEngine;
using System.Collections;

public class CapitalShipTurretManager : MonoBehaviour
{
	//TODO: Transforms might have to be replaced with custom script
	public Transform[] turretPositions;

	public GameObject lightLaserPrefab;

	/*private void Start()
	{
		if ( this.networkView.isMine )
		{
			Debug.Log( "Creating turrets" );
			this.CreateTurrets();
		}
	}*/

	public void CreateTurrets()
	{
		foreach ( Transform pos in this.turretPositions )
		{

			GameObject obj = Network.Instantiate( this.lightLaserPrefab, pos.position, pos.rotation, 0 ) as GameObject;
			// Turret itself will handle parenting

			Debug.Log( "Created " + obj );
		} 
	}
}
