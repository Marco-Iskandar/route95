// Created using the fantastic tutorial at:
// http://catlikecoding.com/unity/tutorials/curves-and-splines/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;

/// <summary>
/// Base bezier class with operations.
/// </summary>
public class Bezier : MonoBehaviour{

	#region Bezier Enums

	/// <summary>
	/// Control point mode.
	/// </summary>
	public enum BezierControlPointMode {
		Free,
		Aligned,
		Mirrored
	}

	#endregion
	#region Bezier Vars

	[SerializeField]
	[Tooltip("All points in the bezier.")]
	protected List<Vector3> points;

	[SerializeField]
	[Tooltip("All modes in the bezier.")]
	protected List<BezierControlPointMode> modes;

	#endregion
	#region Unity Callbacks

	void OnDrawGizmosSelected () {
		Gizmos.color = Color.blue;

		Vector3[] verts = GetComponent<MeshFilter>().mesh.vertices;
		int[] tris = GetComponent<MeshFilter>().mesh.triangles;
		for (int i=0; i<verts.Length-4 && i<GetComponent<MeshFilter>().mesh.normals.Length; i++)
			Gizmos.DrawLine (verts[i], verts[i+4]);

		for (int i=0; i<tris.Length; i+=3) {
			Gizmos.DrawLine (verts[tris[i]], verts[tris[i+1]]);
			Gizmos.DrawLine (verts[tris[i+1]], verts[tris[i+2]]);
			Gizmos.DrawLine (verts[tris[i+2]], verts[tris[i]]);
		}
	}

	#endregion
	#region Bezier Methods

	/// <summary>
	/// Sets the points of the bezier.
	/// </summary>
	/// <param name="newPoints">Points to use.</param>
	public void SetPoints (List<Vector3> newPoints) {
		points = newPoints;
	}

	/// <summary>
	/// Gets the points of the bezier.
	/// </summary>
	/// <returns></returns>
	public List<Vector3> GetPoints () {
		return points;
	}

	/// <summary>
	/// Sets the control point modes.
	/// </summary>
	/// <param name="newModes">Modes to use.</param>
	public void SetModes (List<BezierControlPointMode> newModes) {
		modes = newModes;
	}

	/// <summary>
	/// Gets the control point modes.
	/// </summary>
	/// <returns></returns>
	public List<BezierControlPointMode> GetModes () {
		return modes;
	}

	/// <summary>
	/// Sets the point at the specified index.
	/// </summary>
	/// <param name="index">Index to change.</param>
	/// <param name="value">New point.</param>
	public void SetPoint (int index, Vector3 value) {
		if (index >= points.Count) return;
		points[index] = value;
	}

	/// <summary>
	/// Returns the coordinates of a point.
	/// </summary>
	/// <param name="p0">First bezier point.</param>
	/// <param name="p1">First control point.</param>
	/// <param name="p2">Second control point.</param>
	/// <param name="p3">Second bezier point.</param>
	/// <param name="t">Progress.</param>
	/// <returns></returns>
	public static Vector3 GetPoint (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01 (t);
		float oneMinusT = 1f - t;
		return
			oneMinusT * oneMinusT * oneMinusT * p0 +
			3f * oneMinusT * oneMinusT * t * p1 +
			3f * oneMinusT * t * t * p2 +
			t * t * t * p3;
	}

