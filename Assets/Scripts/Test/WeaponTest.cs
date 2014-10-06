using UnityEngine;
using System.Collections;

public class WeaponTest : BaseWeaponManager
{
	public BaseHealth[] targetDummies;

	public float turnRate;

	public bool firing;

	public int weaponIndex = 0;
	private WeaponBase[] weapons;

	private void Start()
	{
		this.weapons = this.GetComponents<WeaponBase>();
		this.restrictions.teams = (int)TEAM.TEAM_2;

		GamePlayer dummyOwner = new GamePlayer();
		dummyOwner.team = TEAM.TEAM_2;

		foreach ( BaseHealth dummy in this.targetDummies )
		{
			dummy.Owner = dummyOwner;
		}

		this.health.Owner = new GamePlayer();
		this.health.Owner.team = TEAM.TEAM_1;
	}

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Space ) )
		{
			this.firing = !this.firing;
		}

		if ( this.firing )
		{
			this.weapons[this.weaponIndex].SendFireMessage();
		}

		if ( Input.GetKey( KeyCode.A ) )
		{
			this.transform.Rotate( this.transform.up, -Time.deltaTime * this.turnRate );
		}
		if ( Input.GetKey( KeyCode.D ) )
		{
			this.transform.Rotate( this.transform.up, Time.deltaTime * this.turnRate );
		}

		if ( Input.GetKeyDown( KeyCode.Q ) )
		{
			--this.weaponIndex;
			if ( this.weaponIndex < 0 )
			{
				this.weaponIndex = this.weapons.Length - 1;
			}
		}
		if ( Input.GetKeyDown( KeyCode.E ) )
		{
			++this.weaponIndex;
			if ( this.weaponIndex >= this.weapons.Length )
			{
				this.weaponIndex = 0;
			}
		}

		if ( Input.GetKeyDown( KeyCode.W ) )
		{
			this.currentTarget = TargetManager.instance.GetCentreTarget( this );
		}
		Debug.DrawRay( this.transform.position, this.transform.forward );

		foreach ( BaseHealth dummy in this.targetDummies )
		{
			if ( dummy.currentHealth <= 0.0f )
			{
				dummy.gameObject.SetActive( false );
				if ( this.currentTarget == dummy )
				{
					this.currentTarget = null;
				}
			}
		}

		WeaponBase currentWeapon = this.weapons[this.weaponIndex];

		if ( currentWeapon.weaponDesc.requiresWeaponLock )
		{
			currentWeapon.LockOnUpdate();
		}
		else
		{
			currentWeapon.ResetLockOn();
		}
	}

	private void OnGUI()
	{
		if ( this.currentTarget != null )
		{
			Vector2 screenPos = Camera.main.WorldToViewportPoint( this.currentTarget.transform.position );

			screenPos.x *= Screen.width;
			screenPos.y *= Screen.height;
			screenPos.y = Screen.height - screenPos.y;

			GUI.Label( new Rect( screenPos.x, screenPos.y, 200, 50 ), (int)this.currentTarget.currentHealth + "/" + (int)this.currentTarget.maxHealth );
			GUI.Label( new Rect( screenPos.x, screenPos.y+15, 200, 50 ), (int)this.currentTarget.currentShield + "/" + (int)this.currentTarget.maxShield );
		}

		EnergySystem energy = this.GetComponent<EnergySystem>();
		GUI.Label( new Rect( 15, 15, 150, 50 ), "Energy: " + energy.currentEnergy.ToString("0.00") + "/1.0" );
		GUI.Label( new Rect( 15, 50, 150, 50 ), "Damage Scale: " + energy.GetDamageScale().ToString("0.00") );
		GUI.Label( new Rect( 15, 85, 150, 50 ), "Current Weapon: " + this.weapons[this.weaponIndex].weaponDesc.label );
	
		SpinUpWeapon spinUpWeapon = this.weapons[this.weaponIndex] as SpinUpWeapon;
		if ( spinUpWeapon != null )
		{
			GUI.Label( new Rect( 15, 120, 150, 50 ), "Spin: " + spinUpWeapon.currentSpin + "/" + spinUpWeapon.spinUpDesc.spinUpTime );
		}

		ChargeUpWeapon chargeUpWeapon = this.weapons[this.weaponIndex] as ChargeUpWeapon;
		if ( chargeUpWeapon != null )
		{
			GUI.Label( new Rect( 15, 155, 150, 50 ), "Charge: " + chargeUpWeapon.currentCharge + "/" + chargeUpWeapon.chargeUpDesc.chargeUpTime );
		}
	}
}
