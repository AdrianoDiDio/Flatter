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

/*
	Basically we could have 3 kind of Coin:
		- Golden that just increase the score.
		- Silver that gives some powerup to the player.
		- Bronze that increases player's health.
	So that OnPickup can be a generic function which interacts with the player.
	The only code shared by them is the OnCollisionEnter which calls the OnPickup.
	By Default OnPick
	Check if it is a trigger - Fail if not.
 */
abstract public class Coin : MonoBehaviour {
	private AudioSource CoinPickupSound;
	protected virtual void OnPickup() {
		GameHandler.Instance.PickedCoin(name);
		//Rewards the player.
	//	Player.Instance.PickupCoin(Value);
	}

	void Start() {

		CoinPickupSound = GetComponent<AudioSource>();
		if( CoinPickupSound == null ) {
			//Provide a default one.
			CoinPickupSound = gameObject.AddComponent<AudioSource>();
			CoinPickupSound.clip = Resources.Load("Sounds/Coin/sfx_coin_single2") as AudioClip;
			CoinPickupSound.playOnAwake = false;
			CoinPickupSound.spatialBlend = 0f; //2D Sound.
		}

	}

	void OnTriggerEnter2D(Collider2D CollisionInfo) {
		if( CollisionInfo.gameObject.tag != "Player" ) {
			return;
		}
		//CoinPickupSound.Play();
		//AudioSource.PlayClipAtPoint(CoinPickupSound.clip,gameObject.transform.position);
		SoundHelper.PlayClipAtPoint(CoinPickupSound.clip,transform.position);
		OnPickup();
		Destroy(gameObject);
	}
}
