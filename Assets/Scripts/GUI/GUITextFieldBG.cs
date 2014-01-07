using UnityEngine;
using System.Collections;

public class GUITextFieldBG : MonoBehaviour
{
	public GUITextField textField;

	public void OnMouseDown()
	{
		Debug.Log ("ARGH");
		this.textField.selected = true;
	}
}
