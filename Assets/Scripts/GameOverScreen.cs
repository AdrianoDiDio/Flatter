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
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameOverScreen : MonoBehaviour {
	public float SecondsToWait;
	private Button MenuButton;
	private Text GameOverLabel;
	private IEnumerator AnimateString(string Text) {
		for( int i = 0; i < Text.Length; i++ ) {
			GameOverLabel.text += Text[i];
			yield return new WaitForSeconds(SecondsToWait);
		}
	}

	private void OnMenuButtonClick() {
		//Reset all the player stuff!
		GameHandler.Instance.CleanUp();
		SceneManager.LoadScene("UiScene");
	}
	void Start () {
		GameOverLabel = GameObject.Find("GameOverLabel").GetComponent<Text>();
		MenuButton = GameObject.Find("MainMenuButton").GetComponent<Button>();
		MenuButton.onClick.AddListener(OnMenuButtonClick);
		StartCoroutine(AnimateString("GAME OVER!"));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
