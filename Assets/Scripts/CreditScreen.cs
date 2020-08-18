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

public class CreditScreen : MonoBehaviour {
	public Text CreditsText;
	private void Init() {
		TextAsset Temp = (TextAsset) Resources.Load("Text/Credits",typeof(TextAsset));
		CreditsText.text = Temp.text;
	}
	void Start () {
		if( CreditsText == null ) {
			Debug.Log("Please provide a valid Text label.");
			return;
		}
		Init();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
