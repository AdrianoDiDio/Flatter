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

public class Rocket : MonoBehaviour {
	public float Speed;
	public float MaxDistance;
	public GameObject Owner;
	private Vector3 StartPos;
	private Vector3 Forward;

	public void Init(GameObject Owner) {
		this.Owner = Owner;
		StartPos = Owner.transform.position;
		Forward =  gameObject.transform.rotation * gameObject.transform.right;
		//Ignore collisions with owner!
		Physics2D.IgnoreCollision(Owner.GetComponent<Collider2D>(),gameObject.GetComponent<Collider2D>());
	}
	void Start () {
	}
	
	void Update () {
	}
	
	private float GetEnemyToPlayerDamage() {
		switch( GameHandler.Instance.Skill ) {
			case GameHandler.PlayerSkill.Easy:
				return 5f;
			case GameHandler.PlayerSkill.Medium:
				return 10f;
			case GameHandler.PlayerSkill.Hard:
				return 20f;
			default:
				Debug.Log("Unknown skill.");
				break;
		}
		return 0f;
	}
	private float GetPlayerToEnemyDamage() {
		switch( GameHandler.Instance.Skill ) {
			case GameHandler.PlayerSkill.Easy:
				return 20f;
			case GameHandler.PlayerSkill.Medium:
				return 15f;
			case GameHandler.PlayerSkill.Hard:
				return 10f;
			default:
				Debug.Log("Unknown skill.");
				break;
		}
		return 0f;
	}

	void OnCollisionEnter2D(Collision2D cInfo) {
		Debug.Log("Owner: " + Owner.tag + "cInfo: " + cInfo.gameObject.tag);
		//If we hit something we need to explode...
		//Add damage to enemy and let him flash (Invlunerability!)
		if( Owner.tag == "Player" && cInfo.gameObject.tag == "Enemy" ) {
			EnemyAI Enemy = cInfo.gameObject.GetComponent<EnemyAI>();
			Enemy.TakeDamage(GetPlayerToEnemyDamage());
		} else if ( Owner.tag == "Enemy" && cInfo.gameObject.tag == "Player" ) {
			GameHandler.Instance.GetPlayer().Damage(GetEnemyToPlayerDamage());
		}
		Destroy(gameObject);
	}

	void FixedUpdate() {
		float Distance = Vector3.Distance(StartPos,transform.position);
		Debug.Log("Going :" + Forward.ToString());
		gameObject.transform.Translate(Forward * Speed);
		if( Distance > MaxDistance ) {
			Destroy(gameObject);
		}
	}
}
