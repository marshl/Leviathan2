using UnityEngine;
using System.Collections;

[BulletTypeAttribute( typeof(LightLaserBullet) )]
public class LightLaserBulletDesc : BulletDescriptor
{
	public float turnRate;
}
