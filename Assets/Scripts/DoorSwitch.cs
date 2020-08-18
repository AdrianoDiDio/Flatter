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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorSwitch : MonoBehaviour,MiniGameListener {
	public enum DoorSwitchUnlockAction {
		ACTION_UNLOCK, // Simply unlock the attached door.
		// Unlock the attached door only if the player is able to solve a MiniGame which is 
		// attached to the script as a scene!
		ACTION_UNLOCK_WITH_MINIGAME
	}
	public enum DoorSwitchMiniGame {
		MINIGAME_15Puzzle
	}
	public DoorController Door;
	public DoorSwitchUnlockAction UnlockAction;
	[Header ("Valid only if ACTION_UNLOCK_MINIGAME is selected.")]
	public DoorSwitchMiniGame MiniGame;
	private Animator Anim;
	private AudioSource PushSound;

	private int PreviousSceneNumber;

	public bool Unlocked;

	public void UnlockDoor(bool Silent = false) {
		string Message = Silent ? "" : "Door is now unlocked!";
		Door.Unlock(Message);
		StartUnlockAnimation(Silent);
		Unlocked = true;
	}
	public void OnWin(DoorSwitchMiniGame Name) {
		UnlockDoor();
		CloseMiniGame(Name);
	}

	public void OnExit(DoorSwitchMiniGame Name) {
		CloseMiniGame(Name);
	}

	private void CloseMiniGame(DoorSwitchMiniGame Name) {
		string SceneName;
		if( Name == DoorSwitchMiniGame.MINIGAME_15Puzzle ) {
			SceneName = "15PuzzleMiniGameScene";
		} else {
			Debug.Log("Unimplemented Minigame scene close");
			return;
		}
		SceneManager.UnloadSceneAsync(SceneName);
		GameHandler.Instance.TogglePause(false);
		Debug.Log("Setting active :" + SceneManager.GetSceneByBuildIndex(PreviousSceneNumber).name);
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(PreviousSceneNumber));
		GameHandler.Instance.InMiniGame = false;
	}
	/*void OnCollisionEnter2D(Collision2D CollisionInfo) {
		//Only player can interact with switches!
		if( CollisionInfo.gameObject.GetComponent<Collider2D>().tag != "Player" ) {
			return;
		}
		//Anim.SetTrigger("PushDoorButton");
		//Door.Unlock("Door is now Unlocked!");
		//PushSound.Play();
		//SoundHelper.PlayClipAtPoint(GetComponent<AudioSource>().clip,transform.position);
		//TODO:Notify player!
		//Player.Instance.Damage(DamageFactor);
	}*/
	private IEnumerator LoadMiniGameScene(string MiniGameName) {
		AsyncOperation AsyncLoadLevel = SceneManager.LoadSceneAsync(MiniGameName,LoadSceneMode.Additive);
		while( !AsyncLoadLevel.isDone ) {
			yield return null;
		}
		Debug.Log("Loading minigame");
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(MiniGameName));
		GameObject.Find("MiniGameHandler").GetComponent<FifteenPuzzleGameHandler>().Init(this);
	}
	private void LaunchMiniGame() {
		if( GameHandler.Instance.InMiniGame ) {
			Debug.Log("Unfortunately we are still in a minigame...");
			return;
		}
		GameHandler.Instance.RegisterMiniGameListener(this);
		GameHandler.Instance.TogglePause(false);
		switch( MiniGame ) {
			case DoorSwitchMiniGame.MINIGAME_15Puzzle:
				PreviousSceneNumber = SceneManager.GetActiveScene().buildIndex;
				StartCoroutine(LoadMiniGameScene("15PuzzleMiniGameScene"));
				break;
			default:
				Debug.Log("Unimplemented minigame." + MiniGame);
				break;

		}
	}

	public void OnPush() {
		if( Unlocked ) {
			return;
		} 
		switch( UnlockAction ) {
			case DoorSwitchUnlockAction.ACTION_UNLOCK:
				UnlockDoor();
				break;
			case DoorSwitchUnlockAction.ACTION_UNLOCK_WITH_MINIGAME:
				LaunchMiniGame();
				break;
			default:
				Debug.Log("DoorSwitch:Unknown unlock action!");
				return;
		}
	}
	private void StartUnlockAnimation(bool Silent) {
		Anim.SetTrigger("PushDoorButton");
		if( Silent ) {
			return;
		}
		PushSound.Play();
	}

	void Start () {
		if( Door == null ) {
			Debug.Log("DoorSwitch without door!");
			return;
		}
		Anim = GetComponent<Animator>();
		PushSound = GetComponent<AudioSource>();
		Unlocked = false;
		if( UnlockAction == DoorSwitchUnlockAction.ACTION_UNLOCK_WITH_MINIGAME ) {
			//Prepare everything.
		}
	}
	
}
