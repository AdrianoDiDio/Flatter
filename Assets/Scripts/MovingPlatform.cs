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
﻿/*
	Adriano di Dio 28/08/18
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class MovingPlatform : MonoBehaviour {
	public enum WayPointCursorDir {
		CursorDir_UP,
		CursorDir_DOWN
	};
	public Transform[] WayPoints;
	[Header("Moving Platform Settings")]
	public float Speed;
	[Header("StartingWayPoint is the index to one of the WayPoints array.",order=0)]
	public int StartingWayPoint;
	[Header("StartingDirection is used to tell the platform at start whether",order=0)]
	[Space(-10,order=1)]
	[Header("to follow the waypoints in Ascending or Descending order.",order=2)]
	[Space(-10,order=3)]
	[Header("Note that when the platform has reached the end or the start of the array.",order=4)]
	[Space(-10,order=5)]
	[Header("It will go back to the previous position,thus not following the order.",order=6)]
	public WayPointCursorDir StartingDirection;
	[Header("CycleWaypoints:If set follow the StartingDirection directive",order=0)]
	[Space(-10,order=1)]
	[Header("by cycling when reaching the end of the array.",order=2)]
	public bool CycleWayPoints;
	private int CurrentWayPointIndex;
	private WayPointCursorDir CurrentCursorDir;
	private bool HasCollided;
	private bool HasCrushedPlayer;

	/*
		Increase the current waypoint index and if exceeds the length of
		the array reset it.
 	*/
	void IncreaseWayPointIndex() {
		//Debug.Log("Current waypoint index: " + CurrentWayPointIndex);
		//Debug.Log("Waypoint array length: " + WayPoints.Length);
		//If we have a single point this code is useless!
		if( WayPoints.Length == 1 ) {
			return;
		}
		//Debug.Log("CurrentCursorDir: " + CurrentCursorDir.ToString());
		//Debug.Log("CurrentWayPointIndex: " + CurrentWayPointIndex);
		if( CurrentCursorDir == WayPointCursorDir.CursorDir_UP ) {
			if( (CurrentWayPointIndex + 1) >= WayPoints.Length ) {
				//If CycleWayPoints cycle the array starting from the first element.
				if( CycleWayPoints ) {
					CurrentWayPointIndex = 0;
					return;
				}
				//Change direction immediately.
				CurrentCursorDir = WayPointCursorDir.CursorDir_DOWN;
				CurrentWayPointIndex--;
				return;
			}
			CurrentWayPointIndex++;
		} else {
			if( (CurrentWayPointIndex - 1) < 0 ) {
				//If CycleWayPoints cycle the array starting from the last element.
				if( CycleWayPoints ) {
					CurrentWayPointIndex = WayPoints.Length - 1;
					return;
				}
				//Change direction immediately.
				CurrentCursorDir = WayPointCursorDir.CursorDir_UP;
				CurrentWayPointIndex++;
				return;
			}
			CurrentWayPointIndex--;
		}
	}

	void OnValidate() {
		if( WayPoints == null || WayPoints.Length == 0 ) {
			return;
		}
		CurrentWayPointIndex = Mathf.Clamp(StartingWayPoint,0,WayPoints.Length - 1);
	}
#if UNITY_EDITOR
	void OnDrawGizmos() {
		if( WayPoints == null || WayPoints.Length == 0 ) {
			return;
		}
		//Preview position only in editor mode.
		if( !Application.isPlaying ) {
			gameObject.transform.position = WayPoints[CurrentWayPointIndex].position;
		}
		for( int i = 1; i < WayPoints.Length; i++ ) {
			Vector3 PrevWayPoint = WayPoints[i-1].position;
			Vector3 NextWayPoint = WayPoints[i].position;
			Vector3 MiddleWayPoint =  (PrevWayPoint + NextWayPoint) / 2;
			Gizmos.color = new Color(Random.value,Random.value,Random.value,1.0f);
			Gizmos.DrawLine(PrevWayPoint,NextWayPoint);
			Handles.Label(MiddleWayPoint,(i-1) + " -> " + i);
		}
	}
#endif
	
	void Start () {
		if( WayPoints == null || WayPoints.Length == 0 ) {
			Debug.Log("Platform has no waypoints...disabling.");
			gameObject.SetActive(false);
			return;
		}
		//CurrentWayPointIndex = 1;
		CurrentCursorDir = StartingDirection;
		HasCollided = false;
		HasCrushedPlayer = false;
	}
	
	void OnCollisionEnter2D(Collision2D CollisionInfo) {
		//Debug.Log("CollisionInfo Gameobject: " + CollisionInfo.gameObject.name );
		//Debug.Log("Collider Gameobject: " + CollisionInfo.collider.gameObject.name );
		if( CollisionInfo.collider.gameObject.tag == "Player" ) {
			//This occurs because when 2 BoxCollider intersects one is pushed onto the other
			//so it triggers both OnCollisionEnter and OnTriggerEnter causing player
			//to be linked to the platform...this is an issue since when GameOver is triggered
			//due to player death then it will unload the scene along with the moving platform
			//containing the player!
			if( HasCrushedPlayer ) {
				return;
			}
			CollisionInfo.collider.transform.SetParent(transform);
			return;
		}
		//Debug.Log("Collided with tile?");
		if( CollisionInfo.collider.gameObject.name == "Tilemap" ) {
			//Debug.Log("Collided increasing waypoint index...");
			//IncreaseWayPointIndex();
			HasCollided = true;
			return;
		}
//		CollisionInfo.collider.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
		//CollisionInfo.transform.SetParent(transform, false);
	}

	void OnCollisionExit2D(Collision2D CollisionInfo) {
		//Debug.Log("Collision exit");
		if( CollisionInfo.collider.gameObject.tag != "Player" ) {
			return;
		}
		CollisionInfo.collider.transform.SetParent(null);
		DontDestroyOnLoad(CollisionInfo.collider.gameObject);
		//CollisionInfo.collider.transform.SetParent(null);
		//CollisionInfo.collider.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
	}

		enum tempDir {
			Dir_LEFT,
			DIR_RIGHT,
			DIR_UP,
			DIR_DOWN,
			Dir_UNKNOWN
		}

	void OnTriggerEnter2D(Collider2D Other) {
		if( Other.tag != "Player" ) {
			return;
		}
		if( !GameHandler.Instance.GetPlayer().CurrentCollisionInfo.IsCollidingWithObject ) {
			return;
		}
		//Avoid recursive crush.
		if( GameHandler.Instance.GetPlayer().CurrentCollisionInfo.Object == gameObject ) {
			return;
		}
		//Crush it.
		GameHandler.Instance.GetPlayer().Unlink();
		GameHandler.Instance.GetPlayer().Damage(100);
		HasCrushedPlayer = true;
	}


	void FixedUpdate () {
		Vector3 Target = new Vector3(WayPoints[CurrentWayPointIndex].position.x,WayPoints[CurrentWayPointIndex].position.y,0f);
		if( Vector3.Equals(transform.position,Target) || HasCollided ) {
			IncreaseWayPointIndex();
			HasCollided = false;
			return;
		}
		transform.position = Vector3.MoveTowards(transform.position,Target,
													Speed * Time.fixedDeltaTime);
		//GetComponent<Rigidbody2D>().MovePosition(NewPos);
	}
}
