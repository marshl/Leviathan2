using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The class used for any spin up weapon instance
/// A spin up weapon is one that increaees in speed as it 
///    continues to fire until it reaches a speed cap.
/// </summary>
public class SpinUpWeapon : WeaponBase
{
	/// <summary>
	/// The descriptor for this weapon type (set elsewhere)
	/// </summary>
	[HideInInspector]
	public SpinUpWeaponDesc spinUpDesc;

	/// <summary>
	/// A number between 0 and 1 storing how fast the weapon can fire
	/// </summary>
	public float currentSpin;

	protected override void Update()
	{
		base.Update();

		// If this hasn't shot for long enough
		if ( this.timeSinceShot > this.spinUpDesc.spinDownDelay )
		{
			// Start reducing the spin
			this.currentSpin = Mathf.Clamp
			( 
				 this.currentSpin - (Time.deltaTime / this.spinUpDesc.spinDownTime),
				 0.0f, 1.0f
			);
		}
	}

	public override bool CanFire()
	{
		return this.timeSinceShot > this.GetCurrentFireRate();
	}

	public virtual float GetCurrentFireRate()
	{
		return Mathf.Lerp( this.spinUpDesc.fireRate, this.spinUpDesc.maxFireRate, this.currentSpin );
	}

	public override List<BulletBase> Fire()
	{
		this.currentSpin = Mathf.Clamp
		(
			this.currentSpin + this.GetCurrentFireRate() / this.spinUpDesc.spinUpTime,
			0.0f, 1.0f
		);
		return base.Fire();
	} 
}

