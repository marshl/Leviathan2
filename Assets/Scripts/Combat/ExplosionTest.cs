using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExplosionTest : MonoBehaviour
{
	public int chunkCount;
	public Vector2 chunkForce;
	public Vector2 chunkSpin;
	public Vector2 shrinkRate;
	public Vector2 startingScale;

	public GameObject chunkPrefab;

	private List<GameObject> chunks;

	private void Start()
	{
		this.Kaboom();
	}

	private void Kaboom()
	{
		if ( this.chunks != null )
		{
			for ( int i = 0; i < this.chunkCount; ++i )
			{
				GameObject.Destroy( this.chunks[i] );
			}
		}

		this.chunks = new List<GameObject>();

		for ( int i = 0; i < this.chunkCount; ++i )
		{
			GameObject chunk = GameObject.Instantiate( this.chunkPrefab, this.transform.position, this.transform.rotation ) as GameObject;
			chunk.transform.parent = this.transform;
			chunk.transform.localScale = Vector3.one * Random.Range( this.startingScale.x, this.startingScale.y );

			chunk.GetComponent<Rigidbody>().AddForce( Common.RandomDirection() * Random.Range( this.chunkForce.x, this.chunkForce.y ) );
			chunk.GetComponent<Rigidbody>().AddTorque( Common.RandomDirection() * Random.Range( this.chunkSpin.x, this.chunkSpin.y ) );
			this.chunks.Add( chunk );
		}
	}

	private void Update()
	{
		if ( Input.GetKeyDown (KeyCode.Space ) )
		{
			this.Kaboom();
		}

		for ( int i = 0; i < this.chunkCount; ++i )
		{
			if ( this.chunks[i] == null )
			{
				continue;
			}

			//GameObject chunk = this.chunks[i];

			//chunk.transform.localScale *= (1.0f - Time.deltaTime * Random.Range( this.shrinkRate.x, this.shrinkRate.y ) );
		}
	}
}
