using UnityEngine;
using System.Collections;

public class TractorBeam : MonoBehaviour {


	public enum TractorFunction
	{
		PUSH,
		PULL,
		HOLD
	};

	public CapitalShipMaster master;
	public float chargePercentage = 100.0f;
	public float chargeDrain = 15.0f;
	public float chargeRegeneration = 10.0f;
	//public float maximumRange = 100000.0f;
	public float strengthMultiplier = 1.0f;
	public float dragOverride = 300;

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
		if ( this.master.dummyShip == false )
			#endif
		{
			if (this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected)
			     
			{
				if(tractorUITargetting && Input.GetMouseButtonDown (1))
				{
					RaycastHit hit;
					Ray toCast = Camera.main.ScreenPointToRay(Input.mousePosition);


					if( Physics.Raycast (toCast, out hit))
					{
						if(tractorActive)
						{

							StopTractor();
						}

						FireAtTarget(hit.collider.gameObject, tractorUITargetType);
						GameNetworkManager.instance.SendTractorStartMessage (this.networkView.viewID, tractorUITargetType, hit.collider.networkView.viewID);
					}
				}
			}
		}
	}

	private void FixedUpdate()
	{
		if(tractorActive)
		{
			Vector3 forceVector = new Vector3(currentTarget.transform.position.x - this.transform.position.x,
			                                  currentTarget.transform.position.y - this.transform.position.y,
			                                  currentTarget.transform.position.z - this.transform.position.z);
			//Vector3 forceVector = new Vector3(1000000000,0,0);

			forceVector.Normalize ();
			forceVector *= tractorDirection * strengthMultiplier * dragOverride;

			DebugConsole.Log ("Force vector is " + forceVector.ToString());
			DebugConsole.Log ("Magnitude is " + forceVector.magnitude.ToString());

			currentTarget.rigidbody.AddForce (forceVector);

			DebugConsole.Log ("Target name is " + currentTarget.name);
			chargePercentage -= chargeDrain * Time.fixedDeltaTime;

			if(chargePercentage <= 0)
			{
				chargePercentage = 0;
				if(this.networkView.isMine)
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

	void OnGUI()
	{
		if(this.networkView.isMine || Network.peerType == NetworkPeerType.Disconnected)
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
	

	public void FireAtTarget(GameObject target, TractorFunction type)
	{
		switch(type)
		{
		case TractorFunction.HOLD:
			{
				targetOriginalDrag = target.rigidbody.drag;
				targetOriginalAngularDrag = target.rigidbody.angularDrag;
				target.rigidbody.drag = dragOverride;
				target.rigidbody.angularDrag = dragOverride;

				tractorDirection = 0;
				currentTarget = target.gameObject;
				tractorActive = true;

			DebugConsole.Log ("Tractor beam activated in Hold mode");
				
			}
			break;
		case TractorFunction.PULL:
		{
			targetOriginalDrag = target.rigidbody.drag;
			targetOriginalAngularDrag = target.rigidbody.angularDrag;
			target.rigidbody.drag = dragOverride;
			target.rigidbody.angularDrag = dragOverride;
			
			tractorDirection = -1;
			currentTarget = target.gameObject;
			tractorActive = true;

			DebugConsole.Log ("Tractor beam activated in Pull mode");

		}
			break;
		case TractorFunction.PUSH:
		{
			targetOriginalDrag = target.rigidbody.drag;
			targetOriginalAngularDrag = target.rigidbody.angularDrag;
			target.rigidbody.drag = dragOverride;
			target.rigidbody.angularDrag = dragOverride;
			
			tractorDirection = 1;
			currentTarget = target.gameObject;
			tractorActive = true;

			DebugConsole.Log ("Tractor beam activated in Push mode");

		}
			break;
		}
	}

	private void StopTractor()
	{
		RefreshTarget();
		GameNetworkManager.instance.SendTractorStopMessage (this.networkView.viewID);
	}

	public void RefreshTarget()
	{
		currentTarget.rigidbody.drag = targetOriginalDrag;
		currentTarget.rigidbody.angularDrag = targetOriginalAngularDrag;
		tractorActive = false;
	}

}
