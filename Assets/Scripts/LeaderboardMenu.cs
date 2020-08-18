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
using UnityEngine.UI;

public class LeaderboardMenu : Menu {

	public GameObject ContentPanel;

	override protected void Init() {
		List<LeaderBoard.KeyValuePair<string,int>> LeaderBoardList;
		LeaderBoardList = LeaderBoard.Instance.GetRanks();
		if( LeaderBoardList == null ) {
			return;
		}
		LeaderBoardList = LeaderBoardList.OrderByDescending( v => v.Value).ToList();
		foreach( LeaderBoard.KeyValuePair<string,int> Pair in LeaderBoardList ) {
			//
			GameObject Temp = Instantiate(Resources.Load("Prefabs/LeaderboardEntry")) as GameObject;
			Text[] Child = Temp.GetComponentsInChildren<Text>();
			foreach( Text Element in Child ) {
				if( Element.gameObject.name == "PlayerName" ) {
					Element.text = Pair.Key;
				} else if( Element.gameObject.name == "PlayerScore") {
					Element.text = Pair.Value.ToString();
				} else {
					Debug.Log("Unknown child " + Element.name);
				}
			}
			Temp.transform.SetParent(ContentPanel.transform,false);
		}
	}
	override public void Start() {
		if( ContentPanel == null ) {
			Debug.Log("Leaderboard without content panel..aborting.");
			return;
		} 
		Init();
	} 
}
