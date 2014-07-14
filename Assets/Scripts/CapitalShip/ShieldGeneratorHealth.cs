using UnityEngine;
using System.Collections;

public class ShieldGeneratorHealth : BaseHealth {

	public float explosionRadius = 250;
	public float explosionDamage = 150;

	bool kaboom = false;


	// Update is called once per frame
	public override void Update()
	{
		if ( this.currentHealth <= 0 && kaboom == false )
		{
			this.currentHealth = 0;
			Explode();
			kaboom = true;
			Debug.Log ("Kaboom!");
		}
		else
		{
			this.RegenerateShields();
		}
	}

	public void Explode()
	{
//		BaseHealth[] healthScripts = (BaseHealth[]) Object.FindObjectsOfType (typeof (BaseHealth));

	/*	foreach(BaseHealth healths in healthScripts)
		{
			float distanceBetween = Vector3.Distance (this.transform.position, healths.transform.position);

			if(distanceBetween <= explosionRadius)
			{
				float damageMultiplier = 1.0f - (distanceBetween / explosionRadius);

				if(damageMultiplier > 0)
				{
					healths.DealDamage (explosionDamage * damageMultiplier, true);
				}
			}

		}*/

		GameNetworkManager.instance.SendDeadShieldMessage (this.networkView.viewID);

	}

	public void ShieldDestroyedNetwork()
	{
		//Play explosion, decrement shield generator count then remove self

		//todo: explosion

		this.transform.parent.GetComponent<CapitalHealth>().shieldGenerators -= 1;
		this.transform.parent.GetComponent<CapitalHealth>().RecalculateMaxShields();

		TargetManager.instance.RemoveTarget(this.networkView.viewID);
		Destroy(this.gameObject);

		//this.gameObject.collider.enabled = false;
		//this.gameObject.
	}
}
