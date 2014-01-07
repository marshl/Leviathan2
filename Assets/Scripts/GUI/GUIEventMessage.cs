using UnityEngine;
using System.Collections;

public class GUIEventMessage : GUIMessage
{
	public enum MESSAGE_MODE
	{
		ON_MOUSE_DOWN,
		ON_MOUSE_DRAG, 	
		ON_MOUSE_ENTER,
		ON_MOUSE_EXIT, 
		ON_MOUSE_OVER,
		ON_MOUSE_UP,
		ON_MOUSE_UP_AS_BUTTON,
	}
	
	public MESSAGE_MODE mode;

	protected virtual void Start()
	{
		// DO NOT DELETE
		// Allows the enabling/disabling of this script
	}
	
	public void OnMouseDown()
	{
		if ( this.mode == MESSAGE_MODE.ON_MOUSE_DOWN )
		{
			this.EventTrigger();
		}
	}
	
	public void OnMouseDrag()
	{
		if ( this.mode == MESSAGE_MODE.ON_MOUSE_DRAG )
		{
			this.EventTrigger();
		}
	}
	
	public void OnMouseEnter()
	{
		if ( this.mode == MESSAGE_MODE.ON_MOUSE_ENTER )
		{
			this.EventTrigger();
		}
	}
	
	public void OnMouseExit()
	{
		if ( this.mode == MESSAGE_MODE.ON_MOUSE_EXIT )
		{
			this.EventTrigger();
		}
	}
	
	public void OnMouseOver()
	{
		if ( this.mode == MESSAGE_MODE.ON_MOUSE_OVER )
		{
			this.EventTrigger();
		}
	}
	
	public void OnMouseUp()
	{
		if ( this.mode == MESSAGE_MODE.ON_MOUSE_UP )
		{
			this.EventTrigger();
		}
	}
	
	public void OnMouseUpAsButton()
	{
		if ( this.mode == MESSAGE_MODE.ON_MOUSE_UP_AS_BUTTON )
		{
			this.EventTrigger();
		}
	}
}
