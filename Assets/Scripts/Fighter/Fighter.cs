using UnityEngine;
using System.Collections;

public class Fighter : MonoBehaviour {

	public enum FIGHTERSTATE
	{
		DOCKED,
		UNDOCKING,
		FLYING,
		DEAD
	};

	public float turnSpeed = 35.0f;
	public float rollSpeed = 2.0f;
	public float acceleration = 2.0f;
	public float maxSpeed = 10.0f;
	public float desiredSpeed = 0.0f;
	public float minSpeed = -2.0f;
	public float rollDamping = 0.3f;
	public float minimumBounce = 10.0f;
	public float bounceVelocityModifier = 300.0f;
	public int team = 1;
	public FIGHTERSTATE state = FIGHTERSTATE.FLYING;
	//public float currentSpeed = 0.0f;
	//public float desiredSpeed = 0.0f;
	//public float maxSpeed = 10.0f; //Potentially used as a hard limit

	/// <summary>
	/// The distance from the centre of the screen at which the turn amout will be maximum
	/// Example: A value of 1/3 means that the the mouse outside an ellipse with extentes 1/3
	/// of the screen size will hav maximum turn rate,
	/// gradually leading down to zero in the centre of the screen
	/// </summary>
	public float turnExtents;

	//public bool docked = false;
	//public bool undocking = false;
	public float undockingTimer = 0.0f;
	public float undockingDelay = 3.0f;

	public DockingBay.DockingSlot currentSlot;

	private float screenRatio;

	private void Start()
	{
		screenRatio = (float)Screen.width / (float)Screen.height;
	}

	private void LateUpdate()
	{
		if ( Input.GetKeyDown( KeyCode.G ) )
		{
			Respawn();
		}

		switch( state )
		{
		case FIGHTERSTATE.FLYING:
			{
				this.rigidbody.AddForce (this.transform.forward * desiredSpeed * Time.deltaTime);
				if ( !GameMessages.instance.typing )
				{
					CheckFlightControls();
				}
				ApplyDrag();
			};
			break;

		case FIGHTERSTATE.DOCKED:
		{
			//	print("Docked update");
			if ( !GameMessages.instance.typing )
			{
				CheckDockedControls();
			}
			break;
		};
		case FIGHTERSTATE.UNDOCKING:
			{
				this.rigidbody.AddForce (this.transform.forward * desiredSpeed * Time.deltaTime);
				if ( !GameMessages.instance.typing )
				{
					CheckFlightControls();
				}
				ApplyDrag();
					if(undockingTimer > 0)
					{
						undockingTimer -= Time.deltaTime;
						if(undockingTimer < 0)
						{
							undockingTimer = 0;
							state = FIGHTERSTATE.FLYING;
						}
					}
			};
			break;

		case FIGHTERSTATE.DEAD:
			{
				//Do some ongui stuff here to tell people respawn time
				
				//Find an empty fighter slot on the capital ship
				
			}
			break;
		}



	}

	void CheckFlightControls()
	{
		if ( Input.GetMouseButton(0) ) // Left click - Turn the ship
		{
			Vector2 transformedMousePos = new Vector3(Input.mousePosition.x / Screen.width * 2.0f - 1.0f,
			                                          Input.mousePosition.y / Screen.height * 2.0f - 1.0f);

			transformedMousePos.x = Mathf.Clamp( transformedMousePos.x, -1.0f, 1.0f );
			transformedMousePos.y = Mathf.Clamp( transformedMousePos.y, -1.0f, 1.0f );

			//Set the desired rotation based on mouse position.
			transformedMousePos.x = Mathf.Sign( transformedMousePos.x ) * Common.GaussianCurveClamped(
				transformedMousePos.x, -1.0f, 0.0f, this.turnExtents, 1.0f );
			transformedMousePos.y = Mathf.Sign( transformedMousePos.y ) * Common.GaussianCurveClamped(
				transformedMousePos.y, -1.0f, 0.0f, this.turnExtents * this.screenRatio, 1.0f );

			Vector3 torqueValue = new Vector3(
				-transformedMousePos.y * Time.deltaTime * turnSpeed,
				transformedMousePos.x * Time.deltaTime * turnSpeed,
				0.0f );

			this.rigidbody.AddRelativeTorque( torqueValue );
		}



		if(Input.GetKey (KeyCode.W)) // Accelerate
		{
			desiredSpeed += acceleration * Time.deltaTime;
			if(desiredSpeed > maxSpeed)
			{
				desiredSpeed = maxSpeed;
			}
		}
		if(Input.GetKey (KeyCode.S)) // Deccelerate
		{
			//this.rigidbody.AddForce (-this.transform.forward * acceleration * Time.deltaTime);
			desiredSpeed -= acceleration * Time.deltaTime;
			if(desiredSpeed < minSpeed)
			{
				desiredSpeed = minSpeed;
			}
		}

		if(Input.GetKey (KeyCode.Q)) // Rotate left
		{
			this.rigidbody.AddRelativeTorque (new Vector3(0,0,rollSpeed));
		}
		if(Input.GetKey (KeyCode.E)) // Rotate right
		{
			this.rigidbody.AddRelativeTorque (new Vector3(0,0,-rollSpeed));
		}

		if(Input.GetKey (KeyCode.Backspace)) // Stop
		{
			desiredSpeed = 0.0f;
		}
		if(Input.GetKey (KeyCode.Tab)) // Thrusters (not a special)
		{
			desiredSpeed += (acceleration * 2 * Time.deltaTime);
		}
	}

