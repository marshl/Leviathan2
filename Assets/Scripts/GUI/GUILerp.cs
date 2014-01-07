using UnityEngine;
using System.Collections;

public abstract class GUILerp : MonoBehaviour {

	public GUIElement target;
	protected GUITexture targetTexture;
	protected GUIText targetText;

	public float duration = 1.0f;
	protected float currentDelta = 0.0f;

	protected enum STATE
	{
		TO_HOVER,
		TO_PRESSED,
		TO_NORMAL,
		NONE,
	}
	protected STATE currentState = STATE.NONE;

	protected virtual void Awake()
	{
		this.targetTexture = this.target as GUITexture;
		this.targetText = this.target as GUIText;
		if ( this.targetTexture == null && this.targetText == null )
		{
			Debug.LogError( "Target for GUIColor needs to be iehter GUITexture or GUIText.", this );
		}
	}

	protected virtual void Update ()
	{
		if ( this.currentState != STATE.NONE )
		{
			this.currentDelta += Time.deltaTime / this.duration;
		}
	}

	protected virtual void OnMouseEnter()
	{
		this.SwitchState( STATE.TO_HOVER );
	}

	protected virtual void OnMouseDown()
	{
		this.SwitchState( STATE.TO_PRESSED );
	}

	protected virtual void OnMouseExit()
	{
		this.SwitchState( STATE.TO_NORMAL );
	}

	protected virtual void SwitchState( STATE _state )
	{
		if ( this.currentState != _state )
		{
			this.currentDelta = 0.0f;
			this.currentState = _state;
		}
	}
}
