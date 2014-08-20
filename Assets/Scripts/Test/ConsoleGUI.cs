using UnityEngine;
using System.Collections;

public class ConsoleGUI : MonoBehaviour
{
	private bool displaying = false;

	public GUIStyle style;
	private int linesLong;
	public float lineHeight;

	public float sliderValue = 0.0f;

	private bool pickingObject = false;

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.BackQuote ) )
		{
			this.displaying = !this.displaying;
		}

		if ( this.displaying )
		{
			this.linesLong = (int)(( (float)Screen.height * 0.8f ) / lineHeight);
			if ( DebugConsole.input.Length > 0 )
			{
				foreach ( char c in Input.inputString )
				{
					if ( (int)c == 13 ) // New Line
					{
						DebugConsole.ProcessInput();

						this.sliderValue = DebugConsole.outputLines.Count - this.linesLong;
						this.sliderValue = this.sliderValue > 0.0f ? this.sliderValue : 0.0f;
						break;
					}
				}
			}

			if ( Input.GetKeyDown( KeyCode.UpArrow ) || Input.GetKeyDown( KeyCode.DownArrow ) )
			{
				if ( Input.GetKeyDown( KeyCode.UpArrow ) )
				{
					DebugConsole.currentInputLine = Mathf.Max( DebugConsole.currentInputLine - 1, 0 );
				}
				if ( Input.GetKeyDown( KeyCode.DownArrow ) )
				{
					DebugConsole.currentInputLine = Mathf.Min( DebugConsole.currentInputLine + 1, DebugConsole.inputLines.Count );
					
				}

				// Index into the old input strings
				if ( DebugConsole.currentInputLine < DebugConsole.inputLines.Count  )
				{
					DebugConsole.input = DebugConsole.inputLines[DebugConsole.currentInputLine];
				}
				else // If we go over the end of the list, go back to blank input
				{
					DebugConsole.input = "";
				}
			}
		}

		if ( this.pickingObject )
		{
			if ( Input.GetMouseButtonDown( 0 ) )
			{
				Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
				RaycastHit info;
				if ( Physics.Raycast( ray, out info ) )
				{
					DebugConsole.pickedObject = info.collider.gameObject;
				}
				else
				{
					DebugConsole.pickedObject = null;
				}
				this.pickingObject = false;
			}
			else if ( Input.GetMouseButtonDown( 1 ) )
			{
				this.pickingObject = false;
			}
		}
	}

	private void OnGUI()
	{
		if ( this.displaying )
		{
			int lineOverflow = DebugConsole.outputLines.Count - this.linesLong;
			lineOverflow = lineOverflow > 0 ? lineOverflow : 0;

			// Output lines
			for ( int i = 0; i < DebugConsole.outputLines.Count && i < this.linesLong; ++i )
			{
				int lineIndex = i + (int)this.sliderValue;
				GUI.Label(  new Rect( 0.0f, (float)i * lineHeight, Screen.width, lineHeight ), DebugConsole.outputLines[lineIndex], this.style );
			}
			// Object picker


			if ( DebugConsole.pickedObject != null )
			{
				GUI.Label( new Rect( 0.0f, Screen.height * 0.8f, Screen.width/2, Screen.height * 0.1f ), DebugConsole.pickedObject.name );
			}

			if ( this.pickingObject )
			{
				GUI.enabled = false;
			}
			if ( GUI.Button( new Rect( Screen.width / 2, Screen.height * 0.8f, Screen.width/2, Screen.height*0.1f ), "Pick Object" ) )
			{
				this.pickingObject = true;
			}
			GUI.enabled = true;

			// Input field
			DebugConsole.input = GUI.TextField( new Rect( 0.0f, Screen.height*0.9f, Screen.width, Screen.height * 0.1f ), DebugConsole.input );

			if ( DebugConsole.newLine )
			{
				DebugConsole.newLine = false;
				this.sliderValue = lineOverflow;
			}

			// Output slider
			if ( lineOverflow > 0 )
			{
				this.sliderValue = Mathf.Floor( GUI.VerticalSlider( new Rect(Screen.width-10, 0, 10, Screen.height*0.9f), this.sliderValue, 0, lineOverflow ) );
			}
		}
	}
}
