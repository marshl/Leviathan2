using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class SpinUpWeapon : WeaponBase
{
	[HideInInspector]
	public SpinUpWeaponDesc spinUpDesc;
	public float currentSpin;

	protected override void Update()
	{
		base.Update();
		if ( this.timeSinceShot > this.spinUpDesc.spinDownDelay )
		{
			this.currentSpin = Mathf.Clamp
			( 
				 this.currentSpin - (Time.deltaTime / this.spinUpDesc.spinDownTime),
				 0.0f, 1.0f
			);
		}
	}

	public override bool CanFire()
	{
		//Debug.Log( this.GetCurrentFireRate() );
		return this.timeSinceShot > this.GetCurrentFireRate();
	}

	public virtual float GetCurrentFireRate()
	{
		return Mathf.Lerp( this.spinUpDesc.fireRate, this.spinUpDesc.maxFireRate, this.currentSpin );
	}

	public override List<BulletBase> Fire ()
	{
		this.currentSpin = Mathf.Clamp
		(
			this.currentSpin + this.GetCurrentFireRate() / this.spinUpDesc.spinDownTime,
			0.0f, 1.0f
		);
		return base.Fire ();
	}
}

