using UnityEngine;
using System.Collections;

public class ShotgunWeapon : WeaponBase
{
	public ShotgunWeaponDesc shotgunDesc;

	public override bool SendFireMessage()
	{
		if ( this.CanFire() )
		{
			for ( int shotCounter = 0; shotCounter < this.shotgunDesc.shots; ++shotCounter )
			{
				this.Fire( true, 0.0f, Quaternion.identity );
			}
			return true;
		}
		else
		{
			return false;
		}
	}
}
