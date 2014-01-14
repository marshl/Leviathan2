using UnityEngine;
using System.Collections;

public class GUIColor : GUILerp
{
	public Color hoverColour = Color.white;
	public Color pressedColour = Color.white;
	private Color normalColour;

	private Color oldColour;
	private Color currentColour;

	protected override void Awake()
	{
		base.Awake();
		if ( this.targetText != null )
		{
			this.normalColour = this.targetText.color;
		}
		else if ( this.targetTexture != null )
		{
			this.normalColour = this.targetTexture.color;
		}

		this.currentColour = this.oldColour = this.normalColour;
	}
	
	protected override void Update ()
	{
		base.Update();

		switch ( this.currentState )
		{
		case STATE.TO_HOVER:
		{
			this.LerpColour( this.hoverColour );
			break;
		}
		case STATE.TO_PRESSED:
		{
			this.LerpColour( this.pressedColour );
			break;
		}
		case STATE.TO_NORMAL:
		{
			this.LerpColour( this.normalColour );
			break;
		}
		case STATE.NONE:
		{
			break;
		}
		}
	}

	protected void LerpColour( Color _colourTo )
	{
		if ( this.currentDelta > 1.0f )
		{
			this.currentDelta = 0.0f;
			this.currentState = STATE.NONE;
			this.oldColour = _colourTo;
		}
		else
		{
			this.currentColour = Color.Lerp( this.oldColour, _colourTo, this.currentDelta );
		}

		this.SetCurrentColour( this.currentColour );
	}

	protected override void SwitchState( STATE _state )
	{
		base.SwitchState( _state );

		if ( this.currentState != _state )
		{
			this.oldColour = this.currentColour;
		}
	}
	
	private void SetCurrentColour( Color _colour )
	{
		if ( this.targetText != null )
		{
			this.targetText.color = _colour;
		}
		else if ( this.targetTexture != null )
		{
			this.targetTexture.color = _colour;
		}
	}
}
