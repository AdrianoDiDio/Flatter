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
using UnityEngine.Tilemaps;
public class DoorController : MonoBehaviour {
	private AudioSource OpenSound;
	public bool IsLocked;
	public string LockedHintText;
	private Player MPlayer;

	private Animator Anim;
	private Text	LockedHintLabel;

	/*
		Refresh all tiles in the given range.
	 */
	void RefreshTilesInRange(Vector3Int Position,Vector2Int Min,Vector2Int Max) {
		for( int x = Min.x; x < Max.x; x++ ) {
			//TMap.RefreshTile(new Vector3Int(Position.x + x,Position.y,0));
		}
		for( int y = Min.y; y < Max.y; y++ ) {
			//TMap.RefreshTile(new Vector3Int(Position.x,Position.y + y,0));
		}
	}
	public void Init() {
		MPlayer = GameHandler.Instance.GetPlayer();
		Debug.Log("Greetings from the door!");
		Anim = gameObject.GetComponent<Animator>();
		LockedHintLabel = GameObject.Find("DoorHintText").GetComponent<Text>();
		LockedHintLabel.enabled = false;
		OpenSound = GetComponent<AudioSource>();

	}
	void Awake () {
		if( GameHandler.Instance == null ) {
			Debug.Log("Door awoken without gamehandler.");
			return;
		}
		Init();
		//TMap = gameObject.GetComponent<Tilemap>();
		//Debug.Log("Tiles Position within " + TMap.name);
		/*BoundsInt bounds = TMap.cellBounds;
		TileBase[] CTile = TMap.GetTilesBlock(bounds);
		for( int x = 0; x < bounds.size.x; x++ ) {
			for( int y = 0; y < bounds.size.y; y++ ) {
				TileBase Tile = CTile[x + y * bounds.size.x];
				if( Tile != null ) {
					AnimatedTile ATile = (AnimatedTile) Tile;
					ATile.m_IsActive = true;
					RefreshTilesInRange(new Vector3Int(x,y,0),new Vector2Int(-1,-1),new Vector2Int(1,1));
				}
			}
		}*/
	}
	
	private IEnumerator OnAnimationEnd() {
		//yield return new WaitForSeconds(Anim.runtimeAnimatorController.animationClips[0].length);
		Debug.Log("Door:Animation End.");
		do {
			yield return null;
		} while (Anim.GetCurrentAnimatorStateInfo(0).IsName("UnlockedDoorOpenAnimation"));
		//TODO:Attach next scene to the door!
		GameHandler.Instance.GetPlayer().Unlink();
		GameHandler.Instance.GetPlayer().DropPowerUps();
		GameHandler.Instance.GetPlayer().AddScore(5);
		string NextScene = SceneUtils.GetNextSceneName();
		if( NextScene == string.Empty || !NextScene.StartsWith("Level")) {
			GameObject.Find("HUD_Panel").SetActive(false);
			GameHandler.Instance.GetPlayer().gameObject.SetActive(false);
			SceneManager.LoadScene("VictoryScene");
		} else {
			StartCoroutine(SceneUtils.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1));
		}
		//SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		//SceneManager.MoveGameObjectToScene(Player,SceneManager.GetSceneByName("Level2"));
		//SceneManager.MoveGameObjectToScene(Camera.main.gameObject,SceneManager.GetSceneByName("Level2"));
		//SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
	}
	/*
		Display the locked hint for 2 seconds then disable it!
	 */
	private IEnumerator ShowHint(string Message) {
		if( Message != null ) {
		 LockedHintLabel.text = Message;
		}
		LockedHintLabel.enabled = true;
		yield return new WaitForSeconds(1);
		LockedHintLabel.enabled = false;
	}

	public void Unlock(string Text) {
		IsLocked = false;
		//Doublecheck...Not worth calling a coroutine without a valid string!
		if( Text != null ) {
			StartCoroutine(ShowHint(Text));
		}
	}
	void OnTriggerEnter2D(Collider2D CollisionInfo) {
		// Only player can interact with doors.
		if( CollisionInfo.gameObject.tag != "Player" ) {
			return;
		}
		Debug.Log("Opening for " + CollisionInfo.gameObject.tag);
		if( IsLocked ) {
			StartCoroutine(ShowHint(LockedHintText));
			//if( MPlayer.Inventory.HasKey ) {
			//	IsLocked = false;
			//}
		} else {
			Debug.Log("Opening door");
			//Play Animation and Load next scene!
			OpenSound.Play();
			Anim.Play("UnlockedDoorOpenAnimation");
			StartCoroutine(OnAnimationEnd());
		}
	}

	void OnTriggerExit2D(Collider2D CollisionInfo) {
		if( IsLocked ) {
			return;
		}
		Anim.Play("UnlockedDoorCloseAnimation");
	}
}
