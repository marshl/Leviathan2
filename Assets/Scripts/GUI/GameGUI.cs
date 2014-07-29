﻿using UnityEngine;
using System.Collections;

public class GameGUI : MonoBehaviour 
{
	private GamePlayer player;

	public enum GUI_MODE
	{
		NONE,
		FIGHTER,
		FIGHTER_RESPAWNING,
		CAPITAL,
	};

	public GUI_MODE guiMode = GUI_MODE.NONE;

	public Texture healthBarTexture;
	public Texture shieldBarTexture;
	public Texture reticuleTexture;

	private Rect capitalShipHealthRect1;
	private Rect capitalShipHealthRect2;
	private Rect capitalShipShieldRect1;
	private Rect capitalShipShieldRect2;

	public Rect targetReticuleScale;

	public float targetCameraDistance;

	private void Update () 
	{
		this.DetermineGUIMode();

		switch ( this.guiMode )
		{
		case GUI_MODE.NONE:
		{
			this.camera.enabled = false;
			return;
		}
		case GUI_MODE.FIGHTER:
		{
			this.camera.enabled = true;
			this.UpdateCapitalShipDisplay();

			TargetManager.Target target = this.player.fighter.weapons.currentTarget;
			if ( target != null )
			{
				Vector3 targetPos = target.health.transform.position;
				Vector3 fighterPos = this.player.fighter.transform.position;

				Vector3 diff = targetPos - fighterPos;
				this.transform.position = targetPos - diff.normalized * this.targetCameraDistance;
				this.transform.LookAt( targetPos );
			}
			else
			{
				this.camera.enabled = false;
			}
			break;
		}
		case GUI_MODE.FIGHTER_RESPAWNING:
		{
			this.camera.enabled = false;
			break;
		}
		case GUI_MODE.CAPITAL:
		{
			this.camera.enabled = false;
			this.UpdateCapitalShipDisplay();
			break;
		}
		default:
		{
			DebugConsole.Error( "Uncaught GUIMODE state " + this.guiMode );
			break;
		}
		}
	}

	private void DetermineGUIMode()
	{
		if ( this.player == null )
		{
			this.player = GamePlayerManager.instance.myPlayer;
		}

		if ( this.player != null )
		{
			if ( this.player.fighter != null )
			{
				this.guiMode = this.player.fighter.state == FighterMaster.FIGHTERSTATE.DEAD
					? GUI_MODE.FIGHTER_RESPAWNING : GUI_MODE.FIGHTER;
			}
			else if ( this.player.capitalShip != null )
			{
				this.guiMode = GUI_MODE.CAPITAL;
			}
			else
			{
				this.guiMode = GUI_MODE.NONE;
			}
		}
		else
		{
			this.guiMode = GUI_MODE.NONE;
		}
	}

	private void UpdateCapitalShipDisplay()
	{
		GamePlayer p1 = GamePlayerManager.instance.commander1;
		if ( p1 != null && p1.capitalShip != null )
		{
			CapitalHealth health = p1.capitalShip.health;
			float healthRatio = health.currentHealth / health.maxHealth;
			float shieldRatio = health.currentShield / health.maxShield;

			healthRatio = healthRatio > 0.0f ? healthRatio : 0.0f;
			shieldRatio = shieldRatio > 0.0f ? shieldRatio : 0.0f;

			this.capitalShipHealthRect1.Set( 5, 5, healthRatio * (Screen.width * 0.5f-10), 5 );
			this.capitalShipShieldRect1.Set( 5, 10, shieldRatio * (Screen.width * 0.5f-10), 5 );
		}

		GamePlayer p2  = GamePlayerManager.instance.commander2;
		if ( p2 != null && p2.capitalShip != null )
		{
			CapitalHealth health = p2.capitalShip.health;
			float healthRatio = health.currentHealth / health.maxHealth;
			float shieldRatio = health.currentShield / health.maxShield;

			healthRatio = healthRatio > 0.0f ? healthRatio : 0.0f;
			shieldRatio = shieldRatio > 0.0f ? shieldRatio : 0.0f;

			this.capitalShipHealthRect2.Set( Screen.width * 0.5f+5, 5, healthRatio * (Screen.width * 0.5f - 10), 5 );
			this.capitalShipShieldRect2.Set( Screen.width * 0.5f+5, 10, shieldRatio * (Screen.width * 0.5f - 10), 5 );
		}
	}
	
	private void RenderCapitalShipDisplay()
	{
		if ( GamePlayerManager.instance.commander1 != null )
		{
			this.RenderCapitalShipHealth( GamePlayerManager.instance.commander1.capitalShip );
		}
		if ( GamePlayerManager.instance.commander2 != null )
		{
			this.RenderCapitalShipHealth( GamePlayerManager.instance.commander2.capitalShip );
		}
	}

	private void RenderCapitalShipHealth( CapitalShipMaster _ship )
	{
		GUI.DrawTexture( this.capitalShipHealthRect1, this.healthBarTexture ); 
		GUI.DrawTexture( this.capitalShipHealthRect2, this.healthBarTexture ); 

		GUI.DrawTexture( this.capitalShipShieldRect1, this.shieldBarTexture ); 
		GUI.DrawTexture( this.capitalShipShieldRect2, this.shieldBarTexture ); 
	}

	private void RenderFighterTargets()
	{
		TargetManager.Target target = this.player.fighter.weapons.currentTarget;
		if ( target != null )
		//foreach ( TargetManager.Target target in this.player.fighter.weapons.otherTargets )
		{
			Vector3 toTarget = target.health.transform.position - this.player.fighter.transform.position;
			if ( Vector3.Dot( this.player.fighter.transform.forward, toTarget.normalized ) > 0.0f )
			{
				Vector3 worldPos = target.health.transform.position;

				Vector3 screenPos = Camera.main.WorldToScreenPoint( worldPos );
				Rect r = this.targetReticuleScale;
				r.x += screenPos.x;
				r.y += Screen.height - screenPos.y;

				GUI.DrawTexture( r, this.reticuleTexture );
			}
		}
	}

	private void OnGUI()
	{
		switch ( this.guiMode )
		{
		case GUI_MODE.NONE:
		{
			return;
		}
		case GUI_MODE.FIGHTER:
		{
			this.RenderFighterTargets();
			this.RenderCapitalShipDisplay();
			break;
		}
		case GUI_MODE.FIGHTER_RESPAWNING:
		{
			break;
		}
		case GUI_MODE.CAPITAL:
		{
			this.RenderCapitalShipDisplay();
			break;
		}
		default:
		{
			DebugConsole.Error( "Uncaught GUIMODE state " + this.guiMode );
			break;
		}
		}
	}
}
