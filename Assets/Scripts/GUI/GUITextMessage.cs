using UnityEngine;
using System.Collections;

[RequireComponent( typeof(GUITextField) ) ]
public class GUITextMessage : GUIMessage
{
	private GUITextField textField;

	private void Awake()
	{
		this.textField = this.GetComponent<GUITextField>();
	}

	private void Update()
	{
		if ( this.textField.selected )
		{
			foreach ( char c in Input.inputString )
			{
				if ( (int)c == 13 ) // If the enter key is pressed
				{
					this.EventTrigger();
				}
			}
		}
	}
}
