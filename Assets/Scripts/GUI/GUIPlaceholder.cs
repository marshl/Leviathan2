using UnityEngine;
using System.Collections;

[System.Serializable]
public class GUIPlaceholder
{
	public Rect rect;
	public string text = "";
	
	public GUIPlaceholder(){}
	
	public GUIPlaceholder( Rect _rect, string _text )
	{
		this.rect = _rect;
		this.text = _text;
	}
	
	public void Label()
	{
		GUI.Label( this.rect, this.text );
	}
	
	public bool Button()
	{
		return GUI.Button( this.rect, this.text );
	}
	
	public string TextField( int _maxLength )
	{
		return this.text = GUI.TextField( this.rect, this.text, _maxLength );
	}
};