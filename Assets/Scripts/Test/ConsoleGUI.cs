using UnityEngine;
using System.Collections;

public class ConsoleGUI : MonoBehaviour
{
	private bool displaying = false;

	public GUIStyle style;
	private int linesLong;
	public float lineHeight;

	public float sliderValue = 0.0f;

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.BackQuote ) )
		{
			this.displaying = !this.displaying;
		}

		if ( this.displaying && DebugConsole.input.Length > 0 )
		{
			this.linesLong = (int)(( (float)Screen.height * 0.9f ) / lineHeight);

			foreach ( char c in Input.inputString )
			{
				if ( (int)c == 13 )
				{
					DebugConsole.ProcessInput();

					this.sliderValue = DebugConsole.outputLines.Count - this.linesLong;
					this.sliderValue = this.sliderValue > 0.0f ? this.sliderValue : 0.0f;
					break;
				}
			}
		}
	}

	private void OnGUI()
	{
		if ( this.displaying )
		{
			int lineOverflow = DebugConsole.outputLines.Count - this.linesLong;
			lineOverflow = lineOverflow > 0 ? lineOverflow : 0;

			for ( int i = 0; i < DebugConsole.outputLines.Count && i < this.linesLong; ++i )
			{
				int lineIndex = i + (int)this.sliderValue;
				GUI.Label(  new Rect( 0.0f, (float)i * lineHeight, Screen.width, lineHeight ), DebugConsole.outputLines[lineIndex], this.style );
			}

			DebugConsole.input = GUI.TextField( new Rect( 0.0f, Screen.height*0.9f, Screen.width, Screen.height * 0.1f ), DebugConsole.input );

			if ( lineOverflow > 0 )
			{
				this.sliderValue = GUI.VerticalSlider( new Rect(Screen.width-10, 0, 10, Screen.height*0.9f), this.sliderValue, 0, lineOverflow );
			}
		}
	}
}
