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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager  {
	[System.Serializable]
	public struct SaveGame {
		public GameHandler.PlayerSkill Skill;
		public int PlayerScore;
		public float PlayerHealth;
		//TODO:Serialize player/enemy rotation.
		//TODO:Serialize Game timer.
		public SerializableDoorSwitch DoorSwitch; 
		public SerializableEntity Player;
		public SerializableEntity[] Enemies;
		public SerializableCoin[] ItemPosition;
		public string SceneName; 
	}
	public static string DefaultSaveFileExtension = ".bin";
	public static string DefaultSaveFolderName = "/SavedGames/";



	public static string GetSaveGameFolder() {
		return Application.persistentDataPath + DefaultSaveFolderName;
	}
	public static bool SaveGameExists(string FileName) {
		return File.Exists(GetSaveGameFolder() + FileName);
	}
	public static string[] GetSaveGameFileList() {
		string[] Result;
		Result = new string[] {};
		if( !Directory.Exists(GetSaveGameFolder()) ) {
			return Result;
		}
		Result = System.IO.Directory.GetFiles(GetSaveGameFolder(),"*" + DefaultSaveFileExtension).Select(f => Path.GetFileName(f)).ToArray();
		return Result;
	}
	public static bool HasAutoSave() {
		return File.Exists(GetSaveGameFolder() + "AutoSave.bin");
	}

	public static void LaunchSavedGame(MonoBehaviour ReferenceScript,string SaveGame = "AutoSave.bin") {
		Nullable<SaveManager.SaveGame> SavedGame = SaveManager.LoadGame(SaveGame);
		if( SavedGame == null ) {
			UiDialog.ShowWarningDialog("Couldn't load " + SaveGame  + "...try to start a new game....");
			return;
		}
		Debug.Log("LaunchSavedGame Loading " + SaveGame);
		if( GameHandler.Instance ) {
			GameHandler.Instance.CleanUp();
			//GameHandler.Instance.CleanUp();
		}
		Debug.Log("Loading scene: " + SavedGame.Value.SceneName);
		//GameObject FantomGameObject = new GameObject();
		//MonoBehaviour Dummy = FantomGameObject.AddComponent<MonoBehaviour>();
		InputManager.Instance.StartCoroutine(SceneUtils.LoadSceneAsync(SavedGame.Value));
		//Debug.Log("GameHandler resuming load: " + GameHandler.Instance.IsPaused);
	}
	public static SaveGame BuildSaveGame() {
		SaveGame Result;
		int Index;
		Result.PlayerScore = GameHandler.Instance.GetPlayer().GetScore();
		Result.PlayerHealth = GameHandler.Instance.GetPlayer().GetHealth();
		Result.Player = new SerializableEntity(GameHandler.Instance.GetPlayer().transform.position,
									GameHandler.Instance.GetPlayer().transform.rotation);
		//Result.PlayerPosition = new SerializableVector3(GameHandler.Instance.GetPlayer().transform.position);
		Result.Enemies = new SerializableEntity[GameHandler.Instance.Enemies.Count];
		//Result.EnemyPosition = new SerializableVector3[GameHandler.Instance.Enemies.Count];
		Result.Skill = GameHandler.Instance.Skill;
		Index = 0;
		foreach( EnemyAI Enemy in GameHandler.Instance.Enemies ) {
			if( Enemy.HasLockedDoor() ) {
				continue;
			}
			Result.Enemies[Index++] = new SerializableEntity(Enemy.transform.position,Enemy.transform.rotation);

		}
		Debug.Log("Saving " + GameHandler.Instance.Coins.Count);
		Result.ItemPosition = new SerializableCoin[GameHandler.Instance.Coins.Count];
		Index = 0;
		foreach( Coin CoinPosition in GameHandler.Instance.Coins ) {
			Result.ItemPosition[Index++] = new SerializableCoin(CoinPosition);
		}
		GameObject SwitchObject = GameObject.FindGameObjectWithTag("DoorSwitch");
		if( SwitchObject != null ) {
			DoorSwitch Switch = SwitchObject.GetComponent<DoorSwitch>();
			Result.DoorSwitch = new SerializableDoorSwitch(Switch);
		} else {
			Result.DoorSwitch = null;
		}

		Result.SceneName = SceneManager.GetActiveScene().name;
		Debug.Log("Saving current scene: " + Result.SceneName);
		return Result;
	}
	public static Nullable<SaveGame> LoadGame(string SaveName = "AutoSave.bin") {
		BinaryFormatter BFormatter = new BinaryFormatter();
		FileStream LoadFile;
		try {
			LoadFile = File.Open(GetSaveGameFolder() + SaveName,FileMode.Open);
			SaveGame Instance = (SaveGame) BFormatter.Deserialize(LoadFile);
			Debug.Log("Loaded from instance...");
			Debug.Log("Instance Items: " + Instance.ItemPosition.Length);
			Debug.Log("Item Checkup...");
			for( int i = 0; i < Instance.ItemPosition.Length; i++ ) {
				Debug.Log("Item Type: " + Instance.ItemPosition[i].Type + " " + Instance.ItemPosition[i].Position.ToString());
			}
			Debug.Log("Enemies: " + Instance.Enemies.Length);
			Debug.Log("Score:" + Instance.PlayerScore);
			Debug.Log("Scene: " + Instance.SceneName);
			LoadFile.Close();
			return Instance;
		} catch (FileNotFoundException Exception) {
			Debug.Log("File was not found " + Exception.Message);
		}
		return null;
	}
	public static bool SaveCurrentGame(string SaveName = "AutoSave.bin") {
		//GameHandler.Instance => Save
		GameHandler SaveInstance = GameHandler.Instance;
		BinaryFormatter BFormatter = new BinaryFormatter();
		string Path = GetSaveGameFolder();
		Directory.CreateDirectory(Path);
		Debug.Log("Created path." + Path);
		FileStream SaveFile = File.Open(Path + SaveName,FileMode.Create);
		if( SaveInstance == null ) {
			UiDialog.ShowWarningDialog("GameHandler not instanced...could not save");
			return false;
		}
		//Shouldn't happen but just in case.
		if( SaveInstance.InMiniGame ) {
			UiDialog.ShowWarningDialog("Game is inside a minigame...couldn't save");
			return false;
		}
		//OK!
		BFormatter.Serialize(SaveFile,BuildSaveGame());
		SaveFile.Flush();
		SaveFile.Close();
		return true;
	}
	//
	public static void FetchSaveGameFiles() {

	}

}
