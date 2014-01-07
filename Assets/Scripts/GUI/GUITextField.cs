using UnityEngine;
using System.Collections;

public class GUITextField : MonoBehaviour
{
	public GUIText textScript;
	public int characterLimit = 50;

	public float blinkRate = 0.25f;
	private float blinkTimer = 0.0f;

	private string text = "";

	public bool selected = false;
	private bool mouseIsHovering = false;

	private Color normalColour;
	public Color unselectedColour;
	public Color textlessColour;

	private void Awake()
	{
		this.normalColour = this.textScript.color;
		this.text = this.textScript.text;
	}

	private void Update()
	{
		if ( Input.GetMouseButtonDown( 0 ) && this.mouseIsHovering == false )
		{
			this.selected = false;
		}

		if ( this.selected == false )
		{
			if ( this.text == "" )
			{
				this.textScript.text = "Click here to type.";
				this.textScript.color = this.textlessColour;
			}
			else
			{
				this.textScript.text = this.text;
				this.textScript.color = this.unselectedColour;
			}
			return;
		}

		foreach ( char c in Input.inputString )
		{
			if ( c == '\b' )
			{
				if ( this.text.Length > 0 )
				{
					this.text = this.text.Substring( 0, this.text.Length - 1 );
				}
			}
			else if ( this.text.Length < this.characterLimit )
			{
				this.text += c;
			}
		}

		this.blinkTimer += Time.deltaTime;
		if ( this.blinkTimer > this.blinkRate )
		{
			this.blinkTimer = -this.blinkRate;
		}

		this.textScript.text = this.text + (this.blinkTimer > 0.0f ? '|' : ' ');
		this.textScript.color = this.normalColour;
	}

	private void OnMouseEnter()
	{
		this.mouseIsHovering = true;
	}

	private void OnMouseExit()
	{
		this.mouseIsHovering = false;
	}

	private void OnMouseDown()
	{
		this.selected = true;
	}

	/*public void OnGUI()
	{
		if ( Event.current.type == EventType.KeyDown
		  && Event.current.keyCode != KeyCode.None )
		{
			KeyPressedEventHandler();
		}
	}

	private void KeyPressedEventHandler()
	{
		Debug.Log( Event.current.keyCode );
		this.text += Event.current.keyCode.ToString();
	}
	*/
}
