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

public class VictoryPanel : MonoBehaviour,UiDialog.UiDialogInputInterface,UiDialog.UiInputDialogButtonPress {
	private Text VictoryLabel;
	private Button BackMenuButton;
	private GameObject LeaderboardDialog;

	public void OnEndEdit(InputField InField) {
		if( InField.text.Length == 0 ) {
			return;
		}
		LeaderBoard.Instance.AddRank(InField.text,GameHandler.Instance.GetPlayer().GetScore());
		Destroy(LeaderboardDialog);

	}

	public void OnButtonPress(UiDialog.UiDialogButton ButtonPressed) {
		if( ButtonPressed == UiDialog.UiDialogButton.UI_DIALOG_BUTTON_OK ) {
			InputField InField = LeaderboardDialog.GetComponentInChildren<InputField>();
			if( InField != null ) {
				if( InField.text.Length != 0 ) {
					LeaderBoard.Instance.AddRank(InField.text,GameHandler.Instance.GetPlayer().GetScore());
					Destroy(LeaderboardDialog);
				}
			}
		}
	}

	private IEnumerator AnimateString(string Text) {
		for( int i = 0; i < Text.Length; i++ ) {
			VictoryLabel.text += Text[i];
			yield return new WaitForSeconds(0.1f);
		}
	}

	public void OnBackMenuButtonClick() {
		GameHandler.Instance.CleanUp();
		SceneManager.LoadScene("UiScene");
	}

	private void EvaluateRank() {
		int Score;
		if( GameHandler.Instance == null ) {
			Debug.Log("EvaluateRank:Gamehandler instance is not valid.");
			return;
		}
		Score = GameHandler.Instance.GetPlayer().GetScore();
		if( LeaderBoard.Instance.IsHighScore(Score) ) {
			LeaderboardDialog = UiDialog.ShowInputDialog("New record","Enter your name",this,this);
		}

	}
	void Start () {
		VictoryLabel = GameObject.Find("WinLabel").GetComponent<Text>();
		BackMenuButton = GameObject.Find("MainMenuButton").GetComponent<Button>();
		BackMenuButton.onClick.AddListener(OnBackMenuButtonClick);
		StartCoroutine(AnimateString("YOU WIN!"));
		EvaluateRank();

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