	/// <summary>
	/// Returns the local direction of a point.
	/// </summary>
	/// <param name="p0">First bezier point.</param>
	/// <param name="p1">First control point.</param>
	/// <param name="p2">Second control point.</param>
	/// <param name="p3">Second bezier point.</param>
	/// <param name="t">Progress.</param>
	/// <returns></returns>
	public static Vector3 GetFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01 (t);
		float oneMinusT = 1f - t;
		return
			3f * oneMinusT * oneMinusT * (p1 - p0) +
			6f * oneMinusT * t * (p2 - p1) +
			3f * t * t * (p3 - p2);
	}

	/// <summary>
	/// Returns the point at a certain percentage of the bezier.
	/// </summary>
	/// <param name="t">Percentage.</param>
	/// <returns></returns>
	public Vector3 GetPoint (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Count - 4;
		} else {
			t = Mathf.Clamp01 (t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return GetPoint (points [i ], points [i + 1], points [i + 2], points [i + 3], t);
	}

	/// <summary>
	/// Returns the velocity at a certain percentage of the bezier.
	/// </summary>
	/// <param name="t">Percentage.</param>
	/// <returns></returns>
	public Vector3 GetVelocity (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Count - 4;
		} else {
			t = Mathf.Clamp01 (t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return 
			transform.TransformPoint (GetFirstDerivative (points [i], points [i + 1], points [i + 2], points [i + 3], t)) - 
			transform.position;
	}

	/// <summary>
	/// Returns the normalized velocity at a certain percentage of the bezier.
	/// </summary>
	/// <param name="t">Percentage.</param>
	/// <returns></returns>
	public Vector3 GetDirection (float t) {
		return GetVelocity (t).normalized;
	}

	/// <summary>
	/// Returns the normalized relative downwards vector at a point.
	/// </summary>
	/// <param name="direction">Direction.</param>
	/// <returns></returns>
	public Vector3 BezDown (Vector3 direction) {
		Vector3 planed = Vector3.ProjectOnPlane (direction, Vector3.up).normalized; // Project direction on X/Z plane
		planed = Quaternion.Euler(0, -90, 0) * planed;
		return Vector3.Cross (direction, planed);
	}

	/// <summary>
	/// Returns the normalized relative right vector a point.
	/// </summary>
	/// <param name="direction"></param>
	/// <returns>Direction.</returns>
	public Vector3 BezRight (Vector3 direction) {
		Vector3 planed = Vector3.ProjectOnPlane (direction, Vector3.up).normalized;
		planed = Quaternion.Euler (0, -90, 0) * planed;
		return planed;
	}

	/// <summary>
	/// Number of points in the bezier, including control points.
	/// </summary>
	public int PointsCount {
		get {
			return (points.Count);
		}
	}

	/// <summary>
	/// Number of curves in the bezier.
	/// </summary>
	public int CurveCount {
		get {
			return (points.Count - 1) / 3;
		}
	}

	/// <summary>
	/// Number of control modes in the bezier.
	/// </summary>
	public int ModeCount {
		get {
			return modes.Count;
		}
	}

	/// <summary>
	/// Returns the control point at an index.
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public Vector3 GetControlPoint (int index) {
		return points [index];
	}

	/// <summary>
	/// Sets the control point at an index.
	/// </summary>
	/// <param name="index"></param>
	/// <param name="point"></param>
	public void SetControlPoint (int index, Vector3 point) {
		if (index % 3 == 0) {
			Vector3 delta = point - points [index];
			if (index > 0) points [index - 1] += delta;
			if (index + 1 < points.Count) points [index + 1] += delta;
		}
		points [index] = point;
		EnforceMode (index);
	}

	/// <summary>
	/// Returns the control mode at an index.
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public BezierControlPointMode GetControlPointMode (int index) {
		return modes [(index + 1) / 3];
	}

	/// <summary>
	/// Sets the control mode at an index.
	/// </summary>
	/// <param name="index"></param>
	/// <param name="mode"></param>
	public void SetControlPointMode (int index, BezierControlPointMode mode) {
		modes [(index + 1) / 3] = mode;
		EnforceMode (index);
	}

	/// <summary>
	/// Enforces the control point mode at an index.
	/// </summary>
	/// <param name="index"></param>
	public void EnforceMode (int index) {
		int modeIndex = (index + 1) / 3;
		BezierControlPointMode mode = modes [modeIndex];
		if (mode == BezierControlPointMode.Free || modeIndex == 0 || modeIndex == modes.Count - 1) {
			return;
		}

		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if (index <= middleIndex) {
			fixedIndex = middleIndex - 1;
			enforcedIndex = middleIndex + 1;
		} else {
			fixedIndex = middleIndex + 1;
			enforcedIndex = middleIndex - 1;
		}

		Vector3 middle = points [middleIndex];
		Vector3 enforcedTangent = middle - points [fixedIndex];
		points [enforcedIndex] = middle + enforcedTangent;

		if (mode == BezierControlPointMode.Aligned) {
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance (middle, points [enforcedIndex]);
		}
		points [enforcedIndex] = middle + enforcedTangent;
	}

	#endregion
}
