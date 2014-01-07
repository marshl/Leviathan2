using UnityEngine;
using System.Collections;

public class GUIOffset : GUILerp
{
	public Vector2 hoverOffset = new Vector2( 5, 5 );
	public Vector2 pressedOffset = new Vector2( 10, 10 );

	private Vector2 normalPosition;
	private Vector2 oldPosition = Vector3.zero;
	private Vector2 currentOffset;

	protected override void Awake()
	{
		base.Awake();

		this.oldPosition = this.normalPosition = this.currentOffset = this.GetCurrentOffset();
	}
	
	protected override void Update ()
	{
		base.Update();

		switch ( this.currentState )
		{
		case STATE.TO_HOVER:
		{
			this.LerpPosition( this.normalPosition + this.hoverOffset );
			break;
		}
		case STATE.TO_PRESSED:
		{
			this.LerpPosition( this.normalPosition + this.pressedOffset );
			break;
		}
		case STATE.TO_NORMAL:
		{
			this.LerpPosition( this.normalPosition );
			break;
		}
		case STATE.NONE:
		{
			break;
		}
		}
	}

	protected void LerpPosition( Vector3 _pos )
	{
		if ( this.currentDelta > 1.0f )
		{
			this.currentDelta = 0.0f;
			this.currentState = STATE.NONE;
			this.oldPosition = this.currentOffset = _pos;
		}
		else
		{
			this.currentOffset = Vector3.Lerp( this.oldPosition, _pos, this.currentDelta );
		}

		this.SetOffset();
	}

	protected override void SwitchState( STATE _state )
	{
		base.SwitchState( _state );

		if ( this.currentState != _state )
		{
			this.oldPosition = this.transform.localPosition;
		}
	}

	private void SetOffset()
	{
		if ( this.targetText != null )
		{
			this.targetText.pixelOffset = this.currentOffset;
		}
		else if ( this.targetTexture != null )
		{
			this.targetTexture.pixelInset = new Rect(
				this.currentOffset.x, this.currentOffset.y,
				this.targetTexture.pixelInset.width, this.targetTexture.pixelInset.height );
		}
	}

	private Vector2 GetCurrentOffset()
	{
		if ( this.targetText != null )
		{
			return this.targetText.pixelOffset;
		}
		else if ( this.targetTexture != null )
		{
			return new Vector2( this.targetTexture.pixelInset.x, this.targetTexture.pixelInset.y );
		}
		else
		{
			Debug.LogError( "No texture or text defined on GUI Offset", this );
			return Vector2.zero;
		}
	}
}
