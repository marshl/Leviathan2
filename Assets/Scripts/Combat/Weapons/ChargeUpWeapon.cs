using UnityEngine;
using System.Collections;

public class ChargeUpWeapon : WeaponBase
{
	[HideInInspector]
	public ChargeUpWeaponDesc chargeUpDesc;

	public float currentCharge;

	private bool isOverheating;

	private bool holdingDownTrigger;

	protected override void Update()
	{
		base.Update();

		if ( this.holdingDownTrigger )
		{
			this.currentCharge += Time.deltaTime;

			if ( this.currentCharge >= this.chargeUpDesc.chargeUpTime )
			{
				this.Fire();

				this.currentCharge = 0.0f;
			}
		}
		else if ( this.chargeUpDesc.depleteOnPrerelease )
		{
			this.currentCharge = 0.0f;
		}
		else
		{
			this.currentCharge = Mathf.Max( 0.0f, this.currentCharge - Time.deltaTime ); 
		}

		this.holdingDownTrigger = false;
	}

	public override bool SendFireMessage()
	{
		this.holdingDownTrigger = this.CanFire();
		return this.holdingDownTrigger;

	}

	public float GetChargeTimeAsPercentage()
	{
		return this.currentCharge / this.chargeUpDesc.chargeUpTime;
	}
}
