using UnityEngine;
using System.Collections;

public class MuzzleFlash : MonoBehaviour
{
	public Renderer flash;
	public float flashDuration;
	private float flashTimer = 0.0f;

	private void Update()
	{
		if ( this.flashTimer < this.flashDuration )
		{
			this.flashTimer += Time.deltaTime;
		}
		else
		{
			this.flash.enabled = false;
		}
	}
	
	public void OnFire()
	{
		this.GetComponent<AudioSource>().Play();
		this.flash.enabled = true;
		this.flashTimer = 0.0f;
		if ( this.GetComponent<ParticleSystem>() != null )
		{
			this.GetComponent<ParticleSystem>().Play();
		}
	}
}
