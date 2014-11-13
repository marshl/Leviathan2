using UnityEngine;
using System.Collections;

/// <summary>
/// A bullet descriptor that is used by bullets that can seek out targets
/// </summary>
public class SeekingBulletDesc : SmartBulletDesc
{
	public float turnRate;
	public bool canAngleOut;
	public float maxDetectionAngle; 
	public float seekingDelayDistance; 
}

