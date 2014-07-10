using UnityEngine;
using System.Collections;

public class GUITextFieldBG : MonoBehaviour
{
	public GUITextField textField;

	public void OnMouseDown()
	{
		DebugConsole.Log( "ARGH" );
		this.textField.selected = true;
	}
}
