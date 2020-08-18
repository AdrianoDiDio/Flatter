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
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameHandler : MonoBehaviour {
	public enum PlayerSkill {
		Easy = 1,
		Medium,
		Hard
	}
	public static GameHandler Instance = null;
	private Player MainPlayer;
	public List<EnemyAI> Enemies;
	public List<Coin> 	  Coins;
	public CameraController MainCamera;
	public bool IsPaused;
	public bool InMiniGame;
	private bool WasPaused;

	// FPS Stuff.
	private Text FPSText;
	private DoorSwitch MiniGameSwitch;
	private GameObject AndroidPanel;
	private bool UpdateAndroidPanel;
	public PlayerSkill Skill;
	[ SerializeField ]
	private Hud HudPanel;
	private bool StartingFromSaveGame;
	public SerializableEntity SavedPlayerTransform;
	public bool LoadedFromSaveGame;

	public void ClearCoinsArray() {
		foreach( Coin coinItem in Coins) {
			if( coinItem.gameObject != null) {
				Destroy(coinItem.gameObject);
			}
		}
		Coins.Clear();
	}
	/*
		Static function that helps initializing this object without waiting for the scene
		that contains it to be called!

	 */
	 public static void InstanceGameHandler(PlayerSkill Skill = PlayerSkill.Easy) {
		 GameObject Temp = new GameObject("GameHandler");
		 GameHandler CurrentInstance = Temp.AddComponent<GameHandler>();
		 Debug.Log("Setting skill to " + Skill);
		 CurrentInstance.Skill = Skill;
		 CurrentInstance.Init();
	 }
	 /*
	 	Static function that helps initializing this object without waiting for the scene
		that contains it to be called!
		It accept a savegame to be read in order to instantiate it correctly.
	  */
	public static void InstanceGameHandler(SaveManager.SaveGame SaveGame) {
		GameObject Temp = new GameObject("GameHandler");
		Temp.AddComponent<GameHandler>();
		GameHandler.Instance.Init(SaveGame);
		 /*
		Result.PlayerScore = GameHandler.Instance.GetPlayer().GetScore();
		Result.PlayerPosition = new SerializableVector3(GameHandler.Instance.GetPlayer().transform.position);
		Result.EnemyPosition = new SerializableVector3[GameHandler.Instance.Enemies.Count];
		Index = 0;
		foreach( EnemyAI Enemy in GameHandler.Instance.Enemies ) {
			Result.EnemyPosition[Index] = new SerializableVector3(Enemy.transform.position);

		}
		Result.ItemPosition = new SerializableVector3[GameHandler.Instance.Coins.Count];
		Index = 0;
		foreach( Coin CoinPosition in GameHandler.Instance.Coins ) {
			Result.ItemPosition[Index] = new SerializableVector3(CoinPosition.transform.position);

		}
		Result.SceneName = SceneManager.GetActiveScene().name;
		*/
	 }
	public void PickedCoin(string Name) {
		Debug.Log("Picked coin: " + Name);
		Coin ToRemove = Coins.Find(Item => Item.name == Name);
		if( ToRemove == null ) {
			return;
		}
		Debug.Log("Found.");
		Destroy(ToRemove.gameObject);
		Coins.Remove(ToRemove);
	}
	public void CleanUp() {
		if( Time.timeScale == 0 ) {
			//Reset it.
			Time.timeScale = 1;
		}
		MainPlayer.CleanUp();
		HudPanel.CleanUp();
		Destroy(GameHandler.Instance.gameObject);
		Destroy(GameObject.Find("Main Camera"));
		//Reset the singleton.
		SceneManager.sceneLoaded -= OnSceneLoaded;
		GameHandler.Instance = null;
	}

	/*
		Reload the button position inside the android panel!
	 */
	public void MarkAndroidPanelDirty() {
		UpdateAndroidPanel = true;
	}

	public void MiniGameNotifyWin(DoorSwitch.DoorSwitchMiniGame MiniGame) {
		MiniGameSwitch.OnWin(MiniGame);
		InMiniGame = false;
	}

	public void MiniGameNotifyExit(DoorSwitch.DoorSwitchMiniGame MiniGame) {
		MiniGameSwitch.OnExit(MiniGame);
		InMiniGame = false;
	}

	public void RegisterMiniGameListener(DoorSwitch Switch) {
		if( InMiniGame ) {
			return;
		}
		MiniGameSwitch = Switch;
		InMiniGame = true;
	}
	public Player GetPlayer() {
		return MainPlayer;
	}

	public void OnGameOver() {
		//HudPanel.gameObject.SetActive(false);
		HudPanel.Deactivate();
		MainPlayer.gameObject.SetActive(false);
		SceneManager.LoadScene("GameOver");
	}
	/*
		@Adriano:
		Toggle utility function to go in Pause mode. 
		Pause/Unpause the game depending on IsPaused value.
		It takes care of stopping/playing animation,saving/loading rigidbody variables and
		also loading the UI if TogglePauseMenu is true.
	 */
	public void TogglePause(bool TogglePauseMenu = true) {
		if( IsPaused ) {
			//Unpause.
			Debug.Log("Unpausing.");
			IsPaused = false;
			Time.timeScale = 1f;
			MainPlayer.FreezePlayer(false);
			if( TogglePauseMenu ) {
				SceneManager.UnloadSceneAsync("UiScene");
			}
			if( Application.platform == RuntimePlatform.Android) {
				AndroidPanel.SetActive(true);
				if( UpdateAndroidPanel ) {
					InputManager.Instance.SetAndroidButtonPosition(AndroidPanel);
					UpdateAndroidPanel = false;
				}
			}
			HudPanel.ForceSceneChange();
			//WasPaused = true;
			InitAudioSettings();
			return;
		} else {
			Debug.Log("Pausing");
			IsPaused = true;
			//WasPaused = false;
			//Stops all time related gameobjects (Moving platform,Sawtrap etc...).
			Time.timeScale = 0f;
			MainPlayer.FreezePlayer(true);
			if( TogglePauseMenu ) {
				SceneManager.LoadScene("UiScene",LoadSceneMode.Additive);
			}
			if( Application.platform == RuntimePlatform.Android ) {
				AndroidPanel.SetActive(false);
			}
			return;
		}
	}
	private void CheckEscapeKey() {
		bool EscapeDown = Input.GetKeyDown("escape");
		if( EscapeDown && !InMiniGame ) {
			TogglePause();
		}

	}

	private GameObject InstantiateCoinBySpawn(GameObject Spawn) {
		GameObject ResultCoin = null;
		switch( Spawn.tag ) {
			case "GoldCoinSpawn":
				ResultCoin = Instantiate(Resources.Load("Prefabs/GoldCoin"),Spawn.transform.position,Spawn.transform.rotation)
								 as GameObject;
				break;
			case "SilverCoinSpawn":
				ResultCoin = Instantiate(Resources.Load("Prefabs/SilverCoin"),Spawn.transform.position,Spawn.transform.rotation)
								 as GameObject;
				break;
			case "CopperCoinSpawn":
				ResultCoin = Instantiate(Resources.Load("Prefabs/CopperCoin"),Spawn.transform.position,Spawn.transform.rotation)
								 as GameObject;
				break;
			default:
				Debug.Log("InstantiateCoinByTag:Unknown tag " + Spawn.tag);
				break;
		}
		if( ResultCoin != null ) {
			ResultCoin.transform.parent = Spawn.transform.parent;
		}
		return ResultCoin;
	}
	private void SpawnPlayer() {
		GameObject PlayerSpawn = GameObject.Find("PlayerSpawn");
		Debug.Log("SpawnPlayer");
		if( PlayerSpawn == null ) {
			Debug.Log("SpawnPlayer:Player spawn not found.");
			return;
		}
		//Dont bother spawning if in minigame or
		//we are entering/exiting pause menu.
		Debug.Log("InMinigame||WasPaused||IsPaused: " + InMiniGame + " " + WasPaused + " " + IsPaused);
		if( InMiniGame /*|| WasPaused*/ || IsPaused ) {
			return;
		}
		Debug.Log("Ok");
		//Player exists but needs to be moved to the next scene at the correct position.
		MainPlayer.gameObject.transform.position = PlayerSpawn.transform.position;
	}

	private void SpawnEnemies() {
		GameObject[] SpawnArray = GameObject.FindGameObjectsWithTag("EnemySpawn");
		if( Enemies == null ) {
			Enemies = new List<EnemyAI>();
		}
		Enemies.Clear();
		foreach( GameObject Spawn in SpawnArray ) {
			GameObject Brutus = Instantiate(Resources.Load("Prefabs/Brutus"),Spawn.transform.position,Spawn.transform.rotation) 
												as GameObject;
			//Brutus.transform.localScale = Spawn.transform.localScale;
			Enemies.Add(Brutus.GetComponent<EnemyAI>());
		}
	}
	private void SpawnItems() {
		List<GameObject> ItemSpawnArray;
		int i;

		ItemSpawnArray = new List<GameObject>(GameObject.FindGameObjectsWithTag("GoldCoinSpawn"));
		ItemSpawnArray.AddRange(GameObject.FindGameObjectsWithTag("SilverCoinSpawn"));
		ItemSpawnArray.AddRange(GameObject.FindGameObjectsWithTag("CopperCoinSpawn"));

		if( Coins == null ) {
			Coins = new List<Coin>(); 
		}
		ClearCoinsArray();
		i = 0;
		foreach(GameObject Spawn in ItemSpawnArray ) {
			GameObject Coin = InstantiateCoinBySpawn(Spawn);
			Coin.name = Coin.name + i.ToString();
			Coin Temp = Coin.GetComponent<Coin>();
			Coins.Add(Coin.GetComponent<Coin>());
			i++;
		}
	}

	private void SetDoors() {
		GameObject[] Doors = GameObject.FindGameObjectsWithTag("Door");
		foreach( GameObject Door in Doors ) {
			Debug.Log("Setting door state: " + Door.name);
			Door.GetComponent<DoorController>().Init();
		}
	}
	private void InitScene() {
		SpawnPlayer();
		SpawnEnemies();
		SpawnItems();
		SetDoors();
	}

	//TODO:Iterate over gameobject with tag Player/Enemy - Spawn and instantiate them!
	void OnSceneLoaded(Scene NScene,LoadSceneMode Mode) {
		if( NScene.name.StartsWith("Level") && Mode != LoadSceneMode.Additive ) {
			Debug.Log("OnSceneLoaded:Init");
			InitScene();
			bool Result = SaveManager.SaveCurrentGame();
			Debug.Log("Game was " + (Result ? "succesfully" : "not") + "saved as AutoSave.bin...");
		}
	}



	private void OnAndroidPointerDown(string Name) {
		Debug.Log("OnAndroidPointerDown: " + Name);
		switch( Name ) {
			case "LeftButton":
				MainPlayer.OnAndroidButtonDown(Player.AndroidButton.BUTTON_LEFT);
				//MainPlayer.XInputOffset = -1;
				break;
			case "RightButton":
				MainPlayer.OnAndroidButtonDown(Player.AndroidButton.BUTTON_RIGHT);
				//MainPlayer.XInputOffset = 1;
				break;
			case "UpButton":
				MainPlayer.OnAndroidButtonDown(Player.AndroidButton.BUTTON_UP);
				//MainPlayer.UpPressed = true;
				break;
			case "ActionButton":
				MainPlayer.OnAndroidButtonDown(Player.AndroidButton.BUTTON_ACTION);
				//MainPlayer.HasToShoot = true;
				break;
			case "PauseButton":
				if( !InMiniGame ) {
					TogglePause();
				}
				//TogglePause();
				break;
			default:
				Debug.Log("Unknown button in android panel " + Name);
				break;
			}
	}
	private void OnAndroidPointerUp(string Name) {
		Debug.Log("OnAndroidPointerUp: " + Name);
		switch( Name ) {
			case "LeftButton":
			case "RightButton":
				MainPlayer.OnAndroidButtonUp(Player.AndroidButton.BUTTON_LEFT);
				//MainPlayer.XInputOffset = 0;
				break;
			case "UpButton":
				MainPlayer.OnAndroidButtonUp(Player.AndroidButton.BUTTON_UP);
				//MainPlayer.UpPressed = false;
				break;
			case "ActionButton":
				MainPlayer.OnAndroidButtonUp(Player.AndroidButton.BUTTON_ACTION);
				//MainPlayer.HasToShoot = false;
				break;
			default:
				Debug.Log("Unknown button in android panel " + Name);
				break;
			}
	}

	/*
		@Adriano:
		This function let you define a new Pointer event for a button (button).
		It takes the button on which you want to listen for the event,the EventTriggerType (PointerDown/PointerUp) and a
		Callback which takes as parameter the name of the button.
	 */
	private void RegisterButtonPointerEvent(Button button,EventTriggerType PointerEventType,Action<string> Callback) {
			EventTrigger Trigger = button.gameObject.AddComponent<EventTrigger>();
			var OnPointerDown = new EventTrigger.Entry();
			OnPointerDown.eventID = PointerEventType;
			OnPointerDown.callback.AddListener((e) => Callback(button.gameObject.name));
			Trigger.triggers.Add(OnPointerDown);
	}
	private void InitAndroidControlPanel() {
		//1) Enable the Panel (Just to be sure...but should be enabled by default!).
		//1) Load from the prefab!
		//GameObject AndroidPanel = GameObject.Find("/HUD_Panel/Android/AndroidControlPanel").gameObject;
		//GameObject AndroidPanel = Instantiate(Resources.Load("Prefabs/AndroidControlPanel")) as GameObject;
		// Put it into the correct hierarchy position.
		//AndroidPanel.transform.SetParent(GameObject.Find("/HUD_Panel/Android").transform,false);
		AndroidPanel = InputManager.Instance.InstantiateAndroidControlPanel(GameObject.Find("/HUD_Panel/Android"));
		//2) Get the buttons.
		Button[] Buttons = AndroidPanel.GetComponentsInChildren<Button>();
		//3) For each of them add a on click listener.
		foreach( Button button in Buttons ) {
			//Reload position from playerprefs!
			RegisterButtonPointerEvent(button,EventTriggerType.PointerDown,OnAndroidPointerDown);
			RegisterButtonPointerEvent(button,EventTriggerType.PointerUp,OnAndroidPointerUp);
			//button.onClick.AddListener(() => { OnAndroidButtonClick(button.name);});
		}
		//4) Setup Pause button.
		Button PButton = GameObject.Find("/HUD_Panel/Android/PauseButton").GetComponent<Button>();
		RegisterButtonPointerEvent(PButton,EventTriggerType.PointerDown,OnAndroidPointerDown);
	}

	private void InitFPSCounter() {
		FPSText = GameObject.Find("HUD_FPS_Text").GetComponent<Text>();
		// Sets the default value.
		if( !PlayerPrefs.HasKey("ShowFPSCounter") ) {
			PlayerPrefs.SetInt("ShowFPSCounter",0);
		}
	}
	private void InitHUD() {
		Debug.Log("Init HUD.");
		if( HudPanel != null  ) {
			InitFPSCounter();
			if( Application.platform == RuntimePlatform.Android ) {
				InitAndroidControlPanel();
			}
			return;
		}
		GameObject Panel = Instantiate(Resources.Load("Prefabs/HUD_Panel")) as GameObject;
		HudPanel = Panel.GetComponent<Hud>();
		Panel.name = "HUD_Panel";
		if( Application.platform == RuntimePlatform.Android ) {
			//Spawn control panel.
			InitAndroidControlPanel();
		} else {
			Debug.Log("Android control disabled!");
			GameObject.Find("Android").gameObject.SetActive(false);
		}
		InitFPSCounter();
	}

	private void InitAudioSettings() {
		int Value = PlayerPrefs.GetInt("EnableSoundEffects",1);
		if( Value == 1 ) {
			//Audio On.
			AudioListener.volume = 1f;
			AudioListener.pause = false;
		} else {
			//Audio Off.
			AudioListener.volume = 0f;
			AudioListener.pause = true;
		}
	}
	
	public void InitCamera() {
		GameObject Camera = Instantiate(Resources.Load("Prefabs/MainCamera")) as GameObject;
		Camera.name = "Main Camera";
		MainCamera = Camera.GetComponent<CameraController>();
		MainCamera.Player = MainPlayer.gameObject;
	}

	private void InitFromSaveGame(SaveManager.SaveGame SaveGame) {
		LoadedFromSaveGame = true;
		Skill = SaveGame.Skill;
		GetPlayer().AddScore(SaveGame.PlayerScore);
		GetPlayer().gameObject.transform.position = SaveGame.Player.Position.ToVector3();
		GetPlayer().RestoreRotation(SaveGame.Player.Rotation.ToQuaternion());
		GetPlayer().SetHealth(SaveGame.PlayerHealth);
		//
		//GetPlayer().Flip();
		//SavedPlayerTransform = SaveGame.Player;
		StartingFromSaveGame = true;
		
		if( Coins == null ) {
			Coins = new List<Coin>();
		}
		ClearCoinsArray();
		for(int i = 0; i < SaveGame.ItemPosition.Length; i++ ) {
			GameObject CurrentCoin = null;
			Debug.Log("Got type: " + SaveGame.ItemPosition[i].Type);
			Type CoinType = Type.GetType(SaveGame.ItemPosition[i].Type);
			Vector3 CoinPosition = SaveGame.ItemPosition[i].Position.ToVector3();
			if( CoinType == typeof (GoldCoin) ) {
				CurrentCoin =  Instantiate(Resources.Load("Prefabs/GoldCoin"),CoinPosition,Quaternion.identity)
								 as GameObject;
			} else if( CoinType == typeof (SilverCoin) ) {
				CurrentCoin =  Instantiate(Resources.Load("Prefabs/SilverCoin"),CoinPosition,Quaternion.identity)
							as GameObject;
			} else if( CoinType == typeof (CopperCoin) ) {
				CurrentCoin =  Instantiate(Resources.Load("Prefabs/CopperCoin"),CoinPosition,Quaternion.identity)
								 as GameObject;
			} else {
				Debug.Log("Unknown type: " + CoinType.ToString());
				continue;
			}
			CurrentCoin.name = CurrentCoin.name + i;
			Coin TempCoin = CurrentCoin.GetComponent<Coin>();
			Coins.Add(TempCoin);
		}
		if( SaveGame.DoorSwitch != null && SaveGame.DoorSwitch.UnLocked ) {
			GameObject SwitchObject = GameObject.Find(SaveGame.DoorSwitch.Name);
			if( SwitchObject != null ) {
				DoorSwitch Switch = SwitchObject.GetComponent<DoorSwitch>();	
				Switch.UnlockDoor(true);
			}
		}
	}

	public void Init(Nullable<SaveManager.SaveGame> RestoreGame = null) {
		InitHUD();
		//Spawn Main Camera.
		InitAudioSettings();
		Debug.Log("Instancing it.");
		GameObject Temp = Instantiate(Resources.Load("Prefabs/Player")) as GameObject;
		Temp.name.Replace("(Clone)","");
		MainPlayer = Temp.GetComponent<Player>();
		InitCamera();
		InitScene();
		IsPaused = false;
		InMiniGame = false;
		WasPaused = false;
		UpdateAndroidPanel = false;
		if( RestoreGame != null ) {
			InitFromSaveGame(RestoreGame.Value);
		}
		Debug.Log("SavedPlayerTransfrom => Awake => Invalidated it.");
		//MainPlayer = GameObject.Find("Player").GetComponent<Player>();
		//Debug.Log("Gamemanager:Registered for scene change.");
		//GameObject SpawnPoint = GameObject.Find("BrutusSpawn");
		//GameObject Brutus = Instantiate(Resources.Load("Prefabs/Brutus"),SpawnPoint.transform.position,Quaternion.identity) 
		//	as GameObject;
		//Brutus.transform.localScale = SpawnPoint.transform.localScale;
		SceneManager.sceneLoaded += OnSceneLoaded;
		DontDestroyOnLoad(gameObject);
	}

	void Awake () {
		if( Instance == null ){
			Instance = this;
		} else if( Instance != this ) {
			//It Shouldn't happen...
			Debug.Log("Multiple game handler found...destroying");
			Destroy(gameObject);
		}
	}
	void UpdateFPSString() {
		float FPS;
		float Ms;

		if( PlayerPrefs.GetInt("ShowFPSCounter") == 0 ) {
			if( !String.IsNullOrEmpty(FPSText.text) ) {
				FPSText.text = "";
			}
			return;
		} 
		FPS = 1.0f / Time.unscaledDeltaTime;
		Ms = Time.unscaledDeltaTime * 1000.0f;
		FPSText.text = "FPS:" + (int)FPS + " Ms: " + Ms.ToString("0.00");

	/*	float UpdateFPSTime = 10;
		if( FPSFrameDelta > 1f/UpdateFPSTime ) {
			FPS = FPSFrame / FPSFrameDelta;
			FPSDelta = 1000f/FPS;
			Debug.Log("FPS:" + FPSDelta);
			FPSText.text = "FPS:" + (int)FPS + " Ms: " + FPSDelta.ToString("0.00");
			FPSFrame = 0;
			FPSFrameDelta -= 1f/UpdateFPSTime;
		}*/
	}
	void Update () {
		CheckEscapeKey();
		UpdateFPSString();
	}
}
