using UnityEngine;
using System.Collections;

public class CapitalHealth : BaseHealth 
{
	public int shieldGenerators;
	public float shieldingPerGenerator; //Our base shields, the capital ship gets this * shieldGenerators final max shield
	public bool overrideShieldGenerators = false;

	public void Awake()
	{
		if ( !this.overrideShieldGenerators )
		{
			ShieldGeneratorHealth[] generators = this.GetComponentsInChildren<ShieldGeneratorHealth>();

			shieldGenerators = generators.Length;

			DebugConsole.Log ("Capital shield generator total is " + shieldGenerators.ToString());
		}

		this.RecalculateMaxShields();
	}

	public void RecalculateMaxShields()
	{
		this.maxShield = this.shieldingPerGenerator * shieldGenerators;

		if ( this.currentShield > maxShield )
		{
			DebugConsole.Warning( "Current capital ship (" + this.currentShield + ") is greater than generator limit (" + maxShield + ")", this );
			this.currentShield = maxShield;
		}
	}
}
