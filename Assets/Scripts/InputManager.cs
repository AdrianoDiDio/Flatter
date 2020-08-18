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
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour {
	public struct InButton {
		public KeyCode MainButton { get;set; }
		public KeyCode AltButton { get;set;}

		public InButton(KeyCode MainButton,KeyCode AltButton) {
			this.MainButton = MainButton;
			this.AltButton = AltButton;
		}
	}
	public static InputManager Instance = null;
	public InButton LeftButton;
	public InButton RightButton;
	public InButton UpButton;
	public InButton ActionButton;

	private bool IsKeyPressed(InButton InButton,KeyCode InKey) {
		return InButton.MainButton == InKey || InButton.AltButton == InKey;
	}
	public bool HasPressed(InButton Button) {
		//Prevent false click when mixing Game elements with UI.
		if( Button.MainButton == KeyCode.Mouse0 || Button.AltButton == KeyCode.Mouse0 ) {
			if( Input.GetMouseButton(0) ) {
				if( EventSystem.current.IsPointerOverGameObject() ) {
					return false;
				}
			}
		}
		//Unfortunately we cannot use our function (IsKeyPressed) here since we would have to
		//iterate all over the keycode to know whether or not a key is pressed.
		return Input.GetKey(Button.MainButton) || Input.GetKey(Button.AltButton);
	}

	public bool IsKeyBindInUse(KeyCode KCode) {
		return  IsKeyPressed(LeftButton,KCode) || IsKeyPressed(RightButton,KCode) ||
					IsKeyPressed(UpButton,KCode) || IsKeyPressed(ActionButton,KCode);
	}

	public void SaveSettings() {
		PlayerPrefs.SetString("LeftButton",LeftButton.MainButton.ToString());
		PlayerPrefs.SetString("AltLeftButton",LeftButton.AltButton.ToString());

		PlayerPrefs.SetString("RightButton",RightButton.MainButton.ToString());
		PlayerPrefs.SetString("AltRightButton",RightButton.AltButton.ToString());

		PlayerPrefs.SetString("UpButton",UpButton.MainButton.ToString());
		PlayerPrefs.SetString("AltUpButton",UpButton.AltButton.ToString());

		PlayerPrefs.SetString("ActionButton",ActionButton.MainButton.ToString());
		PlayerPrefs.SetString("AltActionButton",ActionButton.AltButton.ToString());

		PlayerPrefs.Save();
	}

	/*
		Given a valid Android Panel It returns an AndroidButtonPosition
		containing all the current position info.
	 */
	public AndroidButtonPosition DumpCurrentAndroidButtonPosition(GameObject AndroidPanel) {
		AndroidButtonPosition ButtonPosition = new AndroidButtonPosition();
		Button[] Entities = AndroidPanel.GetComponentsInChildren<Button>();
		foreach(Button Entity in Entities) {
			Debug.Log("Saving " + Entity.name + " At position: " + Entity.transform.position.ToString());
			Vector3 LocalPos = Entity.GetComponent<RectTransform>().localPosition;
			Vector2 AnchoredPosition = Entity.GetComponent<RectTransform>().anchoredPosition;
			switch( Entity.name ) {
				case "LeftButton":
					ButtonPosition.LeftButton = new SerializableRectTransform(LocalPos,AnchoredPosition);
					break;
				case "RightButton":
					ButtonPosition.RightButton = new SerializableRectTransform(LocalPos,AnchoredPosition);
					break;
				case "UpButton":
					ButtonPosition.UpButton = new SerializableRectTransform(LocalPos,AnchoredPosition);
					break;
				case "ActionButton":
					ButtonPosition.ActionButton = new SerializableRectTransform(LocalPos,AnchoredPosition);
					break;
				default:
					Debug.Log("Unknown button in android panel( " + Entity.name  + ")!");
					break;
			}
		}
		return ButtonPosition;
	}

	/*
		Given a valid Android Panel and an AndroidButtonPositon
		It sets the buttons inside the panel.
	 */
	public void SetAndroidButtonPosition(GameObject AndroidPanel,AndroidButtonPosition ButtonPosition) {
		Button[] Entities = AndroidPanel.GetComponentsInChildren<Button>();

		foreach(Button Entity in Entities) {
			RectTransform EntityRect = Entity.GetComponent<RectTransform>();
			switch( Entity.name ) {
				case "LeftButton":
					EntityRect.anchoredPosition = ButtonPosition.LeftButton.AnchoredPosition.ToVector3();
					EntityRect.localPosition = ButtonPosition.LeftButton.LocalPosition.ToVector3();
					break;
				case "RightButton":
					EntityRect.anchoredPosition = ButtonPosition.RightButton.AnchoredPosition.ToVector3();
					EntityRect.localPosition = ButtonPosition.RightButton.LocalPosition.ToVector3();
					break;
				case "UpButton":
					EntityRect.anchoredPosition = ButtonPosition.UpButton.AnchoredPosition.ToVector3();
					EntityRect.localPosition = ButtonPosition.UpButton.LocalPosition.ToVector3();
					break;
				case "ActionButton":
					EntityRect.anchoredPosition = ButtonPosition.ActionButton.AnchoredPosition.ToVector3();
					EntityRect.localPosition = ButtonPosition.ActionButton.LocalPosition.ToVector3();
					break;
				default:
					Debug.Log("Unknown button in android panel( " + Entity.name  + ")!");
					break;
			}
		}
		if( GameHandler.Instance && GameHandler.Instance.IsPaused ) {
			Debug.Log("Updating android panel");
			 GameHandler.Instance.MarkAndroidPanelDirty();
		}
		return;
	}
	/*
		Set the android button position by loading the configuration either from the file
		or defaulting to the one of the prefab.
		At the ends it calls SetAndroidButtonPosition that does the actual job of moving things
		in the panel!
	 */
	public void SetAndroidButtonPosition(GameObject AndroidPanel) {
		AndroidButtonPosition ButtonPosition = AndroidButtonPosition.Load();
		if( ButtonPosition == null ) {
			Debug.Log("Load Failed dumping now");
			//Init
			ButtonPosition = DumpCurrentAndroidButtonPosition(AndroidPanel);
			ButtonPosition.Save();
		} else {
			Debug.Log("Setting button position");
			SetAndroidButtonPosition(AndroidPanel,ButtonPosition);
		}
	}

	public GameObject InstantiateAndroidControlPanel(GameObject Parent) {
		//If not found spawn the default prefab!
		GameObject AndroidPanel = Instantiate(Resources.Load("Prefabs/AndroidControlPanel")) as GameObject;
		AndroidPanel.transform.SetParent(Parent.transform,false);
		SetAndroidButtonPosition(AndroidPanel);
		return AndroidPanel;

	}

	public void WriteDefaults() {
		if( Application.platform == RuntimePlatform.Android ) {
			GameObject DefaultAndroidPanel = Instantiate(Resources.Load("Prefabs/AndroidControlPanel")) as GameObject;
			AndroidButtonPosition DefaultButtonPosition = DumpCurrentAndroidButtonPosition(DefaultAndroidPanel);
			DefaultButtonPosition.Save();
			Destroy(DefaultAndroidPanel);
		} else {
			PlayerPrefs.SetString("LeftButton","A");
			PlayerPrefs.SetString("AltLeftButton","LeftArrow");
			PlayerPrefs.SetString("RightButton","D");
			PlayerPrefs.SetString("AltRightButton","RightArrow");
			PlayerPrefs.SetString("UpButton","Space");
			PlayerPrefs.SetString("AltUpButton","UpArrow");
			PlayerPrefs.SetString("ActionButton","LeftControl");
			PlayerPrefs.SetString("AltActionButton","RightControl");
			Init();
		}
	}
	/*
		When initializing we don't know if player has saved preferences yet.
		This means that GetString("XButton") may not exists yet thus returning the default one.
		PlayerPrefs will be saved once the user manually save them on the settings menu.
	 */
	private void Init() {
		KeyCode LocalLeftButton = (KeyCode) System.Enum.Parse(typeof(KeyCode),PlayerPrefs.GetString("LeftButton","A"));
		KeyCode LocalAltLeftButton = (KeyCode) System.Enum.Parse(typeof(KeyCode),PlayerPrefs.GetString("AltLeftButton","LeftArrow"));
		LeftButton = new InButton(LocalLeftButton,LocalAltLeftButton);
		KeyCode LocalRightButton = (KeyCode) System.Enum.Parse(typeof(KeyCode),PlayerPrefs.GetString("RightButton","D"));
		KeyCode LocalAltRightButton = (KeyCode) System.Enum.Parse(typeof(KeyCode),PlayerPrefs.GetString("AltRightButton","RightArrow"));
		RightButton = new InButton(LocalRightButton,LocalAltRightButton);
		KeyCode LocalUpButton = (KeyCode) System.Enum.Parse(typeof(KeyCode),PlayerPrefs.GetString("UpButton","Space"));
		KeyCode LocalAltUpButton = (KeyCode) System.Enum.Parse(typeof(KeyCode),PlayerPrefs.GetString("AltUpButton","UpArrow"));
		UpButton = new InButton(LocalUpButton,LocalAltUpButton);
		KeyCode LocalActionButton = (KeyCode) System.Enum.Parse(typeof(KeyCode),PlayerPrefs.GetString("ActionButton","LeftControl"));
		KeyCode LocalAltActionButton = (KeyCode) System.Enum.Parse(typeof(KeyCode),PlayerPrefs.GetString("AltActionButton","RightControl"));
		ActionButton = new InButton(LocalActionButton,LocalAltActionButton);
	}
	void Start () {
		if( Instance == null ) {
			Instance = this;
		} else if( Instance != this ) {
			Destroy(gameObject);
		}
		Init();
		DontDestroyOnLoad(gameObject);
	}
}
