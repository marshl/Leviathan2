using UnityEngine;
using System.Collections;

public class BaseHealth : MonoBehaviour 
{
	public float currentHealth;
	public float maxHealth;
	
	public bool isIndestructible;

	public virtual void DealDamage( float _damage )
	{
		if ( this.isIndestructible == true )
		{
			return;
		}
		this.currentHealth -= _damage;
	}
}
