using UnityEngine;
using System.Collections;

public class GUITextScale : MonoBehaviour
{
	private int startingSize;
	
	public int maxSize = 2;
	public float scaleRate = 1.0f;

	private float fontSize;
	private bool mouseOver = false;
	
	private void Awake()
	{
		this.startingSize = this.guiText.fontSize;
		this.fontSize = (float)this.startingSize;
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
		this.fontSize = Mathf.Lerp
		( 
			 this.fontSize, this.mouseOver ? this.maxSize : this.startingSize, Time.deltaTime * this.scaleRate 
		);
		this.guiText.fontSize = (int)this.fontSize;
	}
}