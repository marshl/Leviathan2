using UnityEngine;
using System.Collections;
using System;

[Obsolete]
public class Bullet : MonoBehaviour {
	
	public int damage;
	public float lifeTime;
	

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	lifeTime -= Time.deltaTime;
		if(lifeTime <= 0)
		{
			DestroyObject(this.gameObject);
		}
	}
	
	void OnCollisionEnter(Collision collision)
	{
		//collision.gameObject.GetComponent(healthScript).doDamate(damage)
			Destroy(this.gameObject);
	}
}
