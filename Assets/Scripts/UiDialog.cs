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
using UnityEngine.Events;

public class UiDialog : MonoBehaviour {
	public enum UiDialogType {
		UI_DIALOG_WARNING,
		UI_DIALOG_CONFIRM,
		UI_DIALOG_INPUT
	}
	public enum UiDialogButton {
		UI_DIALOG_BUTTON_OK,
		UI_DIALOG_BUTTON_CANCEL
	}
	public interface UiDialogInputInterface {
		void OnEndEdit(InputField InField);
	}
	public interface UiInputDialogButtonPress {
		void OnButtonPress(UiDialogButton PressedButton);
	}
	private Text DialogTitle;
	private Text DialogContent;
	private Button OkButton;
	private Button CancelButton;
	public InputField DialogInputField;
	/*
		Display a warning dialog which contains only a button to dismiss it.
		By default the dialog on ok will dismiss itself this behaviour can be overriden
		by passing a new unity action.
	 */
	public static GameObject ShowWarningDialog(string Content,string Title = "Warning",UnityAction OnOkPress = null) {
		GameObject Temp = Instantiate(Resources.Load("Prefabs/UiDialog")) as GameObject;
		Temp.GetComponent<UiDialog>().Init(Content,Title,UiDialogType.UI_DIALOG_WARNING,OnOkPress);
		return Temp;
	}
	/*
		Display a question dialog which contains only two button one to cancel the other to confirm.
		By default the dialog on ok/cancel will dismiss itself this behaviour can be overriden
		by passing a new unity action.
	 */
	public static GameObject ShowQuestionDialog(string Content,string Title = "Question",UnityAction OnOkPress = null,
											UnityAction OnCancelPress = null) {
		GameObject Temp = Instantiate(Resources.Load("Prefabs/UiDialog")) as GameObject;
		Temp.GetComponent<UiDialog>().Init(Content,Title,UiDialogType.UI_DIALOG_CONFIRM,OnOkPress,OnCancelPress);
		return Temp;
	}
	public static GameObject ShowInputDialog(string Content,string Title = "Question",UiInputDialogButtonPress OnOkPress = null,
		UiDialogInputInterface OnSubmit = null) {
		GameObject Temp = Instantiate(Resources.Load("Prefabs/UiDialog")) as GameObject;
		Temp.GetComponent<UiDialog>().Init(Content,Title,UiDialogType.UI_DIALOG_INPUT,OnOkPress,null,OnSubmit);
		return Temp;
	}
	public void CloseDialog() {
		Destroy(gameObject);
	}
	public void Init(string Content,string Title,UiDialogType DialogType,UnityAction OnOkPress = null,
						UnityAction OnCancelPress = null,UiDialogInputInterface OnSubmit = null) {
		if( DialogType == UiDialogType.UI_DIALOG_CONFIRM || DialogType == UiDialogType.UI_DIALOG_INPUT ) {
			CancelButton.gameObject.SetActive(true);
		}
		OkButton.gameObject.SetActive(true);
		if( OnOkPress != null ) {
			OkButton.onClick.AddListener(OnOkPress);
		} else {
			OkButton.onClick.AddListener(delegate {CloseDialog();});
		}
		if( OnCancelPress != null ) {
			CancelButton.onClick.AddListener(OnCancelPress);
		} else {
			CancelButton.onClick.AddListener(delegate {CloseDialog();});
		}
		if( DialogType == UiDialogType.UI_DIALOG_INPUT ) {
			DialogInputField.gameObject.SetActive(true);
			if( OnSubmit != null ) {
				DialogInputField.onEndEdit.AddListener(delegate { OnSubmit.OnEndEdit(DialogInputField);});
			} else {
				DialogInputField.onEndEdit.AddListener(delegate {CloseDialog();});
			}
		}
		
		RectTransform CanvasRect;
		//Make it show on top.
		CanvasRect = transform as RectTransform;
		CanvasRect.SetAsLastSibling();
		DialogTitle.text = Title;
		DialogContent.text = Content;
		
	}
	public void Init(string Content,string Title,UiDialogType DialogType,UiInputDialogButtonPress OnOkPress = null,
						UnityAction OnCancelPress = null,UiDialogInputInterface OnSubmit = null) {
		if( DialogType == UiDialogType.UI_DIALOG_CONFIRM ) {
			CancelButton.gameObject.SetActive(true);
		}
		OkButton.gameObject.SetActive(true);
		if( OnOkPress != null ) {
			OkButton.onClick.AddListener(delegate {OnOkPress.OnButtonPress(UiDialogButton.UI_DIALOG_BUTTON_OK);});
		} else {
			OkButton.onClick.AddListener(delegate {CloseDialog();});
		}
		if( OnCancelPress != null ) {
			CancelButton.onClick.AddListener(OnCancelPress);
		} else {
			CancelButton.onClick.AddListener(delegate {CloseDialog();});
		}
		if( DialogType == UiDialogType.UI_DIALOG_INPUT ) {
			DialogInputField.gameObject.SetActive(true);
			if( OnSubmit != null ) {
				DialogInputField.onEndEdit.AddListener(delegate { OnSubmit.OnEndEdit(DialogInputField);});
			} else {
				DialogInputField.onEndEdit.AddListener(delegate {CloseDialog();});
			}
		}
		
		RectTransform CanvasRect;
		//Make it show on top.
		CanvasRect = transform as RectTransform;
		CanvasRect.SetAsLastSibling();
		DialogTitle.text = Title;
		DialogContent.text = Content;
		
	}

	void Awake() {
		Debug.Log("UiDialog Spawned");
		DialogTitle = GameObject.Find("DialogTitle").GetComponent<Text>();
		DialogContent = GameObject.Find("DialogContent").GetComponent<Text>();
		OkButton = GameObject.Find("DialogOkButton").GetComponent<Button>();
		CancelButton = GameObject.Find("DialogCancelButton").GetComponent<Button>();
		DialogInputField = GameObject.Find("DialogInputField").GetComponent<InputField>();
		DialogInputField.gameObject.SetActive(false);
		OkButton.gameObject.SetActive(false);
		CancelButton.gameObject.SetActive(false);
	}
}
