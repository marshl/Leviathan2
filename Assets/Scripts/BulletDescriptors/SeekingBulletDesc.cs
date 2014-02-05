using UnityEngine;
using System.Collections;

/// <summary>
/// An abstract bullet descriptor that is used by bullets that can seek out targets
/// </summary>
public abstract class SeekingBulletDesc : BulletDescriptor
{
	public float turnRate;
	public float maxDetectionAngle;
	public float seekingDelayDistance;
}

