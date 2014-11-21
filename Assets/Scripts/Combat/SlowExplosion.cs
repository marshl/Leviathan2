using UnityEngine;
using System.Collections;

public class SlowExplosion : MonoBehaviour {

	public float startRadius = 0.1f;
	public float endRadius = 15.0f;
	public float duration = 6.0f;
	public float damageInterval = 0.2f;
	public int damagePerSecond = 100;

	private float counter = 0.0f;
	private float durationTimer = 0.0f;
	private float currentRadius = 0.1f;
	private bool damageless = false;

	public BaseWeaponManager source;

	// Use this for initialization
	void Start () {
	
	}

	void OnNetworkInstantiate(NetworkMessageInfo info)
	{
		if(!this.networkView.isMine)
		{
			damageless = true;
		}
	}
	
	// Update is called once per frame
	void Update () {

		counter += Time.deltaTime;
		durationTimer += Time.deltaTime; //Keeping it simple here

		currentRadius = Mathf.Lerp (startRadius, endRadius, durationTimer/duration);

		this.transform.localScale = new Vector3(currentRadius, currentRadius, currentRadius);


		if(counter >= 0.2f && !damageless)
		{
			counter -= 0.2f;

			TargetManager.instance.AreaOfEffectDamage(this.transform.position, currentRadius, damagePerSecond * damageInterval, true, source.health.Owner);
			//foreach (BaseHealth health in GameObject.FindObjectOfType<BaseHealth>())
			//{
			//	if((health.transform.position - this.transform.position).magnitude() < currentRadius)
			//	{
					//hit it m8

			//	}
			//}
		}

		if(durationTimer >= duration)
		{
			Network.Destroy (this.gameObject);
		}
	
	}
}
