/*
===========================================================================
    Copyright (C) 2018 Adriano Di Dio.
    
    Flatter is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Flatter is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Flatter.  If not, see <http://www.gnu.org/licenses/>.
===========================================================================
*/ 
﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour,GameTimerInterface {
	public struct CrushInfo {
		public bool IsCollidingWithObject;
		public GameObject Object; //If not null,it will be used to compare against.
	}
	public float MaxSpeed = 10f;
	public float JumpVelocity = 5f;
	private Rigidbody2D RBody;
	private Animator Anim;
	private bool FacingRight;
	public bool IsOnGround;
	public bool IsPaused;
	public bool IsDead;
	private int Score;
	public Transform GroundPosition;
	public Transform ArmPosition; // Used to spawn the rocket.
	public float GroundRadius; // Radius of the ground check cirlce.
	public LayerMask GroundMask;
	// Player controls.
	float XInputOffset; //Amount of movement along X axis.
	bool UpPressed; // Jump.
	bool ActionPressed;

	private float LastRocketFireTime;
	[SerializeField] private Slider HealthBar;
	//TODO:Still not sure if this is needed....
	public PlayerObjectManager Inventory;
	//public static Player Instance = null;

	//RigidBody Pause stuff
	private Vector3 SavedVelocity;
	private float SavedAngularVelocity;
	private float FallTime = 0f;

	private Text ScoreText;

	private GameObject PowerUpTimerPanel;
	private GameObject InvulnerabilityPowerUpPanel;
	private GameObject SpeedPowerUpPanel;
	private Timer InvulnerabiliyPowerupTimer;
	private Timer SpeedPowerupTimer;

	private bool Invulnerability;
	private bool SpeedBooster;

	private AudioSource DamageSound;
	private AudioSource ShootSound;
	private AudioSource JumpSound;
	private AudioSource LandSound;
	public CrushInfo CurrentCollisionInfo;


	public enum AndroidButton {
		BUTTON_LEFT,
		BUTTON_RIGHT,
		BUTTON_UP,
		BUTTON_ACTION, // Shoot/Do something.
		BUTTON_UNKNOWN
		
	}

	public void CleanUp() {
		Destroy(gameObject);
	}

	private void DisableInvulnerability() {
		if( !Invulnerability ) {
			return;
		}
		//PowerUpTimerPanel.SetActive(false);
		InvulnerabilityPowerUpPanel.SetActive(false);
		Invulnerability = false;
	}

	private void DisableSpeedBooster() {
		if( !SpeedBooster ) {
			return;
		}
		//PowerUpTimerPanel.SetActive(false);
		SpeedPowerUpPanel.SetActive(false);
		SpeedBooster = false;
	}
	/*
		Unlink the player to the moving platform.
	 */
	public void Unlink() {
		Debug.Log("Transform Parent:");
		if( transform.parent == null ) {
			return;
		}
		Debug.Log("Unlinking player.");
		transform.parent = null;
		DontDestroyOnLoad(gameObject);
	}
	public void DropPowerUps() {
		DisableInvulnerability();
		DisableSpeedBooster();
	}

	public void OnTimeOut(Guid InstanceID) {
		if( InstanceID == InvulnerabiliyPowerupTimer.InstanceID ) {
			Debug.Log("Invulnerability Timed Out!");
			DisableInvulnerability();
			return;
		}
		if( InstanceID == SpeedPowerupTimer.InstanceID ) {
			Debug.Log("Speed Timed Out!");
			DisableSpeedBooster();
			return;
		}
	}

	public void GiveHealth(float HealthAmount) {
		if( HealthBar.value >= 100 ) {
			return;
		}
		HealthBar.value += HealthAmount;
	}

	public void SetHealth(float HealthValue) {
		HealthBar.value = HealthValue;
		return;
	}
	public float GetHealth() {
		return HealthBar.value;
	}
	public int GetScore() {
		return Score;
	}
	
	/*
	 */
	public void AddScore(int Value) {
		Score += Value;
		ScoreText.text = Score.ToString();
	}

	public void GiveInvulnerability() {
		//PowerUpTimerPanel.SetActive(true);
		InvulnerabilityPowerUpPanel.SetActive(true);
		Invulnerability = true;
		InvulnerabiliyPowerupTimer.Reset(10);
	}

	public void GiveSpeedBoost() {
		//PowerUpTimerPanel.SetActive(true);
		SpeedPowerUpPanel.SetActive(true);
		SpeedBooster = true;
		SpeedPowerupTimer.Reset(10);
	}

	
	public void Damage(float Amount) {
		if( Invulnerability ) {
			return;
		}
		HealthBar.value -= Amount;
		DamageSound.Play();
		if( HealthBar.value <= 0 ) {
			Debug.Log("Player died!");
		}
	}

	private Collider2D GetNearestSwitch() {
		//Raycast to check if near a switch!
		int ObjectLayer = LayerMask.NameToLayer("Object");
		int Mask = 1 << ObjectLayer;
		Vector3 Forward = gameObject.transform.rotation * Vector3.right;
		Debug.Log("Forward is: " + Forward.ToString());
		Debug.Log("Mask Is: " + ObjectLayer +  " " + Mask);
		Debug.Log("Checking it " + Forward.ToString());
		Debug.Log("Player Pos: " + ArmPosition.position);
		Vector3 StartPos = gameObject.transform.position + (Forward*2f);
		RaycastHit2D Ray = Physics2D.Raycast(StartPos,Forward,0.1f,Mask);
		Debug.DrawRay(StartPos,Forward,Color.blue,1);
		if( Ray.collider != null ) {
			Debug.Log("Collided with " + Ray.collider.name);
			if( Ray.collider.tag == "DoorSwitch" ) {
				return Ray.collider;
			}
		}
		return null;
	}
	public void OnAndroidButtonUp(AndroidButton Button) {
		switch( Button ) {
			case AndroidButton.BUTTON_LEFT:
			case AndroidButton.BUTTON_RIGHT:
				XInputOffset = 0;
				break;
			case AndroidButton.BUTTON_UP:
				UpPressed = false;
				break;
			case AndroidButton.BUTTON_ACTION:
				Debug.Log("Releasing actionpressed");
				ActionPressed = false;
				break;
			case AndroidButton.BUTTON_UNKNOWN:
			default:
				Debug.Log("OnAndroidButtonUp:Unknown Button " + Button);
				break;


		}
	}

	public void OnAndroidButtonDown(AndroidButton Button) {
		switch( Button ) {
			case AndroidButton.BUTTON_LEFT:
				XInputOffset = -1;
				break;
			case AndroidButton.BUTTON_RIGHT:
				XInputOffset = 1;
				break;
			case AndroidButton.BUTTON_UP:
				UpPressed = true;
				break;
			case AndroidButton.BUTTON_ACTION:
				//Maybe use it to activate a button under certain conditions.
				ActionPressed = true;
				break;
			case AndroidButton.BUTTON_UNKNOWN:
			default:
				Debug.Log("OnAndroidButtonUp:Unknown Button " + Button);
				break;
		}
	}

	private void InitPowerUpsTimer() {
		PowerUpTimerPanel = GameObject.Find("HUD_Panel/HUD_PowerUpPanel");
		InvulnerabilityPowerUpPanel = GameObject.Find("HUD_Panel/HUD_PowerUpPanel/InvulnerabilityPanel");
		SpeedPowerUpPanel = GameObject.Find("HUD_Panel/HUD_PowerUpPanel/SpeedPanel");
		
		InvulnerabiliyPowerupTimer = InvulnerabilityPowerUpPanel.GetComponentInChildren<Timer>();
		InvulnerabiliyPowerupTimer.Init(this,10,false);
		SpeedPowerupTimer = SpeedPowerUpPanel.GetComponentInChildren<Timer>();
		SpeedPowerupTimer.Init(this,10,false);

		//PowerUpTimerPanel.SetActive(t);
		InvulnerabilityPowerUpPanel.SetActive(false);
		SpeedPowerUpPanel.SetActive(false);
	}
	private AudioSource AddAudioSource(string SoundName) {
		AudioSource Temp;
		Temp = gameObject.AddComponent<AudioSource>();
		Temp.clip  = Resources.Load("Sounds/Player/" + SoundName) as AudioClip;
		Temp.playOnAwake = false;
		Temp.spatialBlend = 0f;
		return Temp;
		
	}
	private void InitAudioEffects() {
		DamageSound = AddAudioSource("sfx_damage_hit1");
		ShootSound = AddAudioSource("sfx_wpn_laser3");
		JumpSound = AddAudioSource("sfx_movement_jump15");
		LandSound = AddAudioSource("sfx_movement_jump15_landing");
	}
	void Awake () {
		RBody = GetComponent<Rigidbody2D>();
		Anim  = GetComponent<Animator>();
		ScoreText = GameObject.Find("HUD_Score_Text").GetComponent<Text>();
		HealthBar = GameObject.Find("HUD_Health_Bar").GetComponentInChildren<Slider>();
		Debug.Log("HUD Score: " + ScoreText == null);
		Debug.Log("Hud Health: " + HealthBar == null);
		Inventory = new PlayerObjectManager();
		GroundRadius = 0.5f;
		XInputOffset = 0f;
		ActionPressed = false;
		UpPressed = false;
		FacingRight = true;
		IsOnGround = false;
		IsPaused = false;
		LastRocketFireTime = 0f;
		CurrentCollisionInfo.Object = null;
		CurrentCollisionInfo.IsCollidingWithObject = false;
		//TODO:Load this together with the player!
		Score = 0;
		DontDestroyOnLoad(this.gameObject);
		InitPowerUpsTimer();
		InitAudioEffects();
	}
	
	public void RestoreRotation(Quaternion Rotation) {
		gameObject.transform.rotation = Rotation;
		if( Rotation.y == 180 ) {
			FacingRight = true;
		} else {
			FacingRight = false;
		}
	}
	// Rotate player sprite.
	public void Flip() {
		Quaternion Rotation;
		float YOffset;
		FacingRight = !FacingRight;
		YOffset = FacingRight == true ? 0f : 180f;
		Rotation = Quaternion.Euler(0f,YOffset,0f);
		transform.rotation = Rotation;

	}

	private void HandleAnimationLayers() {
		if( !IsOnGround ) {
			// Use Air layer.
			Anim.SetLayerWeight(1,1);
		} else {
			Anim.SetLayerWeight(1,0);
		}
	}



	private void SetAllAnimationsEnableState(bool State) {
		var AllAnims = FindObjectsOfType<Animator>();
		foreach( var Anim in AllAnims ) {
			Anim.enabled = State;
		}
	}

	private void SetAllSawtrapsState(bool State) {
		var Sawtraps = FindObjectsOfType<SawTrap>();
		foreach( var Sawtrap in Sawtraps ) {
			Sawtrap.Enabled = State;
		}
	}

	public void FreezePlayer(bool Freeze,bool PlayerDied = false) {
		if( Freeze ) {
			if( PlayerDied ) {
				IsDead = true;
			} else {
				IsPaused = true;
			}
			if( InvulnerabiliyPowerupTimer.IsRunning() ) {
				InvulnerabiliyPowerupTimer.Pause();
			}
			if( SpeedPowerupTimer.IsRunning() ) {
				SpeedPowerupTimer.Pause();
			}
			//SetAllAnimationsEnableState(false);
			//SetAllSawtrapsState(false);
			//Anim.enabled = false;
			SavedVelocity = RBody.velocity;
			SavedAngularVelocity = RBody.angularVelocity;
			RBody.velocity = Vector3.zero;
			RBody.angularVelocity = 0;
			RBody.isKinematic = true;
		} else {
			// Unfreeze
			IsPaused = false;
			if( InvulnerabiliyPowerupTimer.IsPaused ) {
				InvulnerabiliyPowerupTimer.Resume();
			}
			if( SpeedPowerupTimer.IsPaused ) {
				SpeedPowerupTimer.Resume();
			}
			//SetAllAnimationsEnableState(true);
			//Anim.enabled = true;
			RBody.velocity = SavedVelocity;
			RBody.angularVelocity = SavedAngularVelocity;
			RBody.isKinematic = false;
			RBody.WakeUp();
		}
	}

	private void FireRocket() {
		//Rocket PRocket = new Rocket(this.gameObject);
		GameObject PRocket = Instantiate(Resources.Load("Prefabs/Rocket"),ArmPosition.position,transform.rotation) as GameObject;
		//Ignore collision with ourself..
		//Physics2D.IgnoreCollision(PRocket.GetComponent<Collider2D>(),GetComponent<Collider2D>());
		ShootSound.Play();
		PRocket.SendMessage("Init",gameObject);
		LastRocketFireTime = Time.time;
	}
	//TODO:Maybe use an array of 2 elements to determine player interaction with other objects?
	void OnCollisionEnter2D(Collision2D CollisionInfo) {
		if( CollisionInfo.collider.name == "Tilemap" || CollisionInfo.collider.tag == "MovingPlatform" ) {
			CurrentCollisionInfo.Object = CollisionInfo.collider.gameObject;
			CurrentCollisionInfo.IsCollidingWithObject = true;
			return;
		}
	}

	void OnCollisionExit2D(Collision2D CollisionInfo) {
		if( CollisionInfo.collider.name == "Tilemap" || CollisionInfo.collider.tag == "MovingPlatform" ) {
			CurrentCollisionInfo.Object = null;
			CurrentCollisionInfo.IsCollidingWithObject = false;
			return;
		}
	}

	void FixedUpdate () {
		Collider2D DoorSwitch = null;
		float FinalSpeed;
		//CheckEscapeKey();

		if( IsPaused || IsDead ) {
			return;
		}

		IsOnGround = Physics2D.OverlapCircle(GroundPosition.position,GroundRadius,GroundMask);
		//Debug.Log("IsOnGround: " + IsOnGround);
		//In general set health to -99999 and check for this!
		//If not on ground and we have registered our fall time...
		if( !IsOnGround && FallTime > 0) {
			float Delta = Time.time - FallTime;
			if( Delta > 3f ) {
				Damage(100);
			}
		} else {
			//Debug.Log("Stopped fall time");
			FallTime = 0f;
		}
		if( HealthBar != null && HealthBar.value <= 0 ) {
			//FreezePlayer(true,true);
			GameHandler.Instance.OnGameOver();
			return;
		} 
		//Anim.SetFloat("VerticalSpeed",RBody.velocity.y);
		
		// Reset Jump animation parameters if we are on ground. 
		if( IsOnGround && ( Anim.GetBool("Jump") || Anim.GetBool("Land") ) ) {
			Anim.SetBool("Jump",false);
			Anim.SetBool("Land",false);
			LandSound.Play();
		}

		// Jump and also sets the correct animation.
		if( IsOnGround && UpPressed ) {
			Debug.Log("Jumping!");
			RBody.velocity = Vector2.up * JumpVelocity;
			Anim.SetBool("Jump",true);
			JumpSound.Play();
		}
		FinalSpeed = SpeedBooster ? MaxSpeed + 5f : MaxSpeed;
		RBody.velocity = new Vector2(XInputOffset * FinalSpeed,RBody.velocity.y);
		Anim.SetFloat("PlayerSpeed",Mathf.Abs(XInputOffset));
		if( XInputOffset > 0 && !FacingRight ) {
			Flip();
		} else if( XInputOffset < 0 && FacingRight ) {
			Flip();
		}

		// End of jump/ Free fall.
		if( !IsOnGround && RBody.velocity.y < 0 ) {
			RBody.velocity += Vector2.up * Physics2D.gravity * 1.5f * Time.deltaTime;
			Anim.SetBool("Land",true);
			//Make sure we don't overwrite the previous frame value.
			if( FallTime == 0f ) {
				FallTime = Time.time;
			}
		} else if( RBody.velocity.y > 0 ) {
			RBody.velocity += Vector2.up * Physics2D.gravity * 1f * Time.deltaTime;
		}

		//Shoot
		if( ActionPressed && (DoorSwitch = GetNearestSwitch()) != null ) {
			//Push it.
			//Handle it
			Debug.Log("Pressing the switch!");
			DoorSwitch Switch = DoorSwitch.GetComponent<DoorSwitch>();
			if( Switch == null ) {
				Debug.Log("Warning Gameobject marked as DoorSwitch has not a doorswitch script attached!");
				return;
			}
			Switch.OnPush();
			ActionPressed = false;
		} else {
			float Delta = Time.time - LastRocketFireTime;
			//Rate limit.
			if( Anim.GetBool("Shoot") ) {
				Anim.SetBool("Shoot",false);
			}
			if( Delta < 2 && LastRocketFireTime > 0f ) {
				ActionPressed = false;
				return;
			}
			if( ActionPressed ) {
				FireRocket();
				Anim.SetBool("Shoot",true);
				LastRocketFireTime = Time.time;
			}
			HandleAnimationLayers();
		}
	}

	void Update() {
		//Perform input detection here.
		if( Application.platform != RuntimePlatform.Android ) {
			/*if( Input.GetMouseButton(0) ) {
				if( EventSystem.current.IsPointerOverGameObject() ) {
					if( InputManager.HasAnyoneBindMouse0() ) {

					}
				}
			}*/
			if( InputManager.Instance.HasPressed(InputManager.Instance.LeftButton) ) {
				XInputOffset = -1;
			} else if( InputManager.Instance.HasPressed(InputManager.Instance.RightButton) ) {
				XInputOffset = 1;
			} else {
				XInputOffset = 0;
			}
			UpPressed = InputManager.Instance.HasPressed(InputManager.Instance.UpButton);
			ActionPressed = InputManager.Instance.HasPressed(InputManager.Instance.ActionButton);
		}
	}
}
