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

public class NewGamePanel : Menu {
	public GameObject MainMenuPanel;
	public GameObject SaveManagerPanel;
	public GameObject SkillPanel;

	public void SaveGame() {
		//
		gameObject.SetActive(false);
		SaveManagerPanel.SetActive(true);
		SaveManagerPanel.GetComponent<SaveManagerMenu>().Init(true);
	}

	private void LoadGame() {
		gameObject.SetActive(false);
		SaveManagerPanel.SetActive(true);
		SaveManagerPanel.GetComponent<SaveManagerMenu>().Init(false);

	}
	override protected void OnButtonClick(string ButtonName) {
		switch(ButtonName) {
			case "NewGameButton":
				gameObject.SetActive(false);
				SkillPanel.SetActive(true);
				break;
			case "ResumeButton":
				GameHandler.Instance.TogglePause();
				break;
			case "ContinueButton":
				//Load it from autosave.
				SaveManager.LaunchSavedGame(this);
				//GameHandler.SetGameHandlerState();
				break;
			case "LoadGameButton":
				LoadGame();
				break;
			case "SaveGameButton":
				SaveGame();
				break;
			case "NewGameBackButton":
				gameObject.SetActive(false);
				MainMenuPanel.SetActive(true);
				break;
			case "NewGameQuitButton":
				Application.Quit();
				break;
			default:
				Debug.Log("Unhandled button: " + ButtonName);
				break;
		}
	}
	override protected void Init() {
		Button[] PlayButtons = GetComponentsInChildren<Button>();
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
					Component.name = "ResumeButton";
					Component.GetComponentsInChildren<Text>()[0].text = "Resume";
				}
				if( Component.name == "NewGameBackButton" ) {
					Component.name = "NewGameQuitButton";
					Component.GetComponentsInChildren<Text>()[0].text = "Quit";
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
}
