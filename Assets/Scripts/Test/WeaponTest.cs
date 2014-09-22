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

		foreach ( BaseHealth dummy in this.targetDummies )
		{
			if ( dummy.currentHealth <= 0.0f )
			{
				dummy.gameObject.SetActive( false );
			}
		}
	}

	private void OnGUI()
	{
		foreach ( BaseHealth dummy in this.targetDummies )
		{
			if ( dummy.gameObject.activeSelf == false )
				continue;

			Vector2 screenPos = Camera.main.WorldToViewportPoint( dummy.transform.position );

			screenPos.x *= Screen.width;
			screenPos.y *= Screen.height;

			GUI.Label( new Rect( screenPos.x, screenPos.y, 200, 50 ), (int)dummy.currentHealth + "/" + (int)dummy.maxHealth );
			GUI.Label( new Rect( screenPos.x, screenPos.y+15, 200, 50 ), (int)dummy.currentShield + "/" + (int)dummy.maxShield );
		}

		EnergySystem energy = this.GetComponent<EnergySystem>();
		GUI.Label( new Rect( 15, 15, 150, 50 ), "Energy: " + energy.currentEnergy.ToString("0.00") + "/1.0" );
		GUI.Label( new Rect( 15, 50, 150, 50 ), "Damage Scale: " + energy.GetDamageScale().ToString("0.00") );
		GUI.Label( new Rect( 15, 85, 150, 50 ), "Current Weapon: " + this.weapons[this.weaponIndex].weaponDesc.label );
	}

	public override void OnBulletCreated( BulletBase _bullet )
	{
		base.OnBulletCreated( _bullet );
	}
}
