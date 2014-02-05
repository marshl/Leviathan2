using UnityEngine;
using System.Collections;

/// <summary>
/// The base class for any weapon that fires in a burst
/// </summary>
public abstract class BurstFireWeapon : WeaponBase
{
	/// <summary>
	/// The descriptor for this weapon (Set elsewhere)
	/// </summary>
	[HideInInspector]
	public BurstFireWeaponDesc burstFireDesc;

	/// <summary>
	/// Is this weapon currently firing?
	/// </summary>
	public bool isFiring = false;

	/// <summary>
	/// The time since the last burst
	/// </summary>
	public float timeSinceBurst;

	/// <summary>
	/// The number of shots that have been fired in the current burst
	/// </summary>
	public int shotsFiredInBurst;


	public override bool CanFire ()
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
				this.Fire();
				++this.shotsFiredInBurst;
				if ( this.shotsFiredInBurst >= this.burstFireDesc.shotsInBurst )
				{
					this.isFiring = false;
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

