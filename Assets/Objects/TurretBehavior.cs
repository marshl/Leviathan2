using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurretBehavior : MonoBehaviour {
	
	public GameObject target;
	public List<PunchingBag> targetList;

	public Transform joint;
	public Transform arm;
	public float rotationSpeed;
	
	Vector3 direction;
	Quaternion lookRotation;
	Quaternion newRot;
	float distance;
	float shotVelocity;
	float shotMagnitude;

	public bool chrisTurret =  false;
	public float pitchOffset = 0;
	public int teamNumber = 0;
	
	public float minimumPitchAngle = 180;

	// Use this for initialization
	void Start () {

		TargetManager manager = GameObject.FindObjectOfType (typeof(TargetManager)) as TargetManager;
		targetList = manager.RedFighters;
	
	}
	
	// Update is called once per frame
	void Update () {

		FindClosestTarget();

		TurretAim();
		this.gameObject.GetComponent<WeaponHandler>().Fire();
			
	}

	void FindClosestTarget()
	{
		float minDistance = float.PositiveInfinity;
		PunchingBag newTarget = target.GetComponent<PunchingBag>();

		foreach (PunchingBag fighter in targetList)
		{
			distance = Vector3.Magnitude (fighter.transform.position - transform.position) ;
			if(distance < minDistance)
			{
				newTarget = fighter;
				minDistance = distance;
			}
			 
		}
		target = newTarget.gameObject;
	}

	void TurretAim()
	{
		//find the vector pointing from our position to the target
		distance = Vector3.Magnitude(target.transform.position - transform.position);

		//Work out the time it will take a bullet to reach the target
		shotVelocity = this.GetComponent<WeaponHandler>().projectileSpeed;
		shotMagnitude = Vector3.Magnitude(this.transform.forward * shotVelocity) * Time.deltaTime;

		//Predict the target's location ahead based on the shot speed and current velocity
		direction = ((target.transform.position + (target.rigidbody.velocity * distance / shotMagnitude) ) - transform.position).normalized;
		
		//create the rotation we need to be in to look at the target
		lookRotation = Quaternion.LookRotation(direction);
		
		//copy the quaternion into a new variable so we can mess with it safely
		newRot = lookRotation;

		//Wrapping code
		float threshold = lookRotation.eulerAngles.x;
		
		if(threshold > 180)
		{
			threshold -= 360 ;
		}
		
		//Turret pitch clamp
		if(threshold > minimumPitchAngle)
		{
			newRot = Quaternion.Euler(minimumPitchAngle,lookRotation.eulerAngles.y,lookRotation.eulerAngles.z);
		}
		
		if(chrisTurret)
		{
			Quaternion yRot = Quaternion.Euler (new Vector3(0,newRot.eulerAngles.y,0));
			
			//Transform joint = GameObject.Find ("Light_Laser/Light_Laser_Turret/LT_Base_Joint").transform;
			//Transform arm = GameObject.Find ("Light_Laser/Light_Laser_Turret/LT_Base_Joint/LT_Arm_Joint").transform;
			joint.rotation = Quaternion.Slerp(joint.rotation, yRot, Time.deltaTime * rotationSpeed);
			
			Quaternion zRot = Quaternion.Euler (new Vector3(newRot.eulerAngles.x,joint.rotation.eulerAngles.y,joint.rotation.eulerAngles.z));
			arm.rotation = Quaternion.Slerp(arm.rotation, zRot, Time.deltaTime * rotationSpeed);
		}
		else
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * rotationSpeed);
			
		}
	}
}
