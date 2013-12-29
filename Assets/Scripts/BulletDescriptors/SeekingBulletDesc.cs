using UnityEngine;
using System.Collections;

public abstract class SeekingBulletDesc : BulletDescriptor
{
	public float turnRate;
	public float maxDetectionAngle;
	public float seekingDelayDistance;
}

