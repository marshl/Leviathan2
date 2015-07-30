using UnityEngine;
using System.Collections;

public class TractorBeam : MonoBehaviour
{
	public enum TractorFunction
	{
		PUSH,
		PULL,
		HOLD
	};

	public CapitalShipMaster master;
	public float chargePercentage;
	public float chargeDrain;
	public float chargeRegeneration;
	//public float maximumRange = 100000.0f;
	public float strengthMultiplier;
	public float dragOverride;

	private bool tractorActive = false;
	private GameObject currentTarget;
	private int tractorDirection = 0;

	private float targetOriginalDrag;
	private float targetOriginalAngularDrag;

	private Rect tractorStatusLableRect;
	private Rect tractorPushLableRect;
	private Rect tractorHoldLableRect;
	private Rect tractorPullLableRect;
	private Rect tractorDisengageLableRect;

	private bool tractorUITargetting = false;
	private TractorFunction tractorUITargetType;


	private void Start()
	{
		tractorStatusLableRect = new Rect(40, Screen.height / 2 + 40, 140, 30);
		tractorPushLableRect = new Rect(40, Screen.height / 2 , 140, 30);
		tractorHoldLableRect = new Rect(40, Screen.height / 2 - 40, 140, 30);
		tractorPullLableRect = new Rect(40, Screen.height / 2 - 80, 140, 30);
		tractorDisengageLableRect = new Rect(40, Screen.height / 2 - 120, 140, 30);
	}

	private void Update()
	{
#if UNITY_EDITOR
		if ( this.master.isDummyShip == false )
#endif
		{
			if ( this.GetComponent<NetworkView>().isMine || Network.peerType == NetworkPeerType.Disconnected ) 
			{
				if ( this.tractorUITargetting && Input.GetMouseButtonDown(1) ) //Right click
				{
					RaycastHit hit;
					Ray toCast = Camera.main.ScreenPointToRay( Input.mousePosition );

					if( Physics.Raycast( toCast, out hit ) )
					{
						if ( this.tractorActive )
						{
							this.StopTractor();
						}

						if ( hit.collider.GetComponent<FighterMaster>() != null )
						{
							Debug.Log( "Hit " + hit.collider.gameObject.name + " with tractor beam" );
							FireAtTarget( hit.collider.gameObject, tractorUITargetType );
							if ( Network.peerType != NetworkPeerType.Disconnected )
							{
							GameNetworkManager.instance.SendTractorStartMessage( this.GetComponent<NetworkView>().viewID, tractorUITargetType, hit.collider.GetComponent<NetworkView>().viewID );
							}
						}
						else
						{
							Debug.Log( "Target is not a fighter", hit.collider );
						}
					}
				}
			}
		}
	}

	private void FixedUpdate()
	{
		if(tractorActive)
		{
			Vector3 forceVector = (currentTarget.transform.position - this.transform.position).normalized;
			//Vector3 forceVector = new Vector3(1000000000,0,0);

			forceVector *= tractorDirection * strengthMultiplier * dragOverride;

			DebugConsole.Log ("Force vector is " + forceVector.ToString());
			DebugConsole.Log ("Magnitude is " + forceVector.magnitude.ToString());

			currentTarget.GetComponent<Rigidbody>().AddForce (forceVector);

			DebugConsole.Log ("Target name is " + currentTarget.name);
			chargePercentage -= chargeDrain * Time.fixedDeltaTime;

			if(chargePercentage <= 0)
			{
				chargePercentage = 0;
				if(this.GetComponent<NetworkView>().isMine)
				{
					StopTractor();
					DebugConsole.Log ("Stopped tractor");
				}
			}
		}

		if(chargePercentage < 100.0f)
		{
			chargePercentage += (chargeRegeneration * Time.fixedDeltaTime);
			if(chargePercentage > 100.0f)
			{
				chargePercentage = 100.0f;
			}
		}
	}

	private void OnGUI()
	{
#if UNITY_EDITOR 
		if ( !this.master.isDummyShip )
#endif
		{
			if ( this.GetComponent<NetworkView>().isMine || Network.peerType == NetworkPeerType.Disconnected )
			{
				GUI.Label(tractorStatusLableRect,"Tractor: " + chargePercentage + "%");
				if( GUI.Button(tractorPushLableRect, "Engage push"))
				{
					tractorUITargetting = true;
					tractorUITargetType = TractorFunction.PUSH;
					print("Select target");
				}
				if(GUI.Button(tractorHoldLableRect, "Engage hold"))
				{
					tractorUITargetting = true;
					tractorUITargetType = TractorFunction.HOLD;
					print("Select target");
				}
				if(GUI.Button(tractorPullLableRect, "Engage pull"))
				{
					tractorUITargetting = true;
					tractorUITargetType = TractorFunction.PULL;
					print("Select target");
				}
				if(GUI.Button(tractorDisengageLableRect, "Disengage"))
				{
					StopTractor();
				}
			}
		}
	}
	

	public void FireAtTarget(GameObject target, TractorFunction type)
	{
		switch( type )
		{
		case TractorFunction.HOLD:
		{
				targetOriginalDrag = target.GetComponent<Rigidbody>().drag;
				targetOriginalAngularDrag = target.GetComponent<Rigidbody>().angularDrag;
				target.GetComponent<Rigidbody>().drag = dragOverride;
				target.GetComponent<Rigidbody>().angularDrag = dragOverride;

				tractorDirection = 0;
				currentTarget = target.gameObject;
				tractorActive = true;

			DebugConsole.Log ("Tractor beam activated in Hold mode");
			break;
		}
		case TractorFunction.PULL:
		{
			targetOriginalDrag = target.GetComponent<Rigidbody>().drag;
			targetOriginalAngularDrag = target.GetComponent<Rigidbody>().angularDrag;
			target.GetComponent<Rigidbody>().drag = dragOverride;
			target.GetComponent<Rigidbody>().angularDrag = dragOverride;
			
			tractorDirection = -1;
			currentTarget = target.gameObject;
			tractorActive = true;

			DebugConsole.Log ("Tractor beam activated in Pull mode");
			break;
		}
		case TractorFunction.PUSH:
		{
			targetOriginalDrag = target.GetComponent<Rigidbody>().drag;
			targetOriginalAngularDrag = target.GetComponent<Rigidbody>().angularDrag;
			target.GetComponent<Rigidbody>().drag = dragOverride;
			target.GetComponent<Rigidbody>().angularDrag = dragOverride;
			
			tractorDirection = 1;
			currentTarget = target.gameObject;
			tractorActive = true;

			DebugConsole.Log ("Tractor beam activated in Push mode");
			break;
		}
		}
	}

	private void StopTractor()
	{
		Debug.Log( "Stopping tractor beam" );
		RefreshTarget();
		if ( Network.peerType != NetworkPeerType.Disconnected )
		{
			GameNetworkManager.instance.SendTractorStopMessage( this.GetComponent<NetworkView>().viewID );
		}
	}

	public void RefreshTarget()
	{
		currentTarget.GetComponent<Rigidbody>().drag = targetOriginalDrag;
		currentTarget.GetComponent<Rigidbody>().angularDrag = targetOriginalAngularDrag;
		tractorActive = false;
	}

}
