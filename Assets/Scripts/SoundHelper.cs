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

class SoundHelper : MonoBehaviour {
	//Modified version of PlayClipAtPoint that lets you decide whether you want
	//a full 3D or 2D sound.
	//It will create a temporary object that holds the AudioSource and
	//will be destroyed when the clip has finished playing.
	//If SpatialBlend is different than 0 then Position will be used as a reference
	//in order to determine whether we are able to hear the sound or not.
	public static void PlayClipAtPoint(AudioClip Clip,Vector3 Position,float Volume = 1f,float SpatialBlend = 0f) {
		GameObject TempAudio = new GameObject("TempAudio" + Clip.name);
		TempAudio.transform.position = Position;
		AudioSource Source = TempAudio.AddComponent<AudioSource>();
		Source.clip = Clip;
		Source.spatialBlend = SpatialBlend;
		Source.volume = Volume;
		Source.Play();
		Destroy(TempAudio,Clip.length);
		return;
	} 
}
