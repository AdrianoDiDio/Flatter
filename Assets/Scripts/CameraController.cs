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

public class CameraController : MonoBehaviour {
	public float TargetAspect;
	public GameObject Player;
	//private Vector3 Offset;
	private float Depth;

	private void SetAspectRatio() {
		float WindowAspect = (float) Screen.width / (float) Screen.height;
		float ScaleHeight = WindowAspect / TargetAspect;
		Camera Cam = GetComponent<Camera>();
		Cam.orthographicSize = Cam.orthographicSize / ScaleHeight;
	}
	void Start () {
		//Offset = transform.position - Player.transform.position;
		//Vector3 CamPosition = Player.gameObject.transform.position;
		//Preserve camera depth.
		//CamPosition.z = transform.position.z;
		//transform.position = CamPosition;
		Depth = transform.position.z;
		SetAspectRatio();
		DontDestroyOnLoad(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	}

	void LateUpdate() {
		//transform.position = Player.transform.position + Offset;
		if( Player == null ) {
			return;
		}
		Vector3 CamPosition = Player.gameObject.transform.position;
		CamPosition.z = Depth;
		transform.position = CamPosition;
	}
}
