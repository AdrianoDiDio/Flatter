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
using System.Linq;

public class SettingsMenu : Menu {
	public GameObject GeneralSettingsPanel;
	public GameObject VideoSettingsPanel;
	private Button BackButton;
	private Toggle ShowFPSToggle; 
	private Toggle SoundEffectsToggle;
	private Toggle FullScreenToggle;
	private Dropdown ResolutionList;
	private Button QualityChange;
	private Resolution SafeResolution;

	protected override void OnButtonClick(string ButtonName) {
		switch( ButtonName ) {
			case "GeneralSettingsButton":
				InitGeneralSettings();
				break;
			case "VideoSettingsButton":
				InitVideoSettings();
				break;
			case "BackButton":
				break;
			case "QualityChangeButton":
				UpdateQualityLevel();
				break;
			default:
				break;
		}
	}
	private void CloseMenu() {
			// Go back to main.
			GeneralSettingsPanel.SetActive(false);
			gameObject.SetActive(true);
	}
	override protected void OnToggleCheck(string Option) {
		switch( Option ) {
			case "FPSCheckbox" :
				PlayerPrefs.SetInt("ShowFPSCounter",ShowFPSToggle.isOn ? 1 : 0);
				PlayerPrefs.Save();
				break;
			case "SoundEffectsCheckbox":
				PlayerPrefs.SetInt("EnableSoundEffects",SoundEffectsToggle.isOn ? 1 : 0);
				PlayerPrefs.Save();
				break;
			case "FullScreenCheckbox":
				PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode",FullScreenToggle.isOn ? 1 : 0);

				if( FullScreenToggle.isOn ) {
					Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
				} else {
					Screen.fullScreenMode = FullScreenMode.Windowed;
				}
				Screen.fullScreen = FullScreenToggle.isOn;
				PlayerPrefs.Save();
				break;
			default:
				Debug.Log("Unimplemented option " + Option);
				break;
		}
	}
	private bool StillInvalid = false;

	private void SaveResolutionToPlayerPrefs(Resolution Res) {
		PlayerPrefs.SetInt("Screenmanager Resolution Width",Res.width);
		PlayerPrefs.SetInt("Screenmanager Resolution Height",Res.height);
		PlayerPrefs.Save();
	}
	private IEnumerator IsResolutionValid(GameObject Dialog) { 
		yield return new WaitForSeconds(5);
		Debug.Log("Elapsed...");		
		//If 5 seconds elapsed without user confirming the change switch to the safe one...
		Screen.SetResolution(SafeResolution.width,SafeResolution.height,Screen.fullScreen,SafeResolution.refreshRate);
		//Unbind the event listener in order to just update the UI and not triggering another resolution change.
		ResolutionList.onValueChanged.RemoveAllListeners();
		ResolutionList.value = GetCurrentResolutionIndexByResolution(SafeResolution);
		SaveResolutionToPlayerPrefs(SafeResolution);
		//Make sure we rebind it in order to work properly.
		BindResolutionListValueChanged();
		Destroy(Dialog);
		
	}
	private void OnResolutionSelected() {
		Coroutine ConfirmResolution;
		Resolution SelectedRes;
		GameObject Dialog;
		Debug.Log("Selected resolution: " + ResolutionList.value);
		Debug.Log("This means that our index is: " + Screen.resolutions[ResolutionList.value]);
		Debug.Log("ScreenRes: " + Screen.currentResolution.width + "x" + Screen.currentResolution.height + "@" + Screen.currentResolution.refreshRate);
		Debug.Log("Screen Width/Height: " + Screen.width + "x" + Screen.height + "@");

		Dialog = null;
		SelectedRes = Screen.resolutions[ResolutionList.value];

		if( SelectedRes.width == Screen.width && SelectedRes.height == Screen.height) {
				return;
		}

		ConfirmResolution = null;
		Screen.SetResolution(SelectedRes.width,SelectedRes.height,Screen.fullScreen,SelectedRes.refreshRate);
		Dialog = UiDialog.ShowWarningDialog("The resolution will reset if ok is not pressed in 5 seconds",
				"Confirm resolution change:",delegate {
					VideoSettingsPanel.GetComponent<Image>().StopCoroutine(ConfirmResolution);
					SafeResolution = SelectedRes;
					//Save it to playerprefs.
					SaveResolutionToPlayerPrefs(SelectedRes);
					Destroy(Dialog);
				});
		Debug.Log("Started coroutine.");
		ConfirmResolution = VideoSettingsPanel.GetComponent<Image>().StartCoroutine(IsResolutionValid(Dialog));
	}

	private void OnBackButtonClick() {
		CloseMenu();
	}
	
