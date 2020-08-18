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

public class Timer : MonoBehaviour {
	private Color DefaultTextColor;
	private Text TimeString;
	private float SecondsLeft;
	private GameTimerInterface Callback;
	private System.Timers.Timer SysTimer;
	private bool Initialized;
	public Guid InstanceID {get; private set;}
	public bool IsPaused {get; private set;}

	public void Pause() {
		Stop();
		Initialized = false;
		IsPaused = true;
		Debug.Log("Paused.");
	}
	public void Resume() {
		if( !IsPaused ) {
			return;
		}
		Reset(SecondsLeft);
		Initialized = true;
		Debug.Log("Resumed.");
	}
	public bool IsRunning() {
		return SysTimer.Enabled;
	}
	private void OnTickTime(System.Object source, System.Timers.ElapsedEventArgs e) {
		SecondsLeft--;
	}

	public void DestroyTimer() {
		SysTimer.Dispose();
	}

	public void OnApplicationQuit() {
		DestroyTimer();
	}

	public void Stop() {
		SysTimer.Stop();
	}

	public void Reset(float Seconds) {
		SecondsLeft = Seconds;
		SysTimer.Stop();
		SysTimer.Start();
		//Make sure to reset the color.
		TimeString.color = DefaultTextColor;
		Initialized = true;
	}
	private void SetTimer(bool Enabled = true) {
		//Fires every seconds.
		SysTimer = new System.Timers.Timer(1000);
		SysTimer.Elapsed += OnTickTime;
		SysTimer.Enabled = Enabled;
		IsPaused = !Enabled;
	}
	public void Enable() {
		SysTimer.Enabled = true;
	}
	public void Init(GameTimerInterface Parent,float Seconds,bool StartTimer = true) {
		if( !(Parent is GameTimerInterface ) ) {
			Debug.Log("Parent doesn't implement interface GameTimerInterface!");
			return;
		}
		InstanceID = Guid.NewGuid();
		Callback = Parent;
		SecondsLeft = Seconds;
		Initialized = true;
		SetTimer(StartTimer);
	}
	void Awake () {
		TimeString = GetComponent<Text>();
		DefaultTextColor = TimeString.color;
	}
	

	void Update () {
		if( !Initialized ) {
			return;
		}
		//Update The ui.
		TimeSpan t = TimeSpan.FromSeconds( SecondsLeft );
		TimeString.text = string.Format("{0:D2}:{1:D2}",t.Minutes,t.Seconds);
		if( SecondsLeft <= 5 ) {
			//Starts blinking
			bool IsRed = (SecondsLeft % 2) == 1 ? true : false;
			if( IsRed ) {
				TimeString.color = Color.red;
			} else {
				TimeString.color = DefaultTextColor;
			}
		}
		if( SecondsLeft <= 0 ) {
			Callback.OnTimeOut(InstanceID);
			//If we want to use this object we need to be initialized again!
			Initialized = false;
			SysTimer.Enabled = false;
			return;
		}
	}
}
