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

public class FifteenPuzzleGameHandler : MonoBehaviour,GameTimerInterface {
	public GameObject Board;
	public GameObject GameOverPanel;
	private RectTransform BoardRect;

	private List<GameObject> Buttons;
	private int GridSize; // 4x4,3x3,2x2 always a square.
	private Button RefreshButton;
	private Button ExitButton;
	private Timer GameTimer;
	private int SkillTime;

	private DoorSwitch Parent;

	public void OnTimeOut(Guid InstanceID) {
		//Shuffle and keep going!
		GameOverPanel.SetActive(true);
	}

	/*
		If Number is odd then ANDing with 1
		result in a number which has the right most bit set to 1.
	 */
	private bool IsOdd(int Number) {
		return (Number & 1) == 1;
	}
	/*
		Returns a vector containing the coordinates of the cell based on the array position.
		X => Row;
		Y => Column;
	 */
	private Vector2Int GetButtonPositionInGrid(int Index) {
		return new Vector2Int(Index/GridSize,Index % GridSize);
	}
	private int GetButtonIndexFromGridPosition(Vector2Int Position) {
		return (GridSize * Position.x) + Position.y;
	}

	private int GetHolePosition() {
		return Buttons.FindIndex(button => button.name == "Hole");
	}

	private int GetNumberOfInversions() {
		int NumInversions = 0;
		for( int i = 0; i < Buttons.Count; i++ ) {
			for( int j = i + 1; j < Buttons.Count; j++ ) {
				if( Buttons[i].name == "Hole" || Buttons[j].name == "Hole" ) {
					continue;
				}
				if( Int32.Parse(Buttons[i].name) > Int32.Parse(Buttons[j].name) ) {
					NumInversions++;
				}
			}
		}
		return NumInversions;
	}
	/*
		Swap 2 gameobject in the list and update the hierarchy to reflect the changes!
	 */
	private void SwapButtons(int AIndex,int BIndex) {
		GameObject Temp;
		if( AIndex > Buttons.Count || BIndex > Buttons.Count ) {
			Debug.Log("SwapButtons:Not allowed since one of the 2 index is out of bounds...A: " + AIndex + " B: " + BIndex);
			return;
		}
		Temp = Buttons[AIndex];
		Buttons[AIndex] = Buttons[BIndex];
		Buttons[BIndex] = Temp;
		Buttons[AIndex].GetComponent<RectTransform>().SetSiblingIndex(AIndex);
		Buttons[BIndex].GetComponent<RectTransform>().SetSiblingIndex(BIndex);
	}

	/*
		Adriano:
		Given that our grid is 4x4 to check if the game can be solved
		we need to check 2 conditions depending on the hole position:
			- If the hole entity is on an even row counting from bottom
			  and number of inversions is odd then can be solved.
			- If the hole is on an odd row counting from the bottom and number
			  of inversions is even.
		Note that this is not true if the grid was not even (5x5,3x3 etc).
		In that case we just need to count the number of inversions and see if is even.
 	*/
	private bool IsSolvable() {
		//First thing first get Hole position.
		int Index = GetHolePosition();
		//Counting from the bottom check if the row count is even or odd!
		Debug.Log("Hole is at " + Index);
		int RowNumberFromLast = GridSize - GetButtonPositionInGrid(Index).x; // Get the row of the button!
		int NumInversions = GetNumberOfInversions();
		Debug.Log("RowNumberFromLast = " + RowNumberFromLast);
		Debug.Log("Inversion Count is: " + NumInversions);

		// If the grid size is odd (3x3) then we just need to find out if the number of inversions
		// is even.
		if( IsOdd(GridSize) ) {
			if( !IsOdd(NumInversions) ) {
				return true;
			}
		} else {
			if( IsOdd(RowNumberFromLast) ) {
				if( !IsOdd(NumInversions) ) {
					return true;
				}
			} else {
				//Even
				if( IsOdd(NumInversions) ) {
					return true;
				}
			}
		}
		return false;
	}

	private void Reload() {
		Refresh();
		ResetTimer();
	}
	private void Shuffle() {
		for( int i = 0; i < Buttons.Count; i++ ) {
			int RandomIndex = UnityEngine.Random.Range(i,Buttons.Count);
			SwapButtons(i,RandomIndex);
		}
	}

	private void CheckWinCondition() {
		bool HasWin;
		HasWin = true;
		for( int i = 0; i < Buttons.Count; i++ ) {
			int ButtonPosition = i+1;
			if( Buttons[i].name == "Hole" ) {
				continue;
			}
			if( ButtonPosition != Int32.Parse(Buttons[i].name) ) {
				Debug.Log("I != ButtonsName: " + i + " " + Int32.Parse(Buttons[i].name));
				HasWin = false;
				break;
			}
		}
		if( HasWin ) {
			Debug.Log("Player won...");
			GameHandler.Instance.MiniGameNotifyWin(Parent.MiniGame);
		} else {
			Debug.Log("Tiles not in order...");
		}
	}

