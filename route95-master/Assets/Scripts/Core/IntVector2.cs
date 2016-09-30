using UnityEngine;
using System.Collections;

/// <summary>
/// Struct to hold an x,y coordinate pair of ints.
/// </summary>
public struct IntVector2 {

	#region IntVector2 Vars

	public int x;
	public int y;

	#endregion
	#region IntVector2 Methods

	/// <summary>
	/// Default constructor.
	/// </summary>
	/// <param name="_x">Initial x value.</param>
	/// <param name="_y">Initial y value.</param>
	public IntVector2 (int _x, int _y) {
		x = _x;
		y = _y;
	}

	/// <summary>
	/// Returns the distance between two IntVector2s.
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public static float Distance (IntVector2 a, IntVector2 b) {
		return Vector2.Distance (a.ToVector2(), b.ToVector2());
	}

	/// <summary>
	/// Returns whether or not a coordinate is a corner.
	/// </summary>
	/// <returns></returns>
	public bool IsCorner () {
		return 
			(x == 0 || x == WorldManager.instance.chunkResolution-1) &&
			(y == 0 || y == WorldManager.instance.chunkResolution-1);
	}

	/// <summary>
	/// Vector2 conversion.
	/// </summary>
	/// <returns></returns>
	public Vector2 ToVector2 () {
		return new Vector2 ((float)x, (float)y);
	}

	/// <summary>
	/// String conversion.
	/// </summary>
	/// <returns></returns>
	public override string ToString () {
		return "("+x+","+y+")";
	}

	#endregion
}