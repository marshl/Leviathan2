using UnityEngine;
using System.Collections;

public class BaseHealth : MonoBehaviour 
{
	public int teamNumber;

	public float currentHealth;
	public float maxHealth;
	
	public bool isIndestructible;

	protected virtual void Start()
	{
		TargetManager.instance.AddTarget( this );
	}

	public virtual void DealDamage( float _damage )
	{
		if ( this.isIndestructible == true )
		{
			return;
		}
		this.currentHealth -= _damage;
	}
}
