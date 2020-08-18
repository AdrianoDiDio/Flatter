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
using UnityEngine.EventSystems;

public class ControlMenu : Menu, IDropHandler {
	public GameObject PcControlPanel;
	private GameObject AndroidPanel;
	private Button	  ResetButton;
	private bool EditMode;
	private string PreviousBindTextButton;

	public void OnDrop(PointerEventData EventData) {
		Debug.Log("Dropping");

		Debug.Log("Saved");
	}

	/*
		Enable/Disable all the buttons except for the one with name ButtonName.
	 */
	private void SetOtherButtonsState(string ButtonName,bool Active) {
		GameObject[] TaggedButtons = GameObject.FindGameObjectsWithTag("BindButton");
		foreach( GameObject Entity in TaggedButtons ) {
			if( Entity.name == ButtonName ) {
				continue;
			}
			Entity.GetComponent<Button>().interactable = Active;
		}
	}
	private void BindNewKey(string ButtonName,KeyCode NewKeyCode) {
		if( ButtonName == GetButtonNameByFunction("Left") ) {
			InputManager.Instance.LeftButton.MainButton = NewKeyCode;

		}
		if( ButtonName == GetButtonNameByFunction("Left",false)) {
			InputManager.Instance.LeftButton.AltButton = NewKeyCode;
		}
		if( ButtonName == GetButtonNameByFunction("Right") ) {
			InputManager.Instance.RightButton.MainButton = NewKeyCode;

		}
		if( ButtonName == GetButtonNameByFunction("Right",false)) {
			InputManager.Instance.RightButton.AltButton = NewKeyCode;
		}
		if( ButtonName == GetButtonNameByFunction("Up") ) {
			InputManager.Instance.UpButton.MainButton = NewKeyCode;

		}
		if( ButtonName == GetButtonNameByFunction("Up",false)) {
			InputManager.Instance.UpButton.AltButton = NewKeyCode;
		}
		if( ButtonName == GetButtonNameByFunction("Action") ) {
			InputManager.Instance.ActionButton.MainButton = NewKeyCode;

		}
		if( ButtonName == GetButtonNameByFunction("Action",false)) {
			InputManager.Instance.ActionButton.AltButton = NewKeyCode;
		}
	}
	
	private IEnumerator GetNewBinding(string ButtonName) {
		Button Element = Buttons.Find(x => x.name == ButtonName);
		Element.GetComponentInChildren<Text>().text = "Press...";
		EditMode = true;
		while( /*!Input.anyKey || (Input.GetMouseButton(0) && EventSystem.current.IsPointerOverGameObject()) */ true ) {
			if( Input.anyKeyDown && !Input.GetMouseButton(0) ) {
				break;
			}
			yield return null;
		}
		Debug.Log("Got something...");
		foreach( KeyCode kCode in Enum.GetValues(typeof(KeyCode))) {
			if( Input.GetKeyDown(kCode) ) {
				if( !InputManager.Instance.IsKeyBindInUse(kCode) ) {
					Debug.Log("Binding: " + kCode.ToString());
					Element.GetComponentInChildren<Text>().text = kCode.ToString();
					BindNewKey(ButtonName,kCode);
					InputManager.Instance.SaveSettings();
					Debug.Log("Bind");
				} else {
					Debug.Log("Key Already in Use");
					Debug.Log("Bind Failed");
					UiDialog.ShowWarningDialog("Key already in use.");
					Element.GetComponentInChildren<Text>().text = PreviousBindTextButton;
				}
				break;
			}
		}
		SetOtherButtonsState(ButtonName,true);
		EditMode = false;
	}
	override protected void OnButtonClick(string ButtonName) {
		//Disable all the other buttons
		if( EditMode ) {
			Debug.Log("Preventing");
			return;
		}
		Debug.Log("Clicking " + ButtonName);
		PreviousBindTextButton = GameObject.Find(ButtonName).GetComponentInChildren<Text>().text;
		SetOtherButtonsState(ButtonName,false);
		StartCoroutine(GetNewBinding(ButtonName));
	}