	void Update() {
		if( Input.GetKeyDown("escape") ) {
			CloseMenu();
		}
	}

	private void UpdateQualityLevel() {
		int CurrentQuality = QualitySettings.GetQualityLevel();
		CurrentQuality = (CurrentQuality + 1) % QualitySettings.names.Length;
		QualityChange.GetComponentInChildren<Text>().text = QualitySettings.names[CurrentQuality];
		QualitySettings.SetQualityLevel(CurrentQuality);
	}

	private Resolution BuildResolutionWithDefaultRefreshRate(int Width,int Height) {
		Resolution Result;
		Result = new Resolution();
		Result.width = Width;
		Result.height = Height;
		Result.refreshRate = Screen.currentResolution.refreshRate; //
		return Result;
	}
	private Resolution GetCurrentResolution() {
		return BuildResolutionWithDefaultRefreshRate(Screen.width,Screen.height);
	}
	private int GetCurrentResolutionIndexByResolution(Resolution InRes) {
		return Screen.resolutions.ToList().IndexOf(InRes);
	}
	private int GetCurrentResolutionIndex() {
		return GetCurrentResolutionIndexByResolution(GetCurrentResolution());
	}
	private void BindResolutionListValueChanged() {
		ResolutionList.onValueChanged.AddListener( delegate { OnResolutionSelected();});
	}
	private void InitVideoSettings() {
		Debug.Log("**InitVideoSettings**");
		gameObject.SetActive(false);
		VideoSettingsPanel.gameObject.SetActive(true);
		QualityChange = GameObject.Find("QualityChangeButton").GetComponent<Button>();
		ResolutionList = GameObject.Find("ResolutionDropdown").GetComponent<Dropdown>();
		QualityChange.GetComponentInChildren<Text>().text = QualitySettings.names[QualitySettings.GetQualityLevel()];
		QualityChange.onClick.AddListener(delegate { OnButtonClick(QualityChange.name);});
		if( Application.platform == RuntimePlatform.Android ) {
			if( QualitySettings.names.Length == 0 ) {
				QualityChange.interactable = false;
			}
			GameObject.Find("Option_Fullscreen").SetActive(false);
			GameObject.Find("Option_Resolution").SetActive(false);
			return;
		} else {
			//PC
			if( Screen.resolutions.Length == 0 ) {
				GameObject.Find("Option_Resolution").SetActive(false);
				return;
			}
			/*AvailableResolution = Screen.resolutions;
			if( AvailableResolution.Length == 0 ) {
				GameObject.Find("Option_Resolution").SetActive(false);
				return;
				//DEBUG CODE
				AvailableResolution = new Resolution[2];
				AvailableResolution[0] = new Resolution();
				AvailableResolution[0].width = 640;
				AvailableResolution[0].height = 480;
				AvailableResolution[0].refreshRate = 60;
				AvailableResolution[1] = Screen.currentResolution;
				*/
			List<string> Resolutions = new List<string>();
			ResolutionList.ClearOptions();
			int Index = 0;
			int CurrentResIndex = -1;

			/*Screen.resolutions*/Screen.resolutions.ToList().ForEach(Res => Resolutions.Add(Res.ToString()));
			CurrentResIndex = GetCurrentResolutionIndex();
			SafeResolution = Screen.resolutions[CurrentResIndex];
			/*foreach( Resolution Res in AvailableResolution ) {
				if( Screen.currentResolution.width == Res.width &&
					Screen.currentResolution.height == Res.height &&
					Screen.currentResolution.refreshRate == Res.refreshRate ) {
						Debug.Log("Res matched...");
						CurrentResIndex = Index;
				}
				Debug.Log("Adding: " + Res.width + "x" + Res.height + " @" + Res.refreshRate);
				Resolutions.Add(Res.width + "x" + Res.height + " @" + Res.refreshRate);
				Index++;
			}*/
			ResolutionList.AddOptions(Resolutions);
			if( CurrentResIndex != -1 ) {
				ResolutionList.value = CurrentResIndex;
			}
			BindResolutionListValueChanged();
		}
		FullScreenToggle = InitToggle("FullScreenCheckbox","Screenmanager Is Fullscreen mode");
	}
	private void InitGeneralSettings() {
		gameObject.SetActive(false);
		GeneralSettingsPanel.gameObject.SetActive(true);
		ShowFPSToggle = InitToggle("FPSCheckbox","ShowFPSCounter",0);
		SoundEffectsToggle = InitToggle("SoundEffectsCheckbox","EnableSoundEffects");

	}
	public override void Start () {
		base.Start();
		Debug.Log("Settings menu has started too!");
	}

}
