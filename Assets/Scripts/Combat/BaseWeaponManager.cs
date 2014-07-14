using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseWeaponManager : MonoBehaviour
{
	public List<TargetManager.Target> targets;

	protected virtual void Awake()
	{
		this.targets = new List<TargetManager.Target>();
	}

	public virtual void OnBulletCreated( BulletBase _bullet )
	{
		SeekingBullet seeking = _bullet as SeekingBullet;
		if ( seeking != null )
		{
			BaseHealth target = this.targets.Count == 0 ? null : this.targets[0].health;
			seeking.target = target;
		}
	}
}
