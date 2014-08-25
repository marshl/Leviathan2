using UnityEngine;
using System.Collections;

public class CapitalShipWeapons : BaseWeaponManager
{
	public CapitalShipMaster master;

	private bool firingGauss;
	public float gaussFireRate;
	public float gaussBurstRate;
	private int fireIndex;
	private float fireTimer;
	private float burstTimer;

	private void Update()
	{
#if UNITY_EDITOR
		if ( this.master.dummyShip == true )
			return;
#endif

		if ( this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected )
		{
		    if ( Input.GetKeyDown( KeyCode.Space )
			  && this.firingGauss == false
			  && this.fireTimer >= this.gaussFireRate ) 
			{
				this.firingGauss = true;
				this.fireIndex = 0;
				this.fireTimer = 0.0f;
			}

			if ( this.firingGauss )
			{
				this.burstTimer += Time.deltaTime;
				
				if ( this.burstTimer >= this.gaussBurstRate )
				{
					this.burstTimer = 0.0f;
					this.master.turrets.gaussCannons[this.fireIndex].SendFireMessage();
					++this.fireIndex;

					if ( this.fireIndex >= this.master.turrets.gaussCannons.Count )
					{
						this.firingGauss = false;
					}
				}
			}
			else
			{
				this.fireTimer += Time.deltaTime;
			}
		}
	}
}
