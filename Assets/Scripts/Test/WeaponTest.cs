using UnityEngine;
using System.Collections;

public class WeaponTest : BaseWeaponManager
{
	public WeaponBase weapon;

	public bool usesEnergy = false;
	public float maxLaserEnergy = 100.0f;
	public float currentLaserEnergy = 100.0f;
	public float energyRechargePerSecond = 10.0f;
	public float laserPenaltyThreshold = 25.0f;
	public float laserMinimumScale = 0.1f;

	public BaseHealth[] targetDummies;

	public float turnRate;

	private void Update()
	{
		if ( Input.GetKey( KeyCode.Space ) )
		{
			if(usesEnergy)
			{
				AttemptToFireLasers();
			}
			else
			{
				weapon.SendFireMessage();
			}
		}

		if ( Input.GetKey( KeyCode.A ) )
		{
			this.transform.Rotate( this.transform.up, -Time.deltaTime * this.turnRate );
		}
		if ( Input.GetKey( KeyCode.D ) )
		{
			this.transform.Rotate( this.transform.up, Time.deltaTime * this.turnRate );
		}

		RegenerateLaserEnergy();
	}

	private void OnGUI()
	{
		foreach ( BaseHealth dummy in this.targetDummies )
		{
			Vector2 screenPos = Camera.main.WorldToViewportPoint( dummy.transform.position );

			screenPos.x *= Screen.width;
			screenPos.y *= Screen.height;

			GUI.Label( new Rect( screenPos.x, screenPos.y, 50, 50 ), dummy.currentHealth + "/" + dummy.maxHealth );

			GUI.Label( new Rect( screenPos.x, screenPos.y+15, 50, 50 ), dummy.currentShield + "/" + dummy.maxShield );
		}
	}

	public void AttemptToFireLasers()
	{
		if(currentLaserEnergy > laserPenaltyThreshold)
		{
			if(this.weapon.SendFireMessage())
			{
				currentLaserEnergy -= this.weapon.weaponDesc.energyCost; //Fire as normal
			}
		}
		else
		{
			//Damage is modified by the percentage under the threshold the energy is at
			float damageScale = (currentLaserEnergy / laserPenaltyThreshold );

			if(damageScale < laserMinimumScale)
			{
				damageScale = laserMinimumScale;
			}

			if(this.weapon.SendFireMessage (damageScale)) //Fire with penalty
			{
				currentLaserEnergy -= this.weapon.weaponDesc.energyCost;
				if(currentLaserEnergy < 0)
				{
					currentLaserEnergy = 0;
				}
			}
		}
	}

	public void RegenerateLaserEnergy()
	{
		if(currentLaserEnergy < maxLaserEnergy)
		{
			currentLaserEnergy += energyRechargePerSecond * Time.deltaTime;
			if(currentLaserEnergy > maxLaserEnergy)
			{
				currentLaserEnergy = maxLaserEnergy;
			}
		}
	}
}
