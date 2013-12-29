using UnityEngine;
using System.Collections;

public class WeaponHandler : MonoBehaviour {
	
	public GameObject bullet;
	public Transform firePoint;
	public float refireTime;
	public Vector3 fireAngleVariance;
	public float projectileSpeed;
	public float projectileLifetime;
	public string weaponName;
	public int damage;
	public int shotQuantity = 1;
	
	
	float refireTimer;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		if(refireTimer > 0)
		{
			refireTimer -= Time.deltaTime;
			if(refireTimer < 0)
			{
				refireTimer = 0;
			}
		}
	
	}
	
	public void Fire()
	{
		if(refireTimer == 0)
		{
			refireTimer = refireTime;
			
			for(int shotLoop = 0; shotLoop < shotQuantity; shotLoop++)
			{
			
				 GameObject newBullet = GameObject.Instantiate(bullet) as GameObject;
				
				newBullet.transform.position = firePoint.gameObject.transform.position;
				
				//get component and set properties
				newBullet.GetComponent<Bullet>().damage = damage;
				newBullet.GetComponent<Bullet>().lifeTime = projectileLifetime;
				
				Vector3 randomVariance = new Vector3(Random.Range(-fireAngleVariance.x,fireAngleVariance.x),
								Random.Range(-fireAngleVariance.y,fireAngleVariance.y),
								Random.Range(-fireAngleVariance.z,fireAngleVariance.z));
				
				newBullet.rigidbody.AddRelativeForce((firePoint.gameObject.transform.forward + randomVariance) * projectileSpeed); 
			}
		}
	}
	
	
}
