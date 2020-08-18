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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

[System.Serializable]
public class AndroidButtonPosition {

	public SerializableRectTransform LeftButton;
	public SerializableRectTransform RightButton;
	public SerializableRectTransform UpButton;
	public SerializableRectTransform ActionButton;


	public void Save() {
		BinaryFormatter BFormatter = new BinaryFormatter();
		FileStream SaveFile = File.Open(Application.persistentDataPath + "/" + 
			PlayerPrefs.GetString("AndroidControlPanelFile","AndroidControlPanelSettings.bin"),FileMode.Create);
		BFormatter.Serialize(SaveFile,this);
		SaveFile.Flush();
		SaveFile.Close();
	}
	/*
		Load the button position from the file and returns the new class if found
		null if not.
	 */
	public static AndroidButtonPosition Load() {
		BinaryFormatter BFormatter = new BinaryFormatter();
		FileStream LoadFile;
		try {
			LoadFile = File.Open(Application.persistentDataPath + "/" + 
				PlayerPrefs.GetString("AndroidControlPanelFile","AndroidControlPanelSettings.bin"),FileMode.Open);
			AndroidButtonPosition Instance = (AndroidButtonPosition) BFormatter.Deserialize(LoadFile);
			Debug.Log("Loaded " + Instance.ToString());
			LoadFile.Close();
			return Instance;
		} catch (FileNotFoundException Exception) {
			Debug.Log("File was not found " + Exception.Message);
		}
		return null;
	}
}
