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
		FIGHTER_DOCKED,
		CAPITAL,
		FIGHTER_PICKER,
		POST_GAME,
	};

	public GUI_MODE guiMode = GUI_MODE.NONE;

	public Camera targetCamera;
	public Camera idleCamera;

	public Texture2D healthBarTexture;
	public Texture2D shieldBarTexture;
	public Texture2D reticuleTexture;
	public Texture2D friendlyReticuleTexture;
	public Texture2D enemyReticuleTexture;
	public Texture2D verticalBarOverlayTexture;
	public Texture2D horizontalBarOverlayTexture;

	public Texture2D centreReticuleTexture;
	public Texture2D floatingReticuleTexture;

	public Texture2D topLeftBracket;
	public Texture2D topRightBracket;
	public Texture2D bottomRightBracket;
	public Texture2D bottomLeftBracket;

	public Texture2D lowEnergyWarningTexture;
	public Texture2D missileLockWarningTexture;

	public Texture2D team1ScoreTexture;
	public Texture2D team2ScoreTexture;

	public Texture2D lockOnBarTexture;

	public float CSHealthBarThickness;
	public float CSHealthBarOffset;
	public float CSHealthBarRelativeLength;
	public float csHealthBarWidth;
	public float healthBarTileCount;
	public float healthShieldBarGap;

	public float csHealthDamageHighlightDuration;

	public float fighterHealthBarWidth;
	public float fighterHealthBarOffset;
	public float fighterHealthBarLength;

	public float energyLowThreshold;
	public float lowEnergyWarningSize;
	public float missileLockWarningSize;

	public float targetReticuleScale;

	public float targetCameraDistance;

	public float centreReticuleScaleRelative;
	public float floatingReticuleScaleRelative;

	public float targetBracketScale;
	public float minimumBracketScale;

	public float scoreBarHeight;
	public float scoreBarWidth;

	public float targetScreenHeightRelative;
	public float targetScreenTopGap;
	public float targetScreenSideGap;
	public float targetScreenAspectRatio;
	public float targetHealthBarWidth;


	private void Awake()
	{
		instance = this;
		//Cursor.SetCursor( this.floatingReticuleTexture, Vector2.zero, CursorMode.Auto );
	}

	private void Update() 
	{
		this.DetermineGUIMode();
	
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
		case GUI_MODE.FIGHTER_DOCKED:
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

		if ( this.targetCamera.enabled )
		{
			Rect targetRect = new Rect();

			targetRect.height = Screen.height * this.targetScreenHeightRelative;
			targetRect.width = targetRect.height * this.targetScreenAspectRatio;

			targetRect.x = this.targetScreenSideGap;
			targetRect.y = Screen.height - ( this.targetScreenTopGap + targetRect.height );

			this.targetCamera.pixelRect = targetRect;
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
			this.RenderScoreBar();
			if ( GUI.Button( new Rect( Screen.width/3, Screen.height/3, Screen.width/3, Screen.height/3 ), "QUIT" ) )
			{
				GameNetworkManager.instance.QuitGame();
			}
			break;
		}
		case GUI_MODE.FIGHTER:
		{
			this.RenderFighterHealth();
			this.RenderCapitalHealthStates();
			this.RenderFighterSpeed();
			this.RenderFighterWeapons();
			this.RenderFighterEnergy();
			this.RenderFighterMissileLock();
			this.RenderScoreBar();
			this.RenderFighterTargets();

			float size = Screen.height * this.centreReticuleScaleRelative;
			GUI.DrawTexture( new Rect( Screen.width/2 - size/2, Screen.height/2 - size/2, size, size ), this.centreReticuleTexture );

			size = Screen.height * this.floatingReticuleScaleRelative;
			GUI.DrawTexture( new Rect( Input.mousePosition.x - size/2, Screen.height - Input.mousePosition.y - size/2, size, size ), this.floatingReticuleTexture );
			break;
		}
		case GUI_MODE.FIGHTER_RESPAWNING:
		{
			this.RenderScoreBar();
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
		case GUI_MODE.FIGHTER_DOCKED:
		{
			this.RenderUpgradeMenu ();
			break;
		}
		case GUI_MODE.CAPITAL:
		{
			this.RenderScoreBar();
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

		if ( this.player.fighter != null)
		{
			switch(this.player.fighter.state)
			{
			case FighterMaster.FIGHTERSTATE.OUT_OF_CONTROL:
				this.guiMode = GUI_MODE.FIGHTER_RESPAWNING;
				break;
			case FighterMaster.FIGHTERSTATE.DEAD:
				this.guiMode = GUI_MODE.FIGHTER_RESPAWNING;
					break;
			case FighterMaster.FIGHTERSTATE.FLYING:
				this.guiMode = GUI_MODE.FIGHTER;
				break;
			case FighterMaster.FIGHTERSTATE.DOCKED:
				this.guiMode = GUI_MODE.FIGHTER_DOCKED;
				break;
			default:
				this.guiMode = GUI_MODE.FIGHTER;
				break;
			}
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
		TEAM playerTeam = GamePlayerManager.instance.myPlayer.team;
		TEAM enemyTeam = Common.OpposingTeam( playerTeam );

		this.RenderCapitalShipHealth( playerTeam, false );
		this.RenderCapitalShipHealth( enemyTeam, true );

		GUI.color = Color.white;
	}

	private void RenderCapitalShipHealth( TEAM _team, bool _left )
	{
		GamePlayer capitalPlayer = GamePlayerManager.instance.GetCommander( _team );
		if ( capitalPlayer != null && capitalPlayer.capitalShip != null )
		{
			float barHeight = Screen.height * this.CSHealthBarRelativeLength;
			float barBase = Screen.height/2 - barHeight/2;

			CapitalHealth health = capitalPlayer.capitalShip.health;
			float healthRatio = health.currentHealth / health.maxHealth;
			float shieldRatio = health.currentShield / health.maxShield;
			
			healthRatio = healthRatio > 0.0f ? healthRatio : 0.0f;
			shieldRatio = shieldRatio > 0.0f ? shieldRatio : 0.0f;
			
			float alpha = health.timeSinceDealtDamage > this.csHealthDamageHighlightDuration ? 0.5f : 1.0f;
			
			GUI.color = new Color( 1.0f, 1.0f, 1.0f, alpha );
			
			Rect rect = new Rect( _left ? this.CSHealthBarOffset : Screen.width - this.CSHealthBarOffset - csHealthBarWidth,
			                     barBase, this.csHealthBarWidth, barHeight );

			this.RenderVerticalBarOverlaid( this.healthBarTexture, rect, healthRatio );
			
			rect.x += (_left ? 1.0f : -1.0f) * (this.csHealthBarWidth + this.healthShieldBarGap);
			this.RenderVerticalBarOverlaid( this.shieldBarTexture, rect, shieldRatio );
		}
	}

	private void RenderFighterHealth()
	{
		FighterHealth health = this.player.fighter.health;

		float healthRatio = health.currentHealth / health.maxHealth;
		float shieldRatio = health.currentShield / health.maxShield;

		Rect healthRect = new Rect();
		healthRect.width = this.fighterHealthBarWidth;
		healthRect.height = this.fighterHealthBarLength * Screen.height;
		healthRect.x = Screen.width/2 - this.fighterHealthBarOffset * Screen.width - healthRect.width;
		healthRect.y = Screen.height/2 - healthRect.height/2;
		
		this.RenderVerticalBarOverlaid( this.healthBarTexture, healthRect, healthRatio );

		healthRect.x = Screen.width/2 + this.fighterHealthBarOffset * Screen.width;

		this.RenderVerticalBarOverlaid( this.shieldBarTexture, healthRect, shieldRatio );
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
			Vector3 screenPos = Camera.main.WorldToScreenPoint( target.transform.position );

			bool offScreen = screenPos.x < 0.0f || screenPos.y < 0.0f
				          || screenPos.x > Screen.width || screenPos.y > Screen.height
				          || screenPos.z < 0.0f;


			if ( offScreen )
			{
				Vector2 n = Common.ClampOffscreenTarget( screenPos );

				GUI.DrawTexture( new Rect( n.x-16, Screen.height - n.y-16, 32, 32 ), this.floatingReticuleTexture );
			}
			else
			{
				Rect r = this.GetHealthGUIRect( target );

				GUI.color = Color.red;
				this.DrawTargetBracket( r );
				GUI.color = Color.white;
			}

			float distanceToTarget = (int)( this.player.fighter.transform.position - target.transform.position ).magnitude;
			string label = Common.DistanceToString( distanceToTarget );

			Rect distanceRect = new Rect();
			distanceRect.x = Screen.width/2 + this.fighterHealthBarOffset * Screen.width + this.fighterHealthBarWidth;
			distanceRect.y = Screen.height/2;
			distanceRect.width = 150.0f;
			distanceRect.height = 50.0f;
			GUI.Label( distanceRect, label );


			float targetScreenHeight = this.targetScreenHeightRelative * Screen.height;
			float targetScreenWidth = targetScreenHeight * this.targetScreenAspectRatio;

			Rect targetHealthRect = new Rect();
			targetHealthRect.width = this.targetHealthBarWidth;
			targetHealthRect.height = targetScreenHeight;
			targetHealthRect.x = this.targetScreenSideGap + targetScreenWidth;
			targetHealthRect.y = this.targetScreenTopGap;

			this.RenderVerticalBarOverlaid( this.healthBarTexture, targetHealthRect, target.currentHealth / target.maxHealth );

			targetHealthRect.x += this.targetHealthBarWidth;
			
			this.RenderVerticalBarOverlaid( this.shieldBarTexture, targetHealthRect, target.currentShield / target.maxShield );
		
			Rect lockOnRect = new Rect();
			lockOnRect.width = targetScreenWidth;
			lockOnRect.height = this.targetHealthBarWidth;
			lockOnRect.x = this.targetScreenSideGap;
			lockOnRect.y = targetScreenHeight;

			WeaponBase seekingWeapon = this.player.fighter.weapons.tertiaryWeapon;
			float lockRatio = seekingWeapon.currentLockOn / seekingWeapon.weaponDesc.lockOnDuration;
			lockRatio = Mathf.Min( lockRatio, 1.0f );

			this.RenderHorizontalBarOverlaid( this.lockOnBarTexture, lockOnRect, lockRatio );

		}

		List<BaseHealth> targets = TargetManager.instance.GetTargets( this.player.fighter.weapons );

		foreach ( BaseHealth t in targets )
		{
			if ( t == target )
			{
				continue;
			}

			Vector3 toTarget = t.transform.position - this.player.fighter.transform.position;
			if ( Vector3.Dot( this.player.fighter.transform.forward, toTarget.normalized ) > 0.0f )
			{
				Vector3 worldPos = t.transform.position;
				
				Vector3 screenPos = Camera.main.WorldToScreenPoint( worldPos );
				screenPos.y = Screen.height - screenPos.y;

				Rect r = this.GetHealthGUIRect( t );
				this.DrawTargetBracket( r );
			}
		}
	}

	private void RenderFighterWeapons()
	{
		FighterWeapons weaponScript = this.player.fighter.weapons;
		GUI.Label( new Rect( 0, 200, 150, 50 ), "Weapon: " + weaponScript.primaryWeapon.weaponDesc.label );
	}

	private void RenderFighterEnergy()
	{
		if ( this.player.fighter.energySystem.currentEnergy < energyLowThreshold )
		{
			Rect r = new Rect();
			r.height = r.width = this.lowEnergyWarningSize;
			r.x = Screen.width/2 - this.fighterHealthBarOffset * Screen.width - this.fighterHealthBarWidth * 2 - r.width;
			r.y = Screen.height/2 - r.height/2;
			GUI.DrawTexture( r, this.lowEnergyWarningTexture );
		}
	}

	private void RenderFighterMissileLock()
	{
		if ( BulletManager.instance.IsMissileTargetting( this.player.fighter.health ) )
		{
			Rect r = new Rect();
			r.height = r.width = this.missileLockWarningSize;
			r.x = Screen.width/2 + this.fighterHealthBarOffset * Screen.width + this.fighterHealthBarWidth * 2;
			r.y = Screen.height/2 - r.height/2;
			GUI.DrawTexture( r, this.missileLockWarningTexture );
		}
	}

	public Rect GetHealthGUIRect( BaseHealth _health )
	{
		Rect r = new Rect();

		if ( _health.guiExtents.Length == 0 )
		{
			Vector3 pos = Camera.main.WorldToScreenPoint( _health.transform.position );
			pos.y = Screen.height - pos.y;
			r.x = pos.x; r.y = pos.y;
			r.height = r.width = this.minimumBracketScale;
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

			if ( r.width < this.minimumBracketScale )
			{
				float centre = r.x + r.width/2;
				r.x = centre - this.minimumBracketScale/2;
				r.width = this.minimumBracketScale;
			}

			if ( r.height < this.minimumBracketScale )
			{
				float centre = r.y + r.height/2;
				r.y = centre - this.minimumBracketScale/2;
				r.height = this.minimumBracketScale;
			}
		}
		return r;
	}

	public void DrawTargetBracket( Rect _r )
	{
		// Top left
		{
			GUI.DrawTexture( new Rect( _r.x, _r.y, this.targetBracketScale, this.targetBracketScale ), this.topLeftBracket );
		}

		// Top Right
		{
			GUI.DrawTexture( new Rect( _r.x + _r.width - this.targetBracketScale, _r.y, this.targetBracketScale, this.targetBracketScale ), this.topRightBracket );
		}

		// Bottom Right
		{
			GUI.DrawTexture( new Rect( _r.x + _r.width - this.targetBracketScale, _r.y + _r.height - this.targetBracketScale, this.targetBracketScale, this.targetBracketScale ), this.bottomRightBracket );
		}

		// Bottom Right
		{
			GUI.DrawTexture( new Rect( _r.x, _r.y + _r.height - this.targetBracketScale, this.targetBracketScale, this.targetBracketScale ), this.bottomLeftBracket );
		}
	}

	private void RenderVerticalBarOverlaid( Texture2D _tiledTexture, Rect _rect, float _ratio )
	{
		Rect innerRect = new Rect( _rect );

		innerRect.height *= _ratio;
		innerRect.y = _rect.y + (_rect.height - innerRect.height);
		GUI.DrawTexture( innerRect, _tiledTexture );

		GUI.DrawTextureWithTexCoords( _rect, this.verticalBarOverlayTexture, new Rect( 1.0f, 0.0f, 1.0f,  this.healthBarTileCount ) );
	}

	private void RenderHorizontalBarOverlaid( Texture2D _tiledTexture, Rect _rect, float _ratio )
	{
		Rect innerRect = new Rect( _rect );
		
		innerRect.width *= _ratio;
		GUI.DrawTexture( innerRect, _tiledTexture );
		
		GUI.DrawTextureWithTexCoords( _rect, this.verticalBarOverlayTexture, new Rect( 0.0f, 0.0f, this.healthBarTileCount, 1.0f ) );
	}

	private void RenderScoreBar()
	{
		Rect overlayRect = new Rect();
		overlayRect.width = this.scoreBarWidth * Screen.width;
		overlayRect.height = this.scoreBarHeight;
		overlayRect.x = Screen.width/2 - overlayRect.width/2;

		float s1 = ScoreManager.instance.team1Score.score;
		float s2 = ScoreManager.instance.team2Score.score;
		float ratio = (s1 + s2 != 0.0f) ? (s1 / (s1 + s2)) : 0.5f;

		Rect team1Rect = new Rect( overlayRect );
		Rect team2Rect = new Rect( overlayRect );
		team1Rect.width = team1Rect.width * ratio;
		team2Rect.width = team2Rect.width * ( 1.0f - ratio );
		team2Rect.x += team1Rect.width;

		GUI.DrawTexture( team1Rect, this.team1ScoreTexture );
		GUI.DrawTexture( team2Rect, this.team2ScoreTexture );

		Rect texCoords = new Rect( 0.0f, 0.0f, this.healthBarTileCount, 1.0f );
		GUI.DrawTextureWithTexCoords( overlayRect, this.horizontalBarOverlayTexture, texCoords );
	}

	private void RenderUpgradeMenu()
	{
		//First off, the multiplier upgrades

		string[] upgradeLabels = { "Defense ", "Speed ", "Energy "}; //This does not include weapon labels

		for(int typesOfUpgrades = 0; typesOfUpgrades < 3; typesOfUpgrades++) //Defense, Speed, Energy
		{
			for(int levelsOfUpgrades = 0; levelsOfUpgrades < 3; levelsOfUpgrades++) //level 1, level 2, level 3
			{
				Rect buttonRect = new Rect(30 + (80 * levelsOfUpgrades), 30 + (80 * typesOfUpgrades), 60, 60);
				GUI.Button (buttonRect, upgradeLabels[typesOfUpgrades] + "level " + levelsOfUpgrades);
			}
		}
	}
}
