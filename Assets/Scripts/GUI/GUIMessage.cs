using UnityEngine;
using System.Collections;

public class GUIMessage : MonoBehaviour
{
	public MonoBehaviour target;
	public string methodName;
	public string value;
	public SendMessageOptions receiverRequired;

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
		
		this.target.SendMessage( methodName, value,this.receiverRequired );
	}
}
