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
using UnityEngine.SceneManagement;

public class SceneUtils {

	public static string GetSceneNameFromScenePath(string Path) {
		int SceneNameStart = Path.LastIndexOf("/",StringComparison.Ordinal) + 1;
		int SceneNameEnd = Path.LastIndexOf(".",StringComparison.Ordinal);
		int SceneNameLength = SceneNameEnd - SceneNameStart;
		return Path.Substring(SceneNameStart,SceneNameLength);
	}

	public static string GetSceneNameByBuildIndex(int BuildIndex) {
		return GetSceneNameFromScenePath(SceneUtility.GetScenePathByBuildIndex(BuildIndex));
	}

	public static string GetNextSceneName() {
		int NextSceneIndex;
		NextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
		if( NextSceneIndex < SceneManager.sceneCountInBuildSettings ) {
			return GetSceneNameByBuildIndex(NextSceneIndex);
		}
		return string.Empty;
	}
	/*
		Load scene async without instancing game handler.
		Useful when moving from one level to another.
	 */
	public static IEnumerator LoadSceneAsync(int SceneIndex) {
		AsyncOperation LoadLevel;
		//In general clean up remaining objects that will be destroyed during scene unload/load.
		GameHandler.Instance.ClearCoinsArray();
		LoadLevel = SceneManager.LoadSceneAsync(SceneIndex);
		while( !LoadLevel.isDone ) {
			yield return null;
		}

	}

	/*
		Load scene async and instance the gamehandler using the current skill.
	 */
	public static IEnumerator LoadSceneAsync(int SceneIndex,GameHandler.PlayerSkill Skill) {
		AsyncOperation LoadLevel = SceneManager.LoadSceneAsync(SceneIndex);
		while( !LoadLevel.isDone ) {
			yield return null;
		}
		GameHandler.InstanceGameHandler(Skill);
	}
	/*
		Load Scene async and instance the game handler restoring the state from a savegame.
	 */
	public static IEnumerator LoadSceneAsync(SaveManager.SaveGame SavedGame) {
		AsyncOperation LoadLevel = SceneManager.LoadSceneAsync(SavedGame.SceneName);
		while( !LoadLevel.isDone ) {
			yield return null;
		}
		GameHandler.InstanceGameHandler(SavedGame);
	}
}
