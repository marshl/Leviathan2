using UnityEngine;
using System.Collections;

public class EnergySystem : MonoBehaviour
{
	public float currentEnergy = 1.0f;

	public float rechargePerSecond;
	public float rechargeDelay;
	public float delayTimer;


	private void Update()
	{
		this.delayTimer += Time.deltaTime;

		if ( this.delayTimer >= this.rechargeDelay )
		{
			this.currentEnergy = Mathf.Min( 
			   this.currentEnergy + this.rechargePerSecond * Time.deltaTime, 1.0f );
		}
	}

	public void ReduceEnergy( float _amount )
	{
		if ( _amount <= 0.0f )
		{
			throw new System.ArgumentException( "Energy usage must be above 0 : " + this.gameObject.name );
		}

		this.currentEnergy = Mathf.Max( 0.0f, this.currentEnergy - _amount );
		this.delayTimer = 0.0f;
	}

	public float GetDamageScale()
	{
		// At "minEnergyDamageMultiplier" energy or below, "energyDamagePenalty" is dealt
		// At 1.0f energy, 1.0f damage is dealt, with a linear scale anywhere between
		float energyMultiplier = (this.currentEnergy - WeaponDescManager.instance.minEnergyDamageMultiplier)
			/ ( 1.0f -  WeaponDescManager.instance.minEnergyDamageMultiplier );

		energyMultiplier = Mathf.Clamp01( energyMultiplier );
		return Mathf.Lerp( WeaponDescManager.instance.energyDamagePenalty, 1.0f, energyMultiplier );
	}
}
