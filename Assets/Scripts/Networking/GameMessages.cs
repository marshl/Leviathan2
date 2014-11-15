using UnityEngine;
using System.Collections;

public class GameMessages : MonoBehaviour
{
	public static GameMessages instance;

	/*public enum DISPLAY_MODE
	{
		NONE,
		INPUT,
		INPUT_AND_OUTPUT,
	}*/

	public bool displayingOutput = false;

	//public DISPLAY_MODE displayMode;

	public Texture2D messageReceivedOnTexture;
	public Texture2D messageReceivedOffTexture;

	public bool sendToAll; //False: Send to team

	public string message = "";

	private GamePlayer privateMessageReceiver;

	public float fieldWidth;
	public float outputFieldHeight;
	public float inputHeight;

	public float leftGap;
	public float bottomGap;

	public float buttonHeight;

	public float indicatorSize;

	public Vector2 sendToggleButtonSize;

	//private int inputInTransition;
	public int outputInTransition;

	//public float inputTransitionDuration;
	public float outputTransitionDuration;
	private float transitionTime;

	private void Awake()
	{
		GameMessages.instance = this;
	}

	private void Update()
	{
		if ( this.outputInTransition != 0 )
		{
			this.transitionTime += Time.deltaTime;
			if ( this.transitionTime >= this.outputTransitionDuration )
			{
				this.transitionTime = 0.0f;
				this.displayingOutput = this.outputInTransition > 0;
				this.outputInTransition = 0;

			}
		}
	}

	private void OnGUI()
	{
		float outputTransitionDelta = this.displayingOutput ? 1.0f : 0.0f;
		if ( this.outputInTransition != 0 )
		{
			outputTransitionDelta = this.outputInTransition > 0
				? this.transitionTime / this.outputTransitionDuration
				: 1.0f - this.transitionTime / this.outputTransitionDuration;
		}

		float outputY = Mathf.Lerp( Screen.height - this.bottomGap - this.inputHeight,
		                     Screen.height - this.bottomGap - this.outputFieldHeight - this.inputHeight,
		                           outputTransitionDelta );

		/// INPUT FIELD
		GUI.SetNextControlName( "ChatEnterField" );
		this.message = GUI.TextField( new Rect( this.leftGap, Screen.height - this.inputHeight - this.bottomGap,
		                                       this.fieldWidth, this.inputHeight ), this.message );
		if ( Event.current.keyCode == KeyCode.Return )
		{
			if ( GUI.GetNameOfFocusedControl() == "ChatEnterField" )
			{
				this.OnSendMessage();
			}
			else
			{
				GUI.FocusControl( "ChatEnterField" );
			}
		}


		/// OUTPUT FIELD
		if ( this.outputInTransition != 0 )
		{
			GUI.TextArea( new Rect( this.leftGap, outputY,
			                       this.fieldWidth, this.outputFieldHeight ),
			             string.Empty );
		}
		else if ( this.displayingOutput )
		{
			string chat = "";
			for ( int i = MessageManager.instance.messages.Count - 1; i >= 0; --i )
			{
				chat += MessageManager.instance.GetFormattedMessage( i );
			}

			GUIStyle style = GUI.skin.textArea;
			style.richText = true;
			GUI.TextArea( new Rect( this.leftGap, outputY,
			                       this.fieldWidth, this.outputFieldHeight ),
			             chat, style );

			if ( GUI.Button( new Rect( this.leftGap + this.fieldWidth - this.sendToggleButtonSize.x - this.indicatorSize * 2,
			                          outputY - this.sendToggleButtonSize.y,
			                          this.sendToggleButtonSize.x, this.sendToggleButtonSize.y ),
			                this.sendToAll ? "ALL" : "TEAM" ) )
			{
				this.sendToAll = !this.sendToAll;
			}
		}

		bool openCloseButton = GUI.Button( new Rect( this.leftGap + this.fieldWidth - this.indicatorSize * 2, outputY - this.indicatorSize,
		                                            this.indicatorSize, this.indicatorSize ), "^" );

		GUI.DrawTexture( new Rect( this.leftGap + this.fieldWidth - this.indicatorSize, outputY - this.indicatorSize,
		                          this.indicatorSize, this.indicatorSize ),
		                               MessageManager.instance.newMessages ? this.messageReceivedOnTexture : this.messageReceivedOffTexture );

		if ( ( openCloseButton || Input.GetKeyDown( KeyCode.Backslash ) )
		    && this.outputInTransition == 0 )
		{
			this.outputInTransition = this.displayingOutput ? -1 : 1;
			MessageManager.instance.newMessages = false;
		}
	}

	public void StartPrivateMessage( GamePlayer _receiver )
	{
		this.privateMessageReceiver = _receiver;

		if ( this.outputInTransition == 0 && !this.displayingOutput )
		{
			this.outputInTransition = 1;
		}
	}

	public void CancelPrivateMessage()
	{
		this.privateMessageReceiver = null;
	}

	private void OnSendMessage()
	{
		if ( this.message != string.Empty )
		{
			if ( this.privateMessageReceiver != null )
			{
				MessageManager.instance.CreateMessageLocal( this.message, MESSAGE_TYPE.PRIVATE, this.privateMessageReceiver.id );
			}
			else if ( this.sendToAll )
			{
				MessageManager.instance.CreateMessageLocal( this.message, MESSAGE_TYPE.TO_ALL );
			}
			else
			{
				MessageManager.instance.CreateMessageLocal( this.message, MESSAGE_TYPE.TO_TEAM );
			}
			this.message = "";
		}
	}
}
