using UnityEngine;
using System.Collections;

//[BulletStats(25, 2.0f)]
public class BulletA : BulletBase
{
	public float extraA;

	private LightLaserBulletDesc laserDesc;

	public override void Start()
	{
		this.laserDesc = this.desc as LightLaserBulletDesc;
	}

	protected override void Update ()
	{
		base.Update ();
		this.transform.Rotate( Vector3.up, this.laserDesc.turnRate );
	}
}
