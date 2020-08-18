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

[ System.Serializable ]
public class SerializableVector3 {
	public float x;
	public float y;
	public float z;

	public override string ToString() {
		return "(" + x + ";" + y + ";" + z + ")"; 
	}
	public Vector3 ToVector3() {
		return new Vector3(x,y,z);
	}

	public void FromVector3(Vector3 InVector) {
		x = InVector.x;
		y = InVector.y;
		z = InVector.z;
	}

	public SerializableVector3(Vector3 InVector) {
		FromVector3(InVector);
	}

	public SerializableVector3(float x,float y,float z) {
		this.x = x;
		this.y = y;
		this.z = z;
	}
	
	public SerializableVector3() {
		x = y = z = 0f;
	}
}
