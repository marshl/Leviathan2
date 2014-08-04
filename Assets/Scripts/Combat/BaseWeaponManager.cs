using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseWeaponManager : MonoBehaviour
{
	public BaseHealth currentTarget;
	public List<BaseHealth> targetList;
	public int targetIndex;

	public float maxTargetDistance;

	protected virtual void Awake()
	{
		this.currentTarget = null;
		this.targetList = new List<BaseHealth>();
	}

	public virtual void OnBulletCreated( BulletBase _bullet )
	{
		SeekingBullet seekingScript = _bullet as SeekingBullet;
		if ( seekingScript != null
		  && this.currentTarget != null )
		{
			seekingScript.target = this.currentTarget;
		}
	}

	public void SwitchToCentreTarget( Vector3 _forward, bool _ignoreBelowHorizon )
	{
		this.UpdateTargetList();

		this.currentTarget = null;
		float closestAngle = float.MaxValue;
		foreach ( BaseHealth target in this.targetList )
		{
			Vector3 vectorToTarget = target.transform.position - this.transform.position;

			// Ignore targets that are below the horizon
			if ( _ignoreBelowHorizon && Vector3.Dot( vectorToTarget, this.transform.up ) < 0.0f )
			{
				continue;
			}
			float angle = Vector3.Angle( _forward, vectorToTarget );

			if ( angle < closestAngle )
			{
				this.currentTarget = target;
				closestAngle = angle;
			}
		}
	}

	public void SwitchToNextTarget()
	{
		if ( this.targetList.Count > 0 )
		{
			--this.targetIndex;
			if ( this.targetIndex < 0 )
			{
				this.targetIndex = this.targetList.Count - 1;
			}
			this.currentTarget = this.targetList[this.targetIndex];
		}
		else
		{
			this.targetIndex = 0;
		}
	}

	public void SwitchToPreviousTarget()
	{
		if ( this.targetList.Count > 0 )
		{
			++this.targetIndex;
			if ( this.targetIndex >= this.targetList.Count )
			{
				this.targetIndex = 0;
			}
			this.currentTarget = this.targetList[this.targetIndex];
		}
		else
		{
			this.targetIndex = 0;
		}
	}

	public void SwitchToNextFriendlyTarget()
	{
		throw new System.NotImplementedException();
	}

	public void SwitchToPreviousFriendlyTarget()
	{
		throw new System.NotImplementedException();
	}

	public void SwitchToNextMissileTargettingMe()
	{
		throw new System.NotImplementedException();
	}
	
	public abstract void UpdateTargetList();
}
