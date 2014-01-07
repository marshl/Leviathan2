using UnityEngine;
using System.Collections;

public class GUIScale : MonoBehaviour
{
	// Public Variables
	public float maxScale = 2.0f;
	public float scaleRate = 1.0f;

	// Private Variables
	private Vector3 startingScale;
	private bool mouseOver = false;

	private void Awake()
	{
		this.startingScale = this.transform.localScale;
	}

	public void OnMouseOver()
	{
		this.mouseOver = true;
	}

	public void OnMouseExit()
	{
		this.mouseOver = false;
	} 

	public void Update()
	{
		this.transform.localScale = Vector3.Lerp
		( 
			 this.transform.localScale, this.mouseOver ? this.startingScale * this.maxScale : this.startingScale, Time.deltaTime * this.scaleRate 
		);
	}
}
