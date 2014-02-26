using UnityEngine;
using System.Collections;

public class CapitalShipNetworkInfo : MonoBehaviour {

	protected float turnAmount = 0.0f;
	protected bool isTurning = false;
	protected float currentTurnDirection = 0.0f;
	protected float currentMovementSpeed = 5.0f;
	protected float lastCommandTime;
	protected Vector3 startPosition;
	protected Quaternion startRotation;
	public CapitalShipNetworkMovement movementScript;

	public class CapitalShipNetworkPacket
	{
		public float turnAmount = 0.0f;
		public bool isTurning = false;
		public float currentTurnDirection = 0.0f;
		public float currentMovementSpeed = 5.0f;
		public float lastCommandTime;
	}

	// Use this for initialization
	void Start () {

		lastCommandTime = Time.time;
		movementScript = this.GetComponent<CapitalShipNetworkMovement>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void StateUpdated(CapitalShipNetworkPacket updatedInfo)
	{
		//This is the sender's information. It gets sent to everyone else in UpdateNetworkInfo
		turnAmount = updatedInfo.turnAmount;
		isTurning = updatedInfo.isTurning;
		currentTurnDirection = updatedInfo.currentTurnDirection;
		lastCommandTime = updatedInfo.lastCommandTime;
		startRotation = this.transform.rotation;
		startPosition = this.transform.position;

		this.networkView.RPC ("UpdateNetworkInfo",RPCMode.All);
	}

	[RPC]
	public void UpdateNetworkInfo()
	{
		print("Network RPC called");
		this.networkView.RPC ("SendNetworkInfo",RPCMode.Others,turnAmount, isTurning, currentTurnDirection, lastCommandTime
		                      ,startPosition, startRotation);
	}

	[RPC]
	public void SendNetworkInfo(float newTurnAmt, bool newTurning, float newTurnDir, float lastCmdTime,
	                            Vector3 newStartPosition, Quaternion newStartRotation)
	{
		//We are on the other computers right now. Update their stuff.
		if(!this.networkView.isMine)
		{

			if(lastCmdTime > lastCommandTime)
			{
				print("Received RPC with valid time");
				turnAmount = newTurnAmt;
				isTurning = newTurning;
				currentTurnDirection = newTurnDir;
				lastCommandTime = lastCmdTime;
				startPosition = newStartPosition;
				startRotation = newStartRotation;

				movementScript.StartNewTurnWithInfo(turnAmount,isTurning,currentTurnDirection,startPosition,startRotation);
			}
		}


	}
}
