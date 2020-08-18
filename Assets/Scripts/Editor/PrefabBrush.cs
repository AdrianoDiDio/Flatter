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
﻿#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityEditor
{
    [CreateAssetMenu(fileName = "Prefab brush", menuName = "Brushes/Prefab brush")]
	[CustomGridBrush(false, true, false, "Prefab Brush")]
	public class PrefabBrush : GridBrushBase
	{
		private const float k_PerlinOffset = 100000f;
		public GameObject[] m_Prefabs;
		public float m_PerlinScale = 0.5f;
		public int m_Z;

		private int NumPaintedTiles = 0;

		public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position)
		{
			// Do not allow editing palettes
			if (brushTarget.layer == 31)
				return;

			int index = Mathf.Clamp(Mathf.FloorToInt(GetPerlinValue(position, m_PerlinScale, k_PerlinOffset)*m_Prefabs.Length), 0, m_Prefabs.Length - 1);
			GameObject prefab = m_Prefabs[index];
			GameObject instance = (GameObject) PrefabUtility.InstantiatePrefab(prefab);
			if (instance != null)
			{
				Undo.MoveGameObjectToScene(instance, brushTarget.scene, "Paint Prefabs");
				Undo.RegisterCreatedObjectUndo((Object)instance, "Paint Prefabs");
				instance.transform.SetParent(brushTarget.transform);
				instance.transform.position = grid.LocalToWorld(grid.CellToLocalInterpolated(new Vector3Int(position.x, position.y, m_Z) + new Vector3(.5f, .5f, .5f)));
				//Adriano:Change name of the prefab
				//int NumPaintedTiles = GameObject.FindGameObjectsWithTag(instance.tag).Length;
				if( NumPaintedTiles < 0 ) {
					NumPaintedTiles = 0;
				}
				instance.name = instance.name + " (" + NumPaintedTiles++ + ")";
			}
		}

		public override void Erase(GridLayout grid, GameObject brushTarget, Vector3Int position)
		{
			// Do not allow editing palettes
			if (brushTarget.layer == 31)
				return;

			Transform erased = GetObjectInCell(grid, brushTarget.transform, new Vector3Int(position.x, position.y, m_Z));
			if (erased != null)
				Undo.DestroyObjectImmediate(erased.gameObject);
				NumPaintedTiles--;
		}

		private static Transform GetObjectInCell(GridLayout grid, Transform parent, Vector3Int position)
		{
			int childCount = parent.childCount;
			Vector3 min = grid.LocalToWorld(grid.CellToLocalInterpolated(position));
			Vector3 max = grid.LocalToWorld(grid.CellToLocalInterpolated(position + Vector3Int.one));
			Bounds bounds = new Bounds((max + min)*.5f, max - min);

			for (int i = 0; i < childCount; i++)
			{
				Transform child = parent.GetChild(i);
				if (bounds.Contains(child.position))
					return child;
			}
			return null;
		}

		private static float GetPerlinValue(Vector3Int position, float scale, float offset)
		{
			return Mathf.PerlinNoise((position.x + offset)*scale, (position.y + offset)*scale);
		}

	}

	[CustomEditor(typeof(PrefabBrush))]
	public class PrefabBrushEditor : GridBrushEditorBase
	{
		private PrefabBrush prefabBrush { get { return target as PrefabBrush; } }

		private SerializedProperty m_Prefabs;
		private SerializedObject m_SerializedObject;

		protected void OnEnable()
		{
			m_SerializedObject = new SerializedObject(target);
			m_Prefabs = m_SerializedObject.FindProperty("m_Prefabs");
		}

		public override void OnPaintInspectorGUI()
		{
			m_SerializedObject.UpdateIfRequiredOrScript();
			prefabBrush.m_PerlinScale = EditorGUILayout.Slider("Perlin Scale", prefabBrush.m_PerlinScale, 0.001f, 0.999f);
			prefabBrush.m_Z = EditorGUILayout.IntField("Position Z", prefabBrush.m_Z);
				
			EditorGUILayout.PropertyField(m_Prefabs, true);
			m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
		}
	}
}
#endif