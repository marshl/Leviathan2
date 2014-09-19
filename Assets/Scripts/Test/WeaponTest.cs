﻿using UnityEngine;
using System.Collections;

public class WeaponTest : BaseWeaponManager
{
	public WeaponBase weapon;

	public BaseHealth[] targetDummies;

	public float turnRate;

	private void Update()
	{
		if ( Input.GetKey( KeyCode.Space ) )
		{
			weapon.SendFireMessage();
		}

		if ( Input.GetKey( KeyCode.A ) )
		{
			this.transform.Rotate( this.transform.up, -Time.deltaTime * this.turnRate );
		}
		if ( Input.GetKey( KeyCode.D ) )
		{
			this.transform.Rotate( this.transform.up, Time.deltaTime * this.turnRate );
		}
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
}
