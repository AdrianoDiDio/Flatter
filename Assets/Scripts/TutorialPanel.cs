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

public class TutorialPanel : MonoBehaviour {
	public GameObject[] TutorialPanels;
	public ScrollRect ScrollView;
	private Button LeftNavButton;
	private Button RightNavButton;
	private int CurrentPanel;

	private void ScrollToTop() {
		ScrollView.normalizedPosition = new Vector2(0,1);
	}
	private void SetCurrentPanelState(bool Active) {
		TutorialPanels[CurrentPanel].SetActive(Active);
		ScrollToTop();
	}
	private void PrevPanel() {
		SetCurrentPanelState(false);
		CurrentPanel--;
		if( CurrentPanel < 0 ) {
			CurrentPanel = TutorialPanels.Length - 1;
		}
		SetCurrentPanelState(true);
	}
	private void NextPanel() {
		SetCurrentPanelState(false);
		CurrentPanel++;
		if( CurrentPanel >= TutorialPanels.Length ) {
			CurrentPanel = 0;
		}
		SetCurrentPanelState(true);
	}
	public void OnNavButtonClick(string ButtonName) {
		if( ButtonName == LeftNavButton.name ) {
			PrevPanel();
			return;

		}
		if( ButtonName == RightNavButton.name ) {
			NextPanel();
			return;
		}
	}
	private void Init() {
		LeftNavButton = GameObject.Find("LeftNavButton").GetComponent<Button>();
		LeftNavButton.onClick.RemoveAllListeners();
		LeftNavButton.onClick.AddListener(delegate {OnNavButtonClick(LeftNavButton.name); });
		RightNavButton = GameObject.Find("RightNavButton").GetComponent<Button>();
		RightNavButton.onClick.RemoveAllListeners();
		RightNavButton.onClick.AddListener(delegate {OnNavButtonClick(RightNavButton.name); });
		CurrentPanel = 0;
		TutorialPanels[CurrentPanel].SetActive(true);
		for( int i = 1; i < TutorialPanels.Length; i++ ) {
			TutorialPanels[i].SetActive(false);
		}
	}
	void Start () {
		if( TutorialPanels.Length == 0 ) {
			Debug.Log("Tutorial Panel:Nothing to show...");
			return;
		}
		Init();
	}
	
}
