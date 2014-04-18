using UnityEngine;
using System.Collections;

public class GUIMessage : MonoBehaviour
{
	public MonoBehaviour target;
	public string methodName;
	public string value;
	public SendMessageOptions receiverRequired;
	public bool parseAsInt = false;
	private int integerResult;

	protected void EventTrigger()
	{
		if ( this.enabled == false )
		{
			return;
		}

		if ( this.target == null || this.methodName == "" )
		{
			if ( this.receiverRequired == SendMessageOptions.RequireReceiver )
			{
				Debug.LogError( "GUIMessage with receiver requirement has no target or message set.", this );
			}
			return;
		}

		if(int.TryParse(value, out integerResult) && this.parseAsInt)
		{
			this.target.SendMessage( methodName, integerResult,this.receiverRequired );
		}
		else
		{
			this.target.SendMessage( methodName, value,this.receiverRequired );
		}
	}
}
