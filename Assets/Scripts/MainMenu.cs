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
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*
	TODO:Make an abstract class which contains Buttons array,Init and OnButtonClick.
 */
public class MainMenu : Menu,UiDialog.UiDialogInputInterface {

	public GameObject NewGamePanel;

	private void OpenNewGamePanel() {
		gameObject.SetActive(false);
		NewGamePanel.SetActive(true);
	}


	/*private IEnumerator LoadSavedScene(SaveManager.SaveGame SavedGame) {
		GameHandler.InstanceGameHandler(SavedGame);
		AsyncOperation AsyncSceneLoad = SceneManager.LoadSceneAsync(SavedGame.SceneName);
		while( !AsyncSceneLoad.isDone ) {
			yield return null;
		}
	}*/
	public void OnEndEdit(InputField InField) {
		Debug.Log("Your name is: " + InField.text);
		int ScoreTest = 10;
		if( LeaderBoard.Instance.IsHighScore(ScoreTest) ) {
			Debug.Log("New record!");
			LeaderBoard.Instance.AddRank(InField.text,ScoreTest);
		} else {
			Debug.Log("BOOOOH!");
		}

	}
	private void Playtest() {
		GameObject LeaderBoard = UiDialog.ShowInputDialog("New record","Enter your name",null,this);
	}

	protected override void OnButtonClick(string ButtonName) {
		switch (ButtonName)
		{
			case "PlayButton":
				//InitPlayButtons();
				OpenNewGamePanel();
				break;
			case "QuitButton":
				//Playtest();
				Application.Quit();
				break;
			default:
				Debug.Log("Unhandled button: " + ButtonName);
				break;
		}
	}



	private void InitPlayButtons() {
		this.gameObject.SetActive(false);
		NewGamePanel.SetActive(true);
		Button[] PlayButtons = NewGamePanel.GetComponentsInChildren<Button>();
		foreach( Button Component in PlayButtons ) {
			Debug.Log("Component name: "+ Component.name);
			if(GameHandler.Instance != null ) {
				Debug.Log("GameHandler Is Null + Is Paused" + GameHandler.Instance == null +  GameHandler.Instance.IsPaused);
			}
			if( Component.tag == "SkipMenuHandler" ) {
				continue;
			}
			if( GameHandler.Instance != null && GameHandler.Instance.IsPaused) {
				if( Component.name == "NewGameButton" ) {
					//Make sure to unbind it.
					//Component.onClick.RemoveAllListeners();
					Component.name = Component.GetComponentsInChildren<Text>()[0].text = "Resume";
				}
				if( Component.name == "SaveGameButton" ) {
					Component.interactable = true;
				}
				if( Component.name == "ContinueButton" ) {
					Component.gameObject.SetActive(false);
				}
			}
			if( Component.name == "ContinueButton" ) {
				if( !SaveManager.HasAutoSave() ) {
					Component.interactable = false;
				}
			}
			Debug.Log("Adding " + Component.name);
			Component.onClick.AddListener(() => { OnButtonClick(Component.name);});
		}
	}
	override public void Start() {
		if( GameHandler.Instance != null && GameHandler.Instance.IsPaused ) {
			OpenNewGamePanel();
			return;
		}
		base.Start();
		//Set app parameters.
		//TODO:Set also quality?
		if( Application.platform != RuntimePlatform.Android ) {
			bool InFullScreen = PlayerPrefs.GetInt("Screenmanager Is Fullscreen mode",1) == 1 ? true : false;
			Screen.fullScreen = InFullScreen;
			if( !Screen.fullScreen ) {
				Screen.fullScreenMode = FullScreenMode.Windowed;
			}
		}
	}
}