	void CheckDockedControls()
	{
		if(Input.GetKey (KeyCode.Space))
		{
			print("Space was pressed");
			Undock();
		}
	}

	void ApplyDrag()
	{

		//I pray this works. Obtain local angular velocity and reduce a single axis of it.

		Vector3 angularVeloc = transform.InverseTransformDirection(rigidbody.angularVelocity);

		angularVeloc.z *= rollDamping;

		this.rigidbody.AddRelativeTorque (angularVeloc - transform.InverseTransformDirection(rigidbody.angularVelocity));
	}

	void OnCollisionEnter(Collision collision)
	{
		/*Vector3 averagePoint = Vector3.zero;
		int counter = 0;

		foreach (ContactPoint contact in collision.contacts)
		{
			averagePoint += contact.normal;
			counter++;
		}

		averagePoint /= counter;*/

		Vector3 bounceForce = (collision.contacts[0].normal *
		                       this.rigidbody.velocity.magnitude * bounceVelocityModifier)
			+ (minimumBounce * collision.contacts[0].normal.normalized);

		if(collision.rigidbody != null)
		{
			bounceForce *= (collision.rigidbody.velocity.magnitude * bounceVelocityModifier);
		}

		//print(bounceForce);


		this.rigidbody.AddForce (bounceForce);
		this.rigidbody.AddTorque (Vector3.Cross(new Vector3(bounceForce.z, bounceForce.y, bounceForce.x), this.transform.rotation.eulerAngles));
		//this.rigidbody.AddForce (averagePoint * this.rigidbody.velocity.magnitude * bounciness); //bounciness);
	}

	public void Dock(DockingBay.DockingSlot slot)
	{
		if(state == FIGHTERSTATE.UNDOCKING )
		{
			print("Skipping dock");
			return;
		}

		print("Proceeding with dock");

		desiredSpeed = 0;
		rigidbody.velocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;
		rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		transform.position = slot.landedPosition.position;
		transform.rotation = slot.landedPosition.rotation;
		currentSlot = slot;
		state = FIGHTERSTATE.DOCKED;
		transform.parent = slot.landedPosition;
		//networkView.RPC ("SendDockedInfo",RPCMode.Others,slot);
		GameNetworkManager.instance.SendDockedMessage ( this.networkView.viewID, slot.slotID );
		this.GetComponent<FighterWeapons>().enabled = false;
	}

	public void Undock()
	{
		print("Undocking");
		rigidbody.constraints = RigidbodyConstraints.None;

		//Vector3 inheritedVelocity = this.transform.root.forward;
		//Vector3 inheritedAngularVelocity = this.transform.root.FindChild ("CapitalCollider").rigidbody.angularVelocity;

		Vector3 inheritedVelocity = this.transform.root.GetComponent<NetworkPositionControl>().CalculateVelocity ();
		//Vector3 inheritedAngularVelocity = this.transform.root.FindChild ("CapitalCollider").rigidbody.angularVelocity;

		//print("Inherited velocity: " + inheritedVelocity);

		state = FIGHTERSTATE.UNDOCKING;
	    currentSlot.occupied = false;
	    currentSlot.landedFighter = null;
	    desiredSpeed = (maxSpeed * 0.75f);
	    undockingTimer = undockingDelay;

	    this.transform.parent = null;
		this.transform.localScale = new Vector3(1.0f,1.0f,1.0f);

		//Apply force of the capital ship so we don't move relative
		GameNetworkManager.instance.SendUndockedMessage ( this.networkView.viewID, currentSlot.slotID );
		currentSlot = null;
		this.GetComponent<FighterWeapons>().enabled = true;

		//this.rigidbody.AddRelativeForce (inheritedVelocity * 5);
		//this.rigidbody.AddRelativeTorque (inheritedAngularVelocity);

		Debug.Log( "Inherited Velocity: " + inheritedVelocity, this );
		
		this.rigidbody.AddForce( inheritedVelocity * 95 );
	}

	public void Respawn()
	{
		//Find an empty spot on the friendly capital ship and dock there
		//state = FIGHTERSTATE.DOCKED;

		DockingBay[] bays = FindObjectsOfType(typeof(DockingBay)) as DockingBay[];
		
		bool firstPass = true;
		int startingIndex = 0;
		for(int ii = (int)Random.Range(0, bays.Length); true; ii++) //This is an infinite loop
		{
			if(firstPass)
			{
				startingIndex = ii;
				firstPass = false;
			}
			else
			{
				if(ii == startingIndex)
				{
					Debug.Log("Could not find an empty docking bay (somehow)");
					break;
				}
			}
			if(ii == bays.Length)
			{
				ii = 0;
			}

			if(bays[ii].team == this.team)
			{
				//todo: fill me up
				this.Dock (bays[ii].GetFreeSlot ());
				break;
			}


		}
				
	}
			

}