	private void OnResetButtonClick() {
		Debug.Log("Reset");
		InputManager.Instance.WriteDefaults();
		if( AndroidPanel != null ) {
			Destroy(AndroidPanel);
		}
		Init();
	}
	/*
		Returns a new string containing Main/Alt+Function+Button
	 */
	private string GetButtonNameByFunction(string Function,bool IsMain = true) {
		string EndResult;
		EndResult = Function + "Button";
		if( IsMain ) {
			return "Main" + EndResult;
		}
		return "Alt" + EndResult;
	}
	private Button SetupButton(string ButtonName,KeyCode ButtonKey) {
		Debug.Log("Setup: Finding " + ButtonName);
		Button Result = GameObject.Find(ButtonName).GetComponent<Button>();
		Result.GetComponentInChildren<Text>().text = ButtonKey.ToString();
		Result.onClick.AddListener(() => { OnButtonClick(Result.name);});
		return Result;
	}
	private void InitButtons(string Name) {
		switch( Name ) {
			case "Left":
				Buttons.Add(SetupButton(GetButtonNameByFunction(Name),InputManager.Instance.LeftButton.MainButton));
				Buttons.Add(SetupButton(GetButtonNameByFunction(Name,false),InputManager.Instance.LeftButton.AltButton));
				break;
			case "Right":
				Buttons.Add(SetupButton(GetButtonNameByFunction(Name),InputManager.Instance.RightButton.MainButton));
				Buttons.Add(SetupButton(GetButtonNameByFunction(Name,false),InputManager.Instance.RightButton.AltButton));
				break;
			case "Jump":
				Buttons.Add(SetupButton(GetButtonNameByFunction(Name),InputManager.Instance.UpButton.MainButton));
				Buttons.Add(SetupButton(GetButtonNameByFunction(Name,false),InputManager.Instance.UpButton.AltButton));
				break;
			case "Action":
				Buttons.Add(SetupButton(GetButtonNameByFunction(Name),InputManager.Instance.ActionButton.MainButton));
				Buttons.Add(SetupButton(GetButtonNameByFunction(Name,false),InputManager.Instance.ActionButton.AltButton));
				break;
			default:
				Debug.Log("Unknown key binding " + Name);
				break;
		}
	}

	 private void InitPcControls() {
		Text[] Labels;
		Buttons = new List<Button>();
		PcControlPanel.SetActive(true);
		Labels = GameObject.FindObjectsOfType<Text>();
		foreach (Text Label in Labels ) {
			if( Label.name == "LeftButtonLabel" ) {
				InitButtons("Left");
			}
			if( Label.name == "RightButtonLabel" ) {
				InitButtons("Right");
			}
			if( Label.name == "JumpButtonLabel" ) {
				InitButtons("Jump");
			}
			if( Label.name == "ActionButtonLabel" ) {
				InitButtons("Action");
			}
		}
		EditMode = false;
	}
	private void InitAndroidControls() {
		int LastSiblingIndex;
		RectTransform BackButton = GameObject.Find("ControlSettingsBackButton").GetComponent<RectTransform>();
		AndroidPanel = InputManager.Instance.InstantiateAndroidControlPanel(gameObject);
		RectTransform AndroidRectTransform = AndroidPanel.GetComponent<RectTransform>();
		LastSiblingIndex = AndroidPanel.GetComponent<RectTransform>().GetSiblingIndex();
		AndroidRectTransform.SetSiblingIndex(BackButton.GetSiblingIndex());
		BackButton.SetSiblingIndex(LastSiblingIndex);
		Button[] AndroidButtons = AndroidPanel.GetComponentsInChildren<Button>();
		foreach( Button Entity in AndroidButtons ) {
			DraggableAndroidButton DraggableButton = Entity.gameObject.AddComponent<DraggableAndroidButton>();
			DraggableButton.AndroidPanel = AndroidPanel;
			Outline BOutline = Entity.gameObject.AddComponent<Outline>();
			BOutline.effectDistance = new Vector2(5,5);
			CanvasGroup CGroup = Entity.gameObject.AddComponent<CanvasGroup>();
			
		}
		
	}
	override protected void Init() {
		ResetButton = GameObject.Find("ControlSettingsResetButton").GetComponent<Button>();
		ResetButton.onClick.AddListener(delegate { OnResetButtonClick();});
		if( Application.platform == RuntimePlatform.Android ) {
			InitAndroidControls();
		} else {
			InitPcControls();
		}
	}
}
