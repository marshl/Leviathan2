using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurretBehavior : BaseWeaponManager
{
	public BaseHealth target;
	
	public Transform joint;
	public Transform arm;
	public float rotationSpeed;
	
	private Vector3 direction;
	private Quaternion lookRotation;
	private Quaternion newRot;
	private float shotVelocity;

	public bool chrisTurret = false;
	public float pitchOffset = 0;
	
	public float minimumPitchAngle = 180;

	public WeaponBase weapon; 

	private void Update()
	{
		this.target = TargetManager.instance.GetBestTarget( this.arm, -1, -1, 1 );

		if ( this.target != null )
		{
			this.TurretAim();
			this.gameObject.GetComponent<WeaponBase>().SendFireMessage();
		}
	}

	/*void FindClosestTarget()
	{
		float minDistance = float.PositiveInfinity;
		BaseHealth newTarget = target.GetComponent<BaseHealth>();

		foreach (FighterHealth fighter in targetList)
		{
			distance = Vector3.Magnitude (fighter.transform.position - transform.position) ;
			if(distance < minDistance)
			{
				newTarget = fighter;
				minDistance = distance;
			}
		}
		target = newTarget.gameObject;
	}*/

	void TurretAim()
	{
		float speed = BulletDescriptorManager.instance.GetDescOfType( this.weapon.weaponDesc.bulletType ).moveSpeed;

		//Predict the target's location ahead based on the shot speed and current velocity
		direction = (Common.GetTargetLeadPosition( this.transform.position, this.target.transform, speed ) - transform.position).normalized;

		//create the rotation we need to be in to look at the target
		lookRotation = Quaternion.LookRotation(direction);
		
		//copy the quaternion into a new variable so we can mess with it safely
		newRot = lookRotation;

		//Wrapping code
		float threshold = lookRotation.eulerAngles.x;
		
		if ( threshold > 180 )
		{
			threshold -= 360 ;
		}
		
		//Turret pitch clamp
		if ( threshold > minimumPitchAngle )
		{
			newRot = Quaternion.Euler(minimumPitchAngle,lookRotation.eulerAngles.y,lookRotation.eulerAngles.z);
		}
		
		if ( chrisTurret )
		{
			Quaternion yRot = Quaternion.Euler (new Vector3(0,newRot.eulerAngles.y,0));
			
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
