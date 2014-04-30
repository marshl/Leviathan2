using UnityEngine;
using System.Collections;

public class RotateObject : MonoBehaviour
{
	public Vector3 axis;
	public float rotationRate;

	private void Update()
	{
		this.transform.Rotate( this.axis, this.rotationRate * Time.deltaTime );
	}
}
