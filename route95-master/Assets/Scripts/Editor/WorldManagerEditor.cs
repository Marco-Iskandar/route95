using UnityEngine;
using System.Collections;
using UnityEditor;

/// <summary>
/// Custom editor for WorldManager.
/// </summary>
[CustomEditor(typeof(WorldManager))]
public class WorldManagerEditor : Editor {

	int x = 0;
	int y = 0;

	float vertBlend = 0f;

	public override void OnInspectorGUI () {

		DrawDefaultInspector();

		// Button to show debug colors
		if (GUILayout.Button("Show constraints")) {
			((WorldManager)target).DebugTerrain();
		}

		if (GUILayout.Button("Print vertex map")) {
			((WorldManager)target).PrintVertexMap();
		}

		x = EditorGUILayout.IntField ("X:", x);
		y = EditorGUILayout.IntField ("Y:", y);

		vertBlend = EditorGUILayout.FloatField ("Blend: ",vertBlend);

		if (GUILayout.Button("Show coordinates")) {
			WorldManager wm = target as WorldManager;
			Vertex vert = wm.terrain.vertexmap.VertexAt(x, y);
			wm.vertexIndicator.transform.position = vert.WorldPos();
			vertBlend = vert.color.a;
		}

	}
}
