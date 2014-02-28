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
	/*public float sendRate = 333/1000;
	private float sendCounter = 0;*/

	public class CapitalShipNetworkPacket
	{
		public float turnAmount = 0.0f;
		public bool isTurning = false;
		public float currentTurnDirection = 0.0f;
		public float currentMovementSpeed = 5.0f;
		public float lastCommandTime;
	}

	private void Awake()
	{
		if ( Network.peerType == NetworkPeerType.Disconnected )
		{
			this.enabled = false;
		}
	}

	// Use this for initialization
	void Start () {

		lastCommandTime = Time.time;
		movementScript = this.GetComponent<CapitalShipNetworkMovement>();
	}
	
	// Update is called once per frame
	void Update () {

		/*sendCounter += Time.deltaTime;
		if(sendCounter >= sendRate)
		{
			sendCounter -= 1;

		}*/
	
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

		this.networkView.RPC ("UpdatePositionInfo",RPCMode.All);
	}
	public void SpeedUpdated(float newSpeed)
	{
		currentMovementSpeed = newSpeed;
		lastCommandTime = Time.time;
		UpdateSpeedInfo();
	}

	[RPC]
	public void UpdatePositionInfo()
	{
	//	print("Network RPC called");
		this.networkView.RPC ("SendPositionInfo",RPCMode.Others,turnAmount, isTurning, currentTurnDirection, lastCommandTime
		                      ,startPosition, startRotation);
	}

	[RPC]
	public void UpdateSpeedInfo()
	{
		print("Sending updated speed info");
		this.networkView.RPC ("SendSpeedInfo",RPCMode.Others,currentMovementSpeed, lastCommandTime);
	}

	[RPC]
	public void SendSpeedInfo(float newSpeed, float lastCmdTime)
	{
		print("Received speed information packet with " + newSpeed + " speed and command time of " + lastCmdTime + ", compared to " + lastCommandTime);
		if(!this.networkView.isMine)
		{
			if(lastCmdTime > lastCommandTime)
			{
				currentMovementSpeed = newSpeed;
				movementScript.SetSpeed (currentMovementSpeed);
			}
		}
	}

	[RPC]
	public void SendPositionInfo(float newTurnAmt, bool newTurning, float newTurnDir, float lastCmdTime,
	                            Vector3 newStartPosition, Quaternion newStartRotation)
	{
		//We are on the other computers right now. Update their stuff.
		if(!this.networkView.isMine)
		{

			if(lastCmdTime > lastCommandTime)
			{
				//print("Received RPC with valid time");
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
