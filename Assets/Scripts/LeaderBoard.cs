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
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using UnityEngine;

public class LeaderBoard {
	[System.Serializable]
	[XmlType(TypeName="LeaderboardEntry")]
	public struct KeyValuePair<K,V> {
		public K Key { get; set; }
		public V Value { get; set; }
		public KeyValuePair(K k,V v) {
			Key = k;
			Value = v;
		}
	}
	private static LeaderBoard localInstance;
	private List<KeyValuePair<string,int>> PlayerLeaderboard;

	public static LeaderBoard Instance {
		get {
			if( localInstance == null ) {
				localInstance = new LeaderBoard();
			}
			return localInstance;
		}
	}

	public List<KeyValuePair<string,int>> GetRanks() {
		return PlayerLeaderboard;
	}

	private void LoadLeaderboard() {
		string LeaderboardLocation = Application.persistentDataPath + "/Leaderboard.xml";
		if( !File.Exists(LeaderboardLocation) ) {
			return;
		}
		XmlSerializer Serializer = new XmlSerializer(typeof(List<KeyValuePair<string,int>>),new XmlRootAttribute("Leaderboard"));
		StreamReader Reader = new StreamReader(LeaderboardLocation);
		PlayerLeaderboard = (List<KeyValuePair<string,int>>) Serializer.Deserialize(Reader);
		Reader.Close();
	}

	public void SaveLeaderboard() {
		if( PlayerLeaderboard == null || PlayerLeaderboard.Count == 0) {
			return;
		}
		string LeaderboardLocation = Application.persistentDataPath + "/Leaderboard.xml";
		XmlSerializer Serializer = new XmlSerializer(typeof(List<KeyValuePair<string,int>>),new XmlRootAttribute("Leaderboard"));
		FileStream Writer = File.Open(LeaderboardLocation,FileMode.Create);
		Serializer.Serialize(Writer,PlayerLeaderboard);
		Writer.Close();
	}

	public void AddRank(string Name,int Score) {
		if( PlayerLeaderboard == null ) {
			PlayerLeaderboard = new List<KeyValuePair<string, int>>();
		}
		PlayerLeaderboard.Add(new KeyValuePair<string,int>(Name,Score));
		SaveLeaderboard();
	}
	public bool IsHighScore(int Score) {
		if( PlayerLeaderboard == null || PlayerLeaderboard.Count == 0 ) {
			return true;
		}
		foreach( KeyValuePair<string,int> Pair in PlayerLeaderboard ) {
			if( Score > Pair.Value ) {
				return true;
			}
		}
		return false;
	}
	private LeaderBoard() {
		Debug.Log("Leaderboard is loading.");
		LoadLeaderboard();

	}
}