	//TODO:Based on PlayerPrefs skill limit the maximum number of moves.
	private void OnButtonClick(string ButtonName) {
		GameObject Button;
		int ButtonIndex;
		Vector2Int GridPosition;

		ButtonIndex = Buttons.FindIndex(button => button.name == ButtonName);
		
		//Should never happen.
		if( ButtonIndex == -1 ) {
			Debug.Log("Unknown button: " + ButtonName);
		}

		Debug.Log("ButtonName: " + ButtonName);
		Debug.Log("ButtonName from array: " + Buttons[ButtonIndex].name);
		Debug.Log("ButtonNumber: " + Int32.Parse(Buttons[ButtonIndex].name));

		Button = Buttons[ButtonIndex];
		GridPosition = GetButtonPositionInGrid(ButtonIndex);
		Debug.Log("GridPosition: " + GridPosition.ToString());
		for( int XOffset = -1; XOffset < 2; XOffset += 2 ) {
			Debug.Log("XOffset: " + XOffset);
			int NeighbourIndex = GetButtonIndexFromGridPosition(new Vector2Int(GridPosition.x + XOffset,GridPosition.y));
			Vector2Int NeighbourPos = GetButtonPositionInGrid(NeighbourIndex);
			Debug.Log("Neighbour index is: " + NeighbourIndex);
			//Off bounds.
			if( NeighbourIndex < 0 || NeighbourIndex >= Buttons.Count) {
				continue;
			}
			//We are allowed to move only horizontally!
			if( Buttons[NeighbourIndex].name == "Hole" && NeighbourPos.y == GridPosition.y) {
				//Thats it...swap me!
				SwapButtons(ButtonIndex,NeighbourIndex);
				CheckWinCondition();
				return;
			}
		}
		for( int YOffset = -1; YOffset < 2; YOffset += 2 ) {
			int NeighbourIndex = GetButtonIndexFromGridPosition(new Vector2Int(GridPosition.x,GridPosition.y + YOffset));
			Vector2Int NeighbourPos = GetButtonPositionInGrid(NeighbourIndex);
			//Off bounds.
			if( NeighbourIndex < 0 || NeighbourIndex >= Buttons.Count ) {
				continue;
			}
			//We are allowed to move only vertically!
			if( Buttons[NeighbourIndex].name == "Hole" && GridPosition.x == NeighbourPos.x ) {
				//Thats it...swap me!
				SwapButtons(ButtonIndex,NeighbourIndex);
				CheckWinCondition();
				return;
			}
		}
	}

	private void OnUiButtonClick(string Name) {
		Debug.Log("UiButtonClick: " + Name);
		if( Name == RefreshButton.name ) {
			//Since we need to shuffle and it could take seconds
			//wait before resetting it!
			GameOverPanel.SetActive(false);
			Reload();
			return;
		}
		if( Name == ExitButton.name ) {
			Debug.Log("Going out of it.");
			//Close the minigame.
			GameHandler.Instance.MiniGameNotifyExit(Parent.MiniGame);			
			return;
		}
	}

	private void Refresh() {
		int DebugNumShuffle = 0;
		do {
			DebugNumShuffle++;
			Shuffle();
		} while( !IsSolvable() );
		Debug.Log("Took " + DebugNumShuffle + " shuffle to create a valid game.");

	}

	private void ResetTimer() {
		GameTimer.Reset(SkillTime);
	}

	private void SetSkill() {
		switch( GameHandler.Instance.Skill ) {
			case GameHandler.PlayerSkill.Easy:
				GridSize = 2;
				SkillTime = 20;
				break;
			case GameHandler.PlayerSkill.Medium:
				GridSize = 3;
				SkillTime = 120;
				break;
			case GameHandler.PlayerSkill.Hard:
				GridSize = 4;
				SkillTime = 300;
				break;
		}
	}
	private void InitGrid() {
		Buttons = new List<GameObject>();
		BoardRect = Board.GetComponent<RectTransform>();
		RefreshButton = GameObject.Find("RefreshButton").GetComponent<Button>();
		RefreshButton.onClick.AddListener(() => { OnUiButtonClick(RefreshButton.name);});
		ExitButton = GameObject.Find("ExitButton").GetComponent<Button>();
		ExitButton.onClick.AddListener(() => { OnUiButtonClick(ExitButton.name);});
		SetSkill();
		GameOverPanel.SetActive(false);
		Board.GetComponent<GridLayoutGroup>().constraintCount = GridSize;
		for( int i = 1; i < GridSize * GridSize; i++ ) {
			string ButtonName = (i).ToString();
			GameObject button = Instantiate(Resources.Load("Prefabs/TileButton")) as GameObject;
			button.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>("Sprites/Numbers/" + ButtonName);
			button.name = ButtonName;
			button.transform.SetParent(BoardRect,false); 
			button.GetComponent<Button>().onClick.AddListener(() => { OnButtonClick(button.name);});
			Buttons.Add(button);
		}
		GameObject Hole = new GameObject();
		Hole.name = "Hole";
		Hole.AddComponent<RectTransform>();
		Hole.transform.SetParent(BoardRect,false);
		Buttons.Add(Hole);
		Refresh();
		int NeighbourIndex = GetButtonIndexFromGridPosition(new Vector2Int(1,0));
		Debug.Log("Neighbourindex: " + NeighbourIndex);

	}

	private void InitTimer() {
		GameObject Timer = Instantiate(Resources.Load("Prefabs/GameTimer")) as GameObject;
		GameTimer = Timer.GetComponent<Timer>();
		GameTimer.Init(this,SkillTime);
		Timer.transform.SetParent(GameObject.Find("TopPanel").GetComponent<RectTransform>(),false);
		//Timer.SendMessage("Init",this);
	}

	public void Init(DoorSwitch Parent) {
		this.Parent = Parent;
		InitGrid();
		InitTimer();
	}
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
