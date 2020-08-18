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
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableAndroidButton : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
	public static GameObject DraggingItem;
	private Transform Parent;
	private Vector3 StartingPosition;
	public GameObject AndroidPanel;

	private bool UIRectOverlaps(RectTransform RectA,RectTransform RectB) {
		Rect A = new Rect(RectA.localPosition.x,RectA.localPosition.y,RectA.rect.width,RectA.rect.height);
		Rect B = new Rect(RectB.localPosition.x,RectB.localPosition.y,RectB.rect.width,RectB.rect.height);
		return A.Overlaps(B);
	}
	private bool UiRectInside(RectTransform RectT,Vector2 Point) {
		Rect A = new Rect(RectT.localPosition.x,RectT.localPosition.y,RectT.rect.width,RectT.rect.height);
		return A.Contains(Point);
	}
	public void OnBeginDrag(PointerEventData EventData) {
		StartingPosition = transform.position;
		DraggingItem = gameObject;
		Parent = transform.parent;

	}

	public void OnDrag(PointerEventData EventData) {
		Vector3 NewPos = new Vector3(Input.mousePosition.x,StartingPosition.y,0f);
		transform.position = NewPos;
	}

	public void OnEndDrag(PointerEventData EventData) {
		//transform.position = StartingPosition;
		Button[] AndroidElements = Parent.gameObject.GetComponentsInChildren<Button>();
		RectTransform DraggedRect = GetComponent<RectTransform>();
		RectTransform TestRect;
		foreach( Button Element in AndroidElements ) {
			//Skip self.
			if( Element.name == name ) {
				continue;
			}
			TestRect = Element.GetComponent<RectTransform>();
			//Make sure we don't overlaps other buttons.
			if( UIRectOverlaps(DraggedRect,TestRect) ) {
				transform.position = StartingPosition;
				return;
			}
			//Make sure we drag within the boundaries
			if( !RectTransformUtility.RectangleContainsScreenPoint(Parent.GetComponent<RectTransform>(),EventData.position,
				EventData.pressEventCamera) ) {
				transform.position = StartingPosition;
				return;
			}
		}
		AndroidButtonPosition ButtonPosition = InputManager.Instance.DumpCurrentAndroidButtonPosition(Parent.gameObject);
		ButtonPosition.Save();
	}
}
