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

public class SawTrap : MonoBehaviour {
	public bool   Enabled;
	public float  DamageFactor;
	public float RotationSpeed;

	private float GetSkillDamageOffset() {
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
	void OnCollisionEnter2D(Collision2D CollisionInfo) {
		//If a collision occurs and it's the player...Damage him!
		if( CollisionInfo.gameObject.GetComponent<Collider2D>().tag != "Player" ) {
			return;
		}
		GameHandler.Instance.GetPlayer().Damage(DamageFactor * GetSkillDamageOffset());
	}
	void OnCollisionStay2D(Collision2D CollisionInfo) {
		//If a collision occurs and it's the player...Damage him!
		if( CollisionInfo.gameObject.GetComponent<Collider2D>().tag != "Player" ) {
			return;
		}
		GameHandler.Instance.GetPlayer().Damage(DamageFactor * GetSkillDamageOffset());	
	}

	void Start () {
		
	}
	
	void Update () {
		if( !Enabled ) {
			return;
		}
		transform.Rotate(Vector3.forward * Time.deltaTime * RotationSpeed);
	}
}
