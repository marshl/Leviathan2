using UnityEngine;
using System.Collections;

/// <summary>
/// The class used for any weapon that fires in a burst
/// </summary>
public class BurstFireWeapon : WeaponBase
{
	public BurstFireWeaponDesc burstFireDesc;
	public bool isFiring = false;
	public float timeSinceBurst;
	public int shotsFiredInBurst;
	
	public override bool CanFire()
	{
		return this.isFiring == false
			&& base.CanFire();
	}

	protected override void Update()
	{
		base.Update();

		if ( this.isFiring == true )
		{
			if ( this.timeSinceShot >= this.burstFireDesc.fireRate )
			{
				{
					this.Fire( true, 0.0f, Quaternion.identity );
					++this.shotsFiredInBurst;
					if ( this.shotsFiredInBurst >= this.burstFireDesc.shotsInBurst )
					{
						this.isFiring = false;
					}
				}
			}
		}
		else
		{
			this.timeSinceBurst += Time.deltaTime;
		}
	}
	
	public override bool SendFireMessage()
	{
		if ( this.isFiring == true
		  || this.CanFire() == false
		  || this.timeSinceBurst < this.burstFireDesc.timeBetweenBursts )
		{
			return false;
		}

		this.isFiring = true;
		this.timeSinceBurst = 0.0f;
		this.shotsFiredInBurst = 0;
		return true;
	}
}

