using UnityEngine;
using System.Collections;

/// <summary>
/// A bullet descriptor that is used by bullets that can seek out targets
/// </summary>
public class SeekingBulletDesc : BulletDescriptor
{
	public float turnRate;
	public float maxDetectionAngle;
	public float seekingDelayDistance;
}

