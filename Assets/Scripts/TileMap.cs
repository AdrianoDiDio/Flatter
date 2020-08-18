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
using System.Xml;
using UnityEngine;

public class TileMap : MonoBehaviour {
	private List<Tile> Tiles;
	private string CurrentMapName;
	private GameObject PlayerObject;
	private GameObject BrickObject;
	// Use this for initialization

	public Tile.TileType StringToTileType(string Name) {
		switch( Name ) {
			case "TILE_BRICK":
				return Tile.TileType.TILE_WALL;
			case "TILE_TARGET":
				return Tile.TileType.TILE_TARGET;
			case "TILE_PLAYER":
				return Tile.TileType.TILE_PLAYER;
			default:
				Debug.Log("Unknown tile type: " + Name);
				break;
			}
			return Tile.TileType.TILE_UNKNOWN;
	}

	private void ParseXML(string Content) {
		XmlDocument Document = new XmlDocument();
		XmlNodeList NodeList;
		
		Document.LoadXml(Content);
		NodeList = Document.SelectNodes("/J2DEditor/MapInfo");
		//foreach( XmlNode Node in NodeList) {
			XmlNode Node = NodeList[0];
			Debug.Log("Node Name: " + Node.Name);
			Debug.Log("Width: " + Node["Width"].InnerText);
			Debug.Log("Height: " + Node["Height"].InnerText);
		//}
		NodeList = Document.SelectNodes("/J2DEditor/TileInfo/Tile");
		Tiles = new List<Tile>();
		foreach( XmlNode tNode in NodeList ) {
			Tile.TileType Type = StringToTileType(tNode["Type"].InnerText);
			Vector2Int Position = new Vector2Int(int.Parse(tNode["X"].InnerText),int.Parse(tNode["Y"].InnerText));
			if( Type == Tile.TileType.TILE_PLAYER ) {
				// Instance new player object...
				GameObject go = Instantiate(PlayerObject,new Vector3(10,0,0),Quaternion.identity) as GameObject;
			} else {
				// Standard map object add it to the list.
				Tiles.Add(new Tile(Position,Type,bool.Parse(tNode["Breakable"].InnerText)));
				//TESTING!
				Vector3Int Pos = new Vector3Int(Position.x,Position.y,0);
				GameObject go = Instantiate(BrickObject,Pos,Quaternion.identity) as GameObject;

			}
		}
		// Now we should have a list of tiles.
		Debug.Log("Loaded " + Tiles.Count + " tiles in list");
	}

	void Start () {
		//CurrentMapName = SceneHolder.GetMapName();
		//DEBUGGING
		CurrentMapName = "UnityTest";
		Debug.Log("Initializing tile map..." + CurrentMapName);
		TextAsset MapData = Resources.Load("Levels/" + CurrentMapName) as TextAsset;
		Debug.Log("Trying to load " + "Levels/" + CurrentMapName);
		if( MapData == null ) {
			Debug.Log("FAILED:File not found...");
			return;
		}
		PlayerObject = Resources.Load("Prefabs/PlayerTile") as GameObject;
		BrickObject = Resources.Load("Prefabs/BrickTile") as GameObject;
		if( BrickObject == null ) {
			Debug.Log("Failed to fetch prefab.");
		}
		//ParseXML(MapData.text);
		GameObject go = Instantiate(PlayerObject,new Vector3(10,0,0),Quaternion.identity) as GameObject;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
