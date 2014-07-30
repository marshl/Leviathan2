using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseWeaponManager : MonoBehaviour
{
	public TargetManager.Target currentTarget;
	public List<TargetManager.Target> otherTargets;

	protected virtual void Awake()
	{
		this.currentTarget = null;
		this.otherTargets = new List<TargetManager.Target>();
	}

	public virtual void OnBulletCreated( BulletBase _bullet )
	{
		SeekingBullet seekingScript = _bullet as SeekingBullet;
		if ( seekingScript != null
		  && this.currentTarget != null )
		{
			seekingScript.target = this.currentTarget.health;
		}
	}
}
