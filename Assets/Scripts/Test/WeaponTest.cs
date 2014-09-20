using UnityEngine;
using System.Collections;

public class WeaponTest : BaseWeaponManager
{
	public WeaponBase weapon;
	
	public BaseHealth[] targetDummies;

	public float turnRate;

	public bool firing;

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Space ) )
		{
			this.firing = !this.firing;
		}

		if ( this.firing )
		{
			this.weapon.SendFireMessage();
		}

		if ( Input.GetKey( KeyCode.A ) )
		{
			this.transform.Rotate( this.transform.up, -Time.deltaTime * this.turnRate );
		}
		if ( Input.GetKey( KeyCode.D ) )
		{
			this.transform.Rotate( this.transform.up, Time.deltaTime * this.turnRate );
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
	}

	public override void OnBulletCreated( BulletBase _bullet )
	{
		base.OnBulletCreated( _bullet );
	}
}
