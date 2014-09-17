using UnityEngine;
using System.Collections;

public class NetworkTestTurret : TurretBehavior
{
	public Transform dummy;

	protected override void Update()
	{
		this.dummy.transform.position = this.TurretAim( this.currentTarget.transform );
		this.gameObject.GetComponent<WeaponBase>().SendFireMessage();
	}
}
