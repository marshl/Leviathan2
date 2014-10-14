using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameGUI : MonoBehaviour 
{
	public static GameGUI instance;

	private GamePlayer player;

	public enum GUI_MODE
	{
		NONE,
		PRE_GAME,
		FIGHTER,
		FIGHTER_RESPAWNING,
		CAPITAL,
		FIGHTER_PICKER,
		POST_GAME,
	};

	public GUI_MODE guiMode = GUI_MODE.NONE;

	public Texture healthBarTexture;
	public Texture shieldBarTexture;
	public Texture reticuleTexture;
	public Texture friendlyReticuleTexture;
	public Texture enemyReticuleTexture;

	public Texture barBracketTexture;

	public float CSHealthBarThickness;
	public float CSHealthBarOffset;
	public float CSHealthBarRelativeLength;
	public float csHealthBarWidth;
	public float healthBarTileCount;
	public float healthShieldBarGap;

	private float team1CSDamageTimer;
	private float team2CSDamageTimer;

	public float csHealthDamageHighlightDuration;

	public float targetReticuleScale;

	public float targetCameraDistance;

	public Camera targetCamera;
	public Camera idleCamera;

	public Rect tiling;

	private void Awake()
	{
		instance = this;
	}

	private void Update() 
	{
		this.DetermineGUIMode();

		this.team1CSDamageTimer += Time.deltaTime;
		this.team2CSDamageTimer += Time.deltaTime;

		switch ( this.guiMode )
		{
		case GUI_MODE.NONE:
		{
			this.targetCamera.enabled = false;
			this.idleCamera.enabled = true;
			break;
		}
		case GUI_MODE.PRE_GAME:
		{
			this.targetCamera.enabled = false;
			this.idleCamera.enabled = true;
			break;
		}
		case GUI_MODE.POST_GAME:
		{
			this.targetCamera.enabled = false;
			this.idleCamera.enabled = true;
			break;
		}
		case GUI_MODE.FIGHTER:
		{
			this.idleCamera.enabled = false;
			this.camera.enabled = true;

			BaseHealth target = this.player.fighter.weapons.currentTarget;
			if ( target != null )
			{
				Vector3 targetPos = target.transform.position;
				Vector3 fighterPos = this.player.fighter.transform.position;

				Vector3 diff = targetPos - fighterPos;
				this.transform.position = targetPos - diff.normalized * this.targetCameraDistance;
				this.transform.LookAt( targetPos );
			}
			else
			{
				this.targetCamera.enabled = false;
			}
			break;
		}
		case GUI_MODE.FIGHTER_RESPAWNING:
		{
			this.targetCamera.enabled = false;
			break;
		}
		case GUI_MODE.CAPITAL:
		{
			this.idleCamera.enabled = false;
			this.targetCamera.enabled = false;
			break;
		}
		case GUI_MODE.FIGHTER_PICKER:
		{
			this.targetCamera.enabled = false;
			this.idleCamera.enabled = true;
			break;
		}
		default:
		{
			DebugConsole.Error( "Uncaught GUIMODE state " + this.guiMode );
			break;
		}
		}
	}

	private void OnGUI()
	{
		switch ( this.guiMode )
		{
		case GUI_MODE.NONE:
		{
			break;
		}
		case GUI_MODE.PRE_GAME:
		{
			break;
		}
		case GUI_MODE.POST_GAME:
		{
			if ( GUI.Button( new Rect( Screen.width/3, Screen.height/3, Screen.width/3, Screen.height/3 ), "QUIT" ) )
			{
				GameNetworkManager.instance.QuitGame();
			}
			break;
		}
		case GUI_MODE.FIGHTER:
		{
			this.RenderFighterHealth();
			this.RenderFighterTargets();
			this.RenderCapitalHealthStates();
			this.RenderFighterSpeed();
			this.RenderFighterWeapons();
			break;
		}
		case GUI_MODE.FIGHTER_RESPAWNING:
		{
			float respawnTimer = this.player.fighter.respawnTimer;
			if ( respawnTimer > 0 )
			{
				GUI.Label (new Rect((Screen.width / 2) - 200, Screen.height / 2, 300, 50), "Respawn available in " + respawnTimer + " seconds");
			}
			else
			{
				GUI.Label (new Rect((Screen.width / 2) - 200, Screen.height / 2, 300, 50), "Press Space to respawn");
			}
			break;
		}
		case GUI_MODE.CAPITAL:
		{
			this.RenderCapitalShipTargets();
			this.RenderCapitalHealthStates();
			break;
		}
		case GUI_MODE.FIGHTER_PICKER:
		{
			if ( GUI.Button( new Rect( 0, Screen.height/3, Screen.width/3, Screen.height/3 ), "SPEED" ) )
			{
				GameNetworkManager.instance.OnFighterTypeSelected( FIGHTER_TYPE.SPEED );
			}
			else if ( GUI.Button( new Rect( Screen.width/3, Screen.height/3, Screen.width/3, Screen.height/3 ), "AGILITY" ) )
			{
				GameNetworkManager.instance.OnFighterTypeSelected( FIGHTER_TYPE.AGILE );
			}
			else if ( GUI.Button( new Rect( Screen.width*2/3, Screen.height/3, Screen.width/3, Screen.height/3 ), "HEAVY" ) )
			{
				GameNetworkManager.instance.OnFighterTypeSelected( FIGHTER_TYPE.HEAVY );
			}

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

		if ( GameNetworkManager.instance.gameState == GameNetworkManager.GAME_STATE.PRE_GAME )
		{
			this.guiMode = GUI_MODE.PRE_GAME;
			return;
		}
		else if ( GameNetworkManager.instance.gameState == GameNetworkManager.GAME_STATE.POST_GAME )
		{
			this.guiMode = GUI_MODE.POST_GAME;
			return;
		}

		if ( this.player == null )
		{
			this.guiMode = GUI_MODE.NONE;
			return;
		}

		if ( this.player.fighter != null )
		{
			this.guiMode = ( this.player.fighter.state == FighterMaster.FIGHTERSTATE.DEAD
				|| this.player.fighter.state == FighterMaster.FIGHTERSTATE.OUT_OF_CONTROL )
				? GUI_MODE.FIGHTER_RESPAWNING : GUI_MODE.FIGHTER;
		}
		else if ( this.player.capitalShip != null )
		{
			this.guiMode = GUI_MODE.CAPITAL;
		}
		else if ( (this.player.playerType == PLAYER_TYPE.FIGHTER1 || this.player.playerType == PLAYER_TYPE.FIGHTER2 )
		         && this.player.fighterType == FIGHTER_TYPE.NONE )
		{
			this.guiMode = GUI_MODE.FIGHTER_PICKER;
		}
		else
		{
			this.guiMode = GUI_MODE.NONE;
		}
	}

	private void RenderCapitalShipTargets()
	{
		List<BaseHealth> targets = TargetManager.instance.GetTargetsOfType( TARGET_TYPE.FIGHTER );
		
		foreach ( BaseHealth t in targets )
		{
			if ( t.Owner == null )
			{
				continue;
			}

			Vector3 toTarget = t.transform.position - this.player.capitalShip.capitalCamera.transform.position;
			if ( Vector3.Dot( this.player.capitalShip.capitalCamera.transform.forward, toTarget.normalized ) < 0.0f )
			{
				continue;
			}

			Vector3 worldPos = t.transform.position;
			
			Vector3 screenPos = Camera.main.WorldToScreenPoint( worldPos );
			screenPos.y = Screen.height - screenPos.y;
			
			//Rect r = this.GetHealthGUIRect( t );
			Rect r = new Rect(0,0,0,0);
			r.width = 50;
			r.height = 50;
			r.center = screenPos;

			if ( t.Owner.team == TEAM.NEUTRAL )
			{
				GUI.DrawTexture( r, this.reticuleTexture );
			}
			else if ( t.Owner.team == this.player.team )
			{
				GUI.DrawTexture( r, this.friendlyReticuleTexture );
			}
			else
			{
				GUI.DrawTexture( r, this.enemyReticuleTexture );
			}
		}
	}

	private void RenderCapitalHealthStates()
	{
		float barHeight = (float)Screen.height * this.CSHealthBarRelativeLength;
		float barBase = (float)Screen.height/2 - barHeight/2;

		GamePlayer p1 = GamePlayerManager.instance.commander1;
		if ( p1 != null && p1.capitalShip != null )
		{
			CapitalHealth health = p1.capitalShip.health;
			float healthRatio = health.currentHealth / health.maxHealth;
			float shieldRatio = health.currentShield / health.maxShield;
			
			healthRatio = healthRatio > 0.0f ? healthRatio : 0.0f;
			shieldRatio = shieldRatio > 0.0f ? shieldRatio : 0.0f;

			float alpha = this.team1CSDamageTimer > this.csHealthDamageHighlightDuration ? 0.5f : 1.0f;

			GUI.color = new Color( 1.0f, 1.0f, 1.0f, alpha );

			Rect rect = new Rect( this.CSHealthBarOffset, barBase, this.csHealthBarWidth, barHeight );
			this.RenderBracketedBar( this.healthBarTexture, rect, healthRatio );

			rect.x += this.csHealthBarWidth + this.healthShieldBarGap;
			this.RenderBracketedBar( this.shieldBarTexture, rect, shieldRatio );
		}
		
		GamePlayer p2  = GamePlayerManager.instance.commander2;
		if ( p2 != null && p2.capitalShip != null )
		{
			CapitalHealth health = p2.capitalShip.health;
			float healthRatio = health.currentHealth / health.maxHealth;
			float shieldRatio = health.currentShield / health.maxShield;
			
			healthRatio = healthRatio > 0.0f ? healthRatio : 0.0f;
			shieldRatio = shieldRatio > 0.0f ? shieldRatio : 0.0f;

			float alpha = this.team2CSDamageTimer > this.csHealthDamageHighlightDuration ? 0.5f : 1.0f;
			
			GUI.color = new Color( 1.0f, 1.0f, 1.0f, alpha );

			Rect rect = new Rect( Screen.width - this.CSHealthBarOffset - csHealthBarWidth, barBase, this.csHealthBarWidth, barHeight );
			this.RenderBracketedBar( this.healthBarTexture, rect, healthRatio);
			
			rect.x -= (this.csHealthBarWidth + this.healthShieldBarGap);
			this.RenderBracketedBar( this.shieldBarTexture, rect, shieldRatio );
		}

		GUI.color = Color.white;
	}

	private void RenderFighterHealth()
	{
		FighterHealth health = this.player.fighter.health;
		GUI.Label( new Rect(0, 50, 150, 50), "Shields: " + health.currentShield + " / " + health.maxShield );
		GUI.Label( new Rect(0, 0, 150, 50), "Hull: " + health.currentHealth + " / " + health.maxHealth );
	}

	private void RenderFighterSpeed()
	{
		FighterMovement movement = this.player.fighter.movement;
		GUI.Label( new Rect( 0, 100, 150, 50 ), "Speed: " + movement.desiredSpeed + " / " + movement.maxSpeed );
	}

	private void RenderFighterTargets()
	{
		BaseHealth target = this.player.fighter.weapons.currentTarget;
		if ( target != null )
		{
			Vector3 toTarget = target.transform.position - this.player.fighter.transform.position;
			if ( Vector3.Dot( this.player.fighter.transform.forward, toTarget.normalized ) > 0.0f )
			{
				Rect r = this.GetHealthGUIRect( target );

				GUI.DrawTexture( r, this.reticuleTexture );
			
				WeaponBase missileWeapon = this.player.fighter.weapons.missileWeapon;
				float lockScale = 2.0f - Mathf.Clamp01( missileWeapon.currentLockOn / missileWeapon.weaponDesc.lockOnDuration );
				r.size = Vector2.one * this.targetReticuleScale * lockScale;

				GUI.DrawTexture( r, this.reticuleTexture );
			}
		}

		List<BaseHealth> targets = TargetManager.instance.GetTargets( this.player.fighter.weapons );

		foreach ( BaseHealth t in targets )
		{
			Vector3 toTarget = t.transform.position - this.player.fighter.transform.position;
			if ( Vector3.Dot( this.player.fighter.transform.forward, toTarget.normalized ) > 0.0f )
			{
				Vector3 worldPos = t.transform.position;
				
				Vector3 screenPos = Camera.main.WorldToScreenPoint( worldPos );
				screenPos.y = Screen.height - screenPos.y;

				Rect r = this.GetHealthGUIRect( t );
				GUI.DrawTexture( r, this.reticuleTexture );
			}
		}
	}

	private void RenderFighterWeapons()
	{
		FighterWeapons weaponScript = this.player.fighter.weapons;
		GUI.Label( new Rect( 0, 200, 150, 50 ), "Weapon: " + weaponScript.laserWeapon.weaponDesc.label );
	}

	public Rect GetHealthGUIRect( BaseHealth _health )
	{
		Rect r = new Rect();

		if ( _health.guiExtents.Length == 0 )
		{
			Vector3 pos = Camera.main.WorldToScreenPoint( _health.transform.position );
			pos.y = Screen.height - pos.y;
			r.x = pos.x; r.y = pos.y;
			r.height = r.width = this.targetReticuleScale;
		}
		else
		{
			r.xMin = r.yMin = float.MaxValue;
			r.xMax = r.yMax = float.MinValue;

			foreach ( Transform extent in _health.guiExtents )
			{
				Vector2 pos = Camera.main.WorldToScreenPoint( extent.position );
				pos.y = Screen.height - pos.y;

				r.xMin = Mathf.Min( r.xMin, pos.x );
				r.xMax = Mathf.Max( r.xMax, pos.x );

				r.yMin = Mathf.Min( r.yMin, pos.y );
				r.yMax = Mathf.Max( r.yMax, pos.y );
			}
		}
		return r;
	}

	private void RenderBracketedBar( Texture _tiledTexture, Rect _rect, float _ratio )
	{
		//GUI.color = new Color( 1.0f, 1.0f, 1.0f, 0.5f );
		Rect innerRect = new Rect( _rect );
		innerRect.height *= _ratio;
		//Debug.Log( innerRect.y );
		innerRect.y = _rect.y + (_rect.height - innerRect.height);
		GUI.DrawTexture( innerRect, _tiledTexture );
		//Debug.Log( innerRect.y );

		GUI.DrawTextureWithTexCoords( _rect, this.barBracketTexture, new Rect( 1.0f, 0.0f, 1.0f,  this.healthBarTileCount ) );
	}

	public void OnCapitalShipDamage( CapitalHealth _health, float _damage )
	{
		TEAM team = _health.Owner.team;

		if ( team == TEAM.TEAM_1 )
		{
			this.team1CSDamageTimer = 0.0f;
		}
		else if ( team == TEAM.TEAM_2 )
		{
			this.team2CSDamageTimer = 0.0f;
		}
		else
		{
			Debug.LogError( "Unknown team " + team );
		}
	}
}
