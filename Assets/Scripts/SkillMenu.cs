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
using UnityEngine.SceneManagement;

public class SkillMenu : Menu {

	override protected void OnButtonClick(string ButtonName) {
		GameHandler.PlayerSkill Skill;
		switch( ButtonName ) {
			case "EasySkillButton":
				Skill = GameHandler.PlayerSkill.Easy;
				break;
			case "MediumSkillButton":
				Skill = GameHandler.PlayerSkill.Medium;
				break;
			case "HardSkillButton":
				Skill = GameHandler.PlayerSkill.Hard;
				break;
			default:
				Debug.Log("Unknown Skill " + ButtonName);
				return;
		}
		InputManager.Instance.StartCoroutine(SceneUtils.LoadSceneAsync(1,Skill));

		//This may work but needs to be double-checked...
		//GameHandler.Instance.Skill = Skill;
	}
}
