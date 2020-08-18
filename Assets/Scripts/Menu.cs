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

public abstract class Menu : MonoBehaviour {
	protected List<Button> Buttons;
	protected virtual void OnButtonClick(string ButtonName) {
		Debug.Log("Clicked: " + ButtonName);
	}

	protected virtual void OnToggleCheck(string Toggle) {
		Debug.Log("Toggle: " + Toggle);
	}
	/*
		Returns a toggle initialized with the value found in the playerprefs that
		can be listened for changes using the OnToggleCheck function.
	 */
	protected Toggle InitToggle(string CheckboxName,string PreferenceName,int DefaultPreferenceValue = 1) {
		Toggle Temp = GameObject.Find(CheckboxName).GetComponent<Toggle>();
		Temp.isOn = PlayerPrefs.GetInt(PreferenceName,1) == 1 ? true : false;
		Temp.onValueChanged.AddListener(delegate {OnToggleCheck(Temp.name);});
		return Temp;
	}
	protected virtual void Init() {
		//Get all the buttons including the one not active...
		Buttons = new List<Button>(this.GetComponentsInChildren<Button>(true));
		foreach( Button Component in Buttons ) {
			Debug.Log("Component name: "+ Component.name);
			if( Component.tag == "SkipMenuHandler" ) {
				continue;
			}
			if(GameHandler.Instance != null ) {
				Debug.Log("GameHandler Is Null + Is Paused" + GameHandler.Instance == null +  GameHandler.Instance.IsPaused);
			}
			Debug.Log("Adding " + Component.name);
			Component.onClick.AddListener(() => { OnButtonClick(Component.name);});
		}
	}

	public virtual void Start() {
		Init();
	}

}
