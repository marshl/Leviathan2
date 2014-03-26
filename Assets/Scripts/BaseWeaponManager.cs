using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseWeaponManager : MonoBehaviour
{
	protected List<TargetManager.Target> possibleTargets;

	protected virtual void Awake()
	{
		this.possibleTargets = new List<TargetManager.Target>();
	}

	public virtual void OnBulletCreated( BulletBase _bullet )
	{
		SeekingBullet seeking = _bullet as SeekingBullet;
		if ( seeking != null )
		{
			BaseHealth target = this.possibleTargets.Count == 0 ? null : this.possibleTargets[0].health;
			seeking.target = target;
		}
	}
}
