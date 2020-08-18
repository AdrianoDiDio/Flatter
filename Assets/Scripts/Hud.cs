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

public class Hud : MonoBehaviour {
	private IEnumerator TextRoutine;
	private string CurrentSceneName;
	private bool SceneChanged;
	private Text ObjectiveTextEntry;
	private bool CoroutineRunning;
	//On Scene loaded => Update object string!

	public void Deactivate() {
		SceneManager.sceneLoaded -= OnSceneLoaded;
		gameObject.SetActive(false);
	}
	public void ForceSceneChange() {
		Debug.Log("Hud Scene changed.");
		if( CoroutineRunning ) {
			return;
		}
		SceneChanged = true;
	}
	public void CleanUp() {
		Debug.Log("HUD Cleanup called.");
		//SceneManager.sceneLoaded -= OnSceneLoaded;
		Destroy(gameObject);
	}

	private IEnumerator AnimateString(string Text) {
		string DisplayText = Text;
		CoroutineRunning = true;
		for( int i = 0; i < DisplayText.Length; i++ ) {
			ObjectiveTextEntry.text += DisplayText[i];
			yield return new WaitForSeconds(0.1f);
		}
		CoroutineRunning = false;
	}
	private void OnSceneLoaded(Scene NScene,LoadSceneMode Mode) {
		Debug.Log("HUD Scene Loaded called");
		Debug.Log("HUD Scene Name: " + NScene.name);
		if( !NScene.name.StartsWith("Level") ) {
			return;
		}
		CurrentSceneName = NScene.name;
		SceneChanged = true;
	}

	public void Init() {
		ObjectiveTextEntry = GameObject.Find("HUD_Objective_Text").GetComponent<Text>();
		CurrentSceneName = SceneManager.GetActiveScene().name;
		TextRoutine = AnimateString("");
		CoroutineRunning = false;
		SceneChanged = true;
		SceneManager.sceneLoaded += OnSceneLoaded;
		DontDestroyOnLoad(gameObject);
	}
	void Awake () {
		Init();
	}
	
	// Update is called once per frame
	void Update () {
		if( SceneChanged ) {
			Debug.Log("Scene changed: " + CurrentSceneName);
			if( CoroutineRunning ) {
				StopCoroutine(TextRoutine);
			}
			ObjectiveTextEntry.text = "";
			switch(CurrentSceneName) {
				case "Level1":
					TextRoutine = AnimateString("Find the door!");
					break;
				case "Level2":
					TextRoutine = AnimateString("Find the switch!");
					break;
				case "Level3":
					TextRoutine = AnimateString("Defeat Brutus!");
					break;
				default:
					ObjectiveTextEntry.text = "";
				break;
			}
			StartCoroutine(TextRoutine);
			SceneChanged = false;
		}
	}
}
