using UnityEngine;
using System.Collections;

public class FusionBeam : MonoBehaviour {

	public int maxRange;
	public TEAM team;
	public int damagePerSecond;
	public float energyCostPerSecond;
	public float duration;
	public Transform sourcePosition;

	// Use this for initialization
	void Start () {

		this.SingleEdgeForwardScale(30);
	
	}
	
	// Update is called once per frame
	void Update () {

	
	}

	void GetHitObjects()
	{
		RaycastHit[] sphereInfo;
		//float distance = Vector3.Distance( this.lastPosition, this.transform.position );
		int layerMask = ~(1 << 8 | 1 << 10);

		float sphereRadius = this.GetComponent<SphereCollider>().radius;

		sphereInfo = Physics.SphereCastAll( sourcePosition.position, sphereRadius, 
		                                   transform.forward, maxRange, layerMask );

		Debug.DrawLine(sourcePosition.position, transform.forward * maxRange);

		foreach ( RaycastHit hit in sphereInfo )
		{
			//this.transform.position = hit.point;
			//DebugConsole.Log("Sphere collision with " + hit.collider.name + " at distance " + distance);
			this.CheckCollision( hit.collider );
			//this.specialCollision = true;
			//BulletManager.instance.DestroyLocalBullet (this);
		}
	}

	void CheckCollision(Collider _collision)
	{
		if(_collision.gameObject.layer == 13)
		{
			//We hit a capital ship bro
		}
	}

	void SingleEdgeForwardScale(float scale)
	{
		Vector3 newScale = new Vector3(this.transform.localScale.x, scale, this.transform.localScale.z);
		this.transform.localScale = newScale;

		Vector3 newPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y , this.transform.localPosition.z + scale);
		this.transform.localPosition = (newPosition);
	}
}
