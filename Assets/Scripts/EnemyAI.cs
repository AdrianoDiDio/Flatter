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
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour {
	public Transform  GroundCheck;
	public Transform ArmPosition; // Used to spawn the rocket.
	private Rigidbody2D RBody;
	private Animator Anim;
	private GameObject HealthInfo;
	private Slider HealthBar;
	private bool FlashEnabled;
	private float FlashStartedAt;
	private int  FlashCounter = 0;
	private Vector3 TargetPosition;
	private float LastFireTime = 0f;
	public  float GroundRadius;// = 1.5f;
	public LayerMask GroundMask;
	public float SightDistance;// = 20;

	private DoorController LockedDoor;

	//Debugging stuff!
	//private Text DebugEnemyStatus;

	private enum ActorState {
		AS_PATROL,
		AS_JUMP,
		AS_CHASE,
		AS_ATTACK
	}

	private ActorState CurrentState = ActorState.AS_PATROL;

	public bool HasLockedDoor() {
		return LockedDoor != null;
	}

	private void ShowHealthInfo() {
		Debug.Log("Showing my vitals");
		for( int i = 0; i < HealthInfo.transform.childCount; i++ ) {
			GameObject Child = HealthInfo.transform.GetChild(i).gameObject;
			//We have already enabled it!
			if( Child.activeSelf ) {
				return;
			}
			Debug.Log("ChildName: " + Child.name);
			if( Child.name == "HUD_Enemy_Health_Bar") {
				HealthBar = Child.GetComponent<Slider>();
				HealthBar.maxValue = 100;
				HealthBar.value = HealthBar.maxValue;
			}
			Child.SetActive(true);
		}
	}
	public void TakeDamage(float Amount) {
		//If flashing is already enabled enemy cannot be damaged.
		if( FlashEnabled ) {
			return;
		}
		ShowHealthInfo();
		HealthInfo.SetActive(true);
		HealthBar.value -= Amount;
		FlashStartedAt = Time.time;
		FlashEnabled = true;
	}
	
	void RemoveUIComponents() {
		//DebugEnemyStatus.GetComponent<Text>().enabled = false;
		HealthInfo.SetActive(false);
	}
	void OnLevelLoad(Scene Scene, LoadSceneMode SceneMode) {
		//Init();
		//RemoveUIComponents();
		
	}
	public void Init() {
		Debug.Log("Init!");
		RBody = GetComponent<Rigidbody2D>();
		Anim = GetComponent<Animator>();
		Anim.applyRootMotion = false;
		//gameObject.animat
		//.wrapMode = WrapMode.Once;
		HealthInfo = GameObject.Find("HUD_Enemy_Info");
		//DebugEnemyStatus = GameObject.Find("DebugEnemyStatus").GetComponent<Text>();
		FlashStartedAt = 0;
		GameObject Temp = GameObject.FindGameObjectWithTag("Door");
		if( Temp != null ) {
			LockedDoor = Temp.GetComponent<DoorController>();
			LockedDoor.LockedHintText = "Defeat brutus first.";
			LockedDoor.IsLocked = true;
		}
		Debug.Log("Hello from Brutus!");
	}
	// Use this for initialization
	void Start () {
		Init();
		//TODO:This will be our base class...then extend it as Brutus extends EnemyAI...
		/*GameObject ArmPos = new GameObject("ArmPosition");
		ArmPos.transform.localPosition = new Vector3(4.24f,4.04f,0f);
		ArmPosition = ArmPos.transform;
		ArmPosition.transform.SetParent(this.gameObject.transform,false);
		GameObject GroundCheckPos = new GameObject("GroundCheck");
		GroundCheckPos.transform.localPosition = new Vector3(0.4f,-0.53f,0f);
		GroundCheck = GroundCheckPos.transform;
		GroundCheck.transform.SetParent(this.gameObject.transform,false);*/

		//GroundMask = LayerMask.GetMask("Tile");
		//SightDistance = 20f;
		//GroundRadius = 1.5f;

		SceneManager.sceneLoaded += OnLevelLoad;
	}
	
	void SetRenderer(bool Enabled) {
		//NOTE: Assuming object has only 1 top-level child containing all the other...
		transform.GetChild(0).gameObject.SetActive(Enabled);
		/*Debug.Log("Child count: " + transform.childCount);
		for( int i = 0; i < transform.childCount; i++ ) {
			SpriteRenderer Child = transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>();
			if( Child == null ) {
				continue;
			}
			Debug.Log("Child" + Child.name);
			Child.enabled = Enabled;
		}*/
	}
	bool IsActive = true;
	private void CheckFlash() {
		float TimeDelta = Time.time - FlashStartedAt;
		// If 5 seconds have passed since we enabled flash disable it!
		if( TimeDelta > 2.5f ) {
			FlashStartedAt = 0f;
			FlashEnabled = false;
			if( !IsActive ) {
				SetRenderer(true);
			}
		}

		if( FlashEnabled ) {
			if( FlashCounter > 10 ) {
				if( IsActive ) {
					//Renderer.enabled = false;
					Debug.Log("Disabling");
					SetRenderer(false);
					IsActive = false;
			} else {
					Debug.Log("Enabling");
					SetRenderer(true);
					IsActive = true;
					//Renderer.enabled = true;
			}
					FlashCounter = 0;
			} else {
				FlashCounter++;
			}
		} else {
			FlashCounter = 0;
		}
	}
	private void FireRocket() {
		GameObject PRocket = Instantiate(Resources.Load("Prefabs/Rocket"),ArmPosition.position,transform.rotation) as GameObject;
		PRocket.SendMessage("Init",gameObject);
	}

	private bool IsOnGround() {
		RaycastHit2D Ray;

		Ray = Physics2D.Raycast(transform.position,Vector2.down,1f,GroundMask);
		Debug.DrawRay(transform.position,Vector2.down*5f,Color.green);
		if( Ray.collider != null ) {
			return true;
		}
		return false;
	}

	private void Flip(int Axis) {
		float YOffset = Axis == -1 ? 180f : 0f;
		Quaternion Rotation = Quaternion.Euler(0f,YOffset,0f);
		transform.rotation = Rotation;
	}
	private int GetCurrentPlayerDirection() {
		Vector3 Dir = TargetPosition - transform.position;
		if( Dir.x < 0 ) {
			return -1;
		}
		return 1;
	}

	private float GetSpeedOffsetBySkill() {
		switch( GameHandler.Instance.Skill ) {
			case GameHandler.PlayerSkill.Easy:
				return 2f;
			case GameHandler.PlayerSkill.Medium:
				return 4f;
			case GameHandler.PlayerSkill.Hard:
				return 6f;
			default:
				Debug.Log("Unknown skill.");
				break;
		}
		return 0f;
	}
	private void MoveToGoal() {
		//We've spotted the player now move towards him!
		int TileLayer = LayerMask.NameToLayer("Tile");
		float Distance = Vector3.Distance(transform.position,TargetPosition);
		int XOffset;		
		//Debug.Log("PlayerDistance: " + Distance);

		//if( Distance > 10 ) {
			//RBody.velocity = new Vector2(-1 * 2,RBody.velocity.y);
			XOffset = GetCurrentPlayerDirection();
			Flip(XOffset);
		//} else {
			//XOffset = 0;
		//}
		Debug.Log("XOffset: " + XOffset);
		RBody.velocity = new Vector2(XOffset * GetSpeedOffsetBySkill(),RBody.velocity.y);
		Anim.SetFloat("BrutusSpeed",Mathf.Abs(XOffset));
	}

	private void AttackPlayer() {
		float Delta = Time.time - LastFireTime;
		Debug.Log("Delta: " + Delta + " Last fire time: " + LastFireTime);
		if( Delta < 2 && LastFireTime > 0f ) {
			Anim.SetBool("BrutusAttack",false);
			return;
		}
		//Clear to go!
		//Aim to the player!
		//Flip(GetCurrentPlayerDirection());
		FireRocket();
		Anim.SetBool("BrutusAttack",true);
		LastFireTime = Time.time;
	}

	private void Jump() {
		if( !IsOnGround() ){
			//Vector2 ForwardMove = RBody.velocity;
			//ForwardMove.x = -4f;
			//RBody.velocity = ForwardMove;
			//CurrentState = ActorState.AS_PATROL;
			return;
		}
		//Basic jump.
		Vector2 JumpVel = new Vector2(0f,2f);
		RBody.velocity += JumpVel;
		//RBody.velocity = Vector2.up * 10f;
	//	RBody.AddForce(Diagonal,ForceMode2D.Impulse);
		//RBody.velocity = Diagonal;
	}

	private bool CastRay(int XOffset) {
		RaycastHit2D Ray;
		RaycastHit2D StepRay;
		int PlayerLayer = LayerMask.NameToLayer("Player");
		int TileLayer = LayerMask.NameToLayer("Tile");
		int Mask = 1 << PlayerLayer ;
		int StepMask = 1 << TileLayer;

		/*Debug.Log("Casting Yellow RAY (" + TileLayer +"," + XOffset + ")!");
		Vector3 LegPosition = new Vector3(ArmPosition.position.x,ArmPosition.position.y - 3f,ArmPosition.position.z);
		Ray = Physics2D.Raycast(LegPosition,Vector2.left * XOffset,SightDistance,1<<TileLayer);
		Debug.DrawRay(LegPosition,Vector3.left*SightDistance,Color.yellow,1);
		if( Ray.collider != null ) {
			Debug.Log("Yellow Collider: " + Ray.collider.name);
			if( Ray.collider.name == "Tilemap" ) {
				float TileDistance = Vector3.Distance(Ray.collider.transform.position,transform.position);
				//Debug.Log("Tile Distance: " + TileDistance);
				if( Ray.collider.transform.position.y < transform.position.y && TileDistance < 24 ) {
					CurrentState = ActorState.AS_JUMP;
					return true;
				}
			}
		}*/

		float StepDistance = 3f;
		Vector3 StepCheck = new Vector3((ArmPosition.position.x + GroundCheck.position.x) / 2,GroundCheck.position.y + 2f,0f);
		StepRay = Physics2D.Raycast(StepCheck,Vector2.left * XOffset,StepDistance,StepMask);
		Debug.DrawRay(StepCheck,Vector3.left*StepDistance,Color.blue,1);
		if( StepRay.collider != null ) {
			if(  StepRay.collider.name == "Tilemap" ) {
				//If we are near enough the edge...jump!
				float ObstacleDistance = Vector3.Distance(StepRay.collider.transform.position,StepCheck);
				Debug.Log("Obstacle at " + ObstacleDistance);
					CurrentState = ActorState.AS_JUMP;
					return true;
			}
		} else {
			Ray = Physics2D.Raycast(ArmPosition.position,Vector2.left * XOffset,SightDistance,Mask);
			Debug.DrawRay(ArmPosition.position,Vector3.left*SightDistance,Color.red,1);
			if( Ray.collider != null ) {
				if( Ray.collider.tag == "Player" ) {
					TargetPosition = Ray.collider.transform.position;
					if( Vector3.Distance(TargetPosition,transform.position) < 20 ) {
						Flip(GetCurrentPlayerDirection());
						CurrentState = ActorState.AS_ATTACK;
					} else {
						CurrentState = ActorState.AS_CHASE;
					}
					return true;
				}
			}
		}

		return false;
	}
	private void CheckState() {
		int Dir = 1;
		bool Found = false;

		if( !IsOnGround() && CurrentState == ActorState.AS_JUMP ) {
			return;
		} 
		//TODO:Do we really want to be able to spot player behind of us?
		for( int i = 0; i < 2; i++ ) {
			if( !CastRay(Dir) ) {
				Dir = -Dir;
			} else {
				Found = true;
			}
		}
		if( !Found ) {
			CurrentState = ActorState.AS_PATROL;
		}
	}
	private string GetCurrentState() {
		string Result = "AS_UNKNOWN";
		switch( CurrentState ) {
			case ActorState.AS_PATROL:
				Result = "AS_PATROL";
				break;
			case ActorState.AS_JUMP:
				Result = "AS_JUMP";
				break;
			case ActorState.AS_CHASE:
				Result = "AS_CHASE";
				break;
			case ActorState.AS_ATTACK:
				Result = "AS_ATTACK";
				break;
		}
		return Result;
	}
	private void DoState() {
		//DebugEnemyStatus.text = "Enemy Status: " + CurrentState;
		switch( CurrentState ) {
			case ActorState.AS_PATROL:
				Anim.SetBool("BrutusAttack",false);
				break;
			case ActorState.AS_JUMP:
				Jump();
				break;
			case ActorState.AS_CHASE:
				ShowHealthInfo();
				MoveToGoal();
				//FireRocket();
				//Anim.SetBool("BrutusAttack",true);
				break;
			case ActorState.AS_ATTACK:
				MoveToGoal();
				AttackPlayer();
				break;
		}
	}
	private void FixedUpdate() {
		if( HealthBar && HealthBar.value <= 0 ) {
			//Brutus is dead!
			GameHandler.Instance.GetPlayer().AddScore(50);
			LockedDoor.Unlock("Door is now unlocked!");
			RemoveUIComponents();
			Destroy(this.gameObject);
			return;
		}
		CheckFlash();
		CheckState();
		DoState();
		//CurrentState = ActorState.AS_PATROL;
	}
}
