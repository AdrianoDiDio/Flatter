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
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveManagerMenu : Menu {
	public GameObject ContentSavePanel;
	bool IsSaveMode;
	private List<GameObject> SavePanels;
	private InputField[] SaveGames;
	private Button LeftNavButton;
	private Button RightNavButton;
	private int CurrentPage;

	public void CleanUp() {
		foreach(GameObject Panel in SavePanels ) {
			Destroy(Panel);
		}
	}

	private void SetCurrentPanelState(bool Active) {
		Debug.Log("Panel setting " + (Active ? "Active" : "Disabled") +  ": " + CurrentPage + " Panel name: " + SavePanels[CurrentPage].name);
		SavePanels[CurrentPage].SetActive(Active);
	}

	private void PrevPage() {
		Debug.Log("Previous page was at: " + CurrentPage);
		SetCurrentPanelState(false);
		CurrentPage--;
		if( CurrentPage < 0 ) {
			Debug.Log("Cycle");
			CurrentPage = SavePanels.Count - 1;
		}
		Debug.Log("Now is: " + CurrentPage);
		SetCurrentPanelState(true);
	}
	private void NextPage() {
		Debug.Log("Next Page was at: " + CurrentPage);
		SetCurrentPanelState(false);
		CurrentPage++;
		if( CurrentPage >= SavePanels.Count ) {
			Debug.Log("Cycle");
			CurrentPage = 0;
		}
		Debug.Log("Now is: " + CurrentPage);
		SetCurrentPanelState(true);
	}

	public void OnNavButtonClick(string ButtonName) {
		Debug.Log("Clicked: " + ButtonName);
		if( ButtonName == LeftNavButton.name ) {
			Debug.Log("Goin prev.");
			PrevPage();
			return;

		}
		if( ButtonName == RightNavButton.name ) {
			Debug.Log("Going next.");
			NextPage();
			return;
		}
	}

	public void OnInputFieldClick(string SaveGameName) {
		Debug.Log("Loading: " + SaveGameName);
		SaveManager.LaunchSavedGame(this,SaveGameName + SaveManager.DefaultSaveFileExtension);
	}

	public void OnOverWriteSaveGame(string SaveGameName) {
		GameObject Dialog = null;
		Dialog = UiDialog.ShowQuestionDialog("Are you sure you want to overwrite " + SaveGameName + "?",
								"Confirm Overwrite",(delegate { 
										SaveManager.SaveCurrentGame(SaveGameName + SaveManager.DefaultSaveFileExtension); 
										if( Dialog != null ) {
											Destroy(Dialog);
										}
									}));
	}

	/*
	 */
	public void OnInputFieldValueSubmit(InputField Field) {
		string SaveFileName;
		if( Field.text.LastIndexOf(".") >= 0 ) {
			SaveFileName = Field.text;
		} else {
			SaveFileName = Field.text + SaveManager.DefaultSaveFileExtension;
		}
		if( SaveManager.SaveGameExists(SaveFileName) ) {
			UiDialog.ShowWarningDialog("The name you have specified already exists...pick another");
			Field.text = "";
			return;
		}
		SaveManager.SaveCurrentGame(SaveFileName);
	}

	private void RegisterInputFieldClickEvent(GameObject InputFieldObject,string SaveGameName,
												UnityAction<string> OnInputFieldClickAction = null) {
		EventTrigger Trigger = InputFieldObject.AddComponent<EventTrigger>();
		var OnPointerClick = new EventTrigger.Entry();
		OnPointerClick.eventID = EventTriggerType.PointerClick;
		if( OnInputFieldClickAction == null ) {
			OnPointerClick.callback.AddListener((e) => OnInputFieldClick(SaveGameName));
		} else {
			OnPointerClick.callback.AddListener((e) => OnInputFieldClickAction(SaveGameName));
		}
		Trigger.triggers.Add(OnPointerClick);
	}

	private void InitNavButtons() {
		LeftNavButton = GameObject.Find("LeftSaveNavButton").GetComponent<Button>();
		LeftNavButton.onClick.RemoveAllListeners();
		LeftNavButton.onClick.AddListener(delegate {OnNavButtonClick(LeftNavButton.name); });
		RightNavButton = GameObject.Find("RightSaveNavButton").GetComponent<Button>();
		RightNavButton.onClick.RemoveAllListeners();
		RightNavButton.onClick.AddListener(delegate {OnNavButtonClick(RightNavButton.name); });
	}

	private void InstanceSavePanels() {
		int NumSaveGameLabels;
		int NumSaveGames;
		int NumSaveLabelsPerPage = 4;
		int NumSavePages;
		string[] SaveGameFileList = SaveManager.GetSaveGameFileList();
		int FileCursorIndex;
		int SaveGameLabelSpawned;
		NumSaveGames = SaveGameFileList.Length;
		//If we are in save mode we need NumSaveGames + a new one for saving it.
		NumSaveGameLabels = IsSaveMode ?  NumSaveGames + 1 : NumSaveGames;
		NumSavePages = (NumSaveGameLabels + NumSaveLabelsPerPage - 1) / NumSaveLabelsPerPage;
		FileCursorIndex = 0;
		CurrentPage = 0;
		SavePanels = new List<GameObject>();
		SaveGameLabelSpawned = NumSaveGameLabels;
		Debug.Log("IsSaveMode: " + IsSaveMode + " NumSaveGames: " + NumSaveGames);
		Debug.Log("Required labels: " + NumSaveGameLabels);
		Debug.Log("Required pages: " + NumSavePages);
	
		//Spawn the necessary panels...
		for( int i = 0; i < NumSavePages; i++ ) {
			GameObject Parent = Instantiate(Resources.Load("Prefabs/SaveGamePage")) as GameObject;
			Parent.name = "SavePage" + i;
			if( i != CurrentPage ) {
				Parent.SetActive(false);
			}
			Parent.transform.SetParent(ContentSavePanel.transform,false);
			SavePanels.Add(Parent);
			for( int ChildIndex = 0; ChildIndex < NumSaveLabelsPerPage; ChildIndex++ ) {
				//We're done.
				if( SaveGameLabelSpawned <= 0 ) {
					break;
				}
				GameObject Child = Instantiate(Resources.Load("Prefabs/SaveGameInputField")) as GameObject;
				Child.name = "SaveLabel" + ChildIndex;
				InputField ChildInputField = Child.GetComponent<InputField>();
				string InText; 
				Debug.Log("FileCursorIndex: " + FileCursorIndex);
				if( FileCursorIndex < NumSaveGames ) {
					InText = SaveGameFileList[FileCursorIndex];
					//Remove the extension.
					InText = InText.Substring(0,SaveGameFileList[FileCursorIndex].LastIndexOf("."));
				} else {
					InText = "";
				}
				ChildInputField.text = InText;
				//If we are in load mode we cannot edit on the input...
				//If we are in save mode and the input represents a saved
				//game we can only overwrite it.
				if( !IsSaveMode ) {
					RegisterInputFieldClickEvent(Child,InText);
					ChildInputField.readOnly = true;
				} else {
					if( InText.Length == 0 ) {
						ChildInputField.onEndEdit.AddListener(delegate { 
							OnInputFieldValueSubmit(ChildInputField);
						});
					} else {
						RegisterInputFieldClickEvent(Child,InText,
								(delegate { OnOverWriteSaveGame(InText);}));
						ChildInputField.readOnly = true;
					}
				}
				Child.transform.SetParent(Parent.transform,false);
				FileCursorIndex++;
				SaveGameLabelSpawned--;
			}
		}
		//Insert order in list may or may be not respected
		//to be sure we sort them.
		SavePanels = SavePanels.OrderBy(Panel => Panel.name).ToList();
		foreach( GameObject Panel in SavePanels ) {
			Debug.Log("Panel: " + Panel.name);
		}
		Debug.Log("NumSaveGamePanel Spawned: " + SavePanels.Count());
	}
	/*
		For now we will support only up to 4 save game + 1 static element to add. 
	 */
	public void Init(bool SaveMode) {
		IsSaveMode = SaveMode;
		InstanceSavePanels();
		InitNavButtons();
		/* 
		GameObject Parent = GameObject.Find("SaveGamePanel");
		for( int i = 0; i < SaveGames.Length; i++ ) {
			GameObject Temp = Instantiate(Resources.Load("Prefabs/SaveGameInputField")) as GameObject;
			Temp.transform.SetParent(Parent.transform,false);
			string Name = "SaveGameEntry" + i;
			SaveGames[i] = Temp.GetComponent<InputField>();
			SaveGames[i].name = Name;
			if( !IsSaveMode ) {
				EventTrigger Trigger = SaveGames[i].gameObject.AddComponent<EventTrigger>();
				var OnPointerClick = new EventTrigger.Entry();
				OnPointerClick.eventID = EventTriggerType.PointerClick;
				OnPointerClick.callback.AddListener((e) => OnInputFieldClick(Name));
				Trigger.triggers.Add(OnPointerClick);
			}
			SaveGames[i].readOnly = IsSaveMode;
		}
		*/

	}
	override protected void Init() {
		Debug.Log("Suppressing it.");
	}
}
