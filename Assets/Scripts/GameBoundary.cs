using UnityEngine;
using System.Collections;

public class GameBoundary : MonoBehaviour
{
	public static GameBoundary instance;

	public Vector3 origin;
	public float radius;
	public int lineVertices;

	public void Awake()
	{
		GameBoundary.instance = this;

		LineRenderer line = this.GetComponent<LineRenderer>();
		line.SetVertexCount( this.lineVertices );
		for ( int i = 0; i < this.lineVertices; ++i )
		{
			float angle = (float)i * Mathf.PI * 2.0f / (float)(this.lineVertices - 1);
			Vector3 offset = new Vector3( Mathf.Sin( angle ),  0.0f, Mathf.Cos( angle ) );
			line.SetPosition( i, offset * this.radius + this.origin );
		}
	} 
}
