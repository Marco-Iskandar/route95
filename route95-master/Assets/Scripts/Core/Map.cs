using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Generic data storage class with a 2D array that automatically
/// resizes itselft when given out-of-range data. Centered at (0,0).
/// </summary>
/// <typeparam name="T"></typeparam>
public class Map <T> where T: class {

	T[,] values; // 2D array of values
	int width;   // Current width of array

	/// <summary>
	/// Default constructor.
	/// </summary>
	/// <param name="w">Initial width.</param>
	public Map (int w) {
		values = new T[w, w];
		width = w;
	}
		
	/// <summary>
	/// Doubles the width of the array and remaps
	/// all stored values.
	/// </summary>
	void Resize () {

		// Error if invalid width
		if (width == 0) {
			Debug.LogError("Map.Resize(): width 0!");
			return;
		}

		// Save old width
		int oldWidth = width;

		// Create new array
		T[,] newValues = new T[oldWidth*2,oldWidth*2];

		// Remap old values
		for (int x=0; x<oldWidth; x++) for (int y=0; y<oldWidth; y++)
			newValues[x+oldWidth/2, y+oldWidth/2] = values[x,y];

		// Assign new array
		values = newValues;

		// Double width
		width *= 2;
	}

	/// <summary>
	/// Returns the value at i.
	/// </summary>
	/// <param name="i">Coordinates.</param>
	/// <returns></returns>
	public T At (IntVector2 i) {
		return At (i.x, i.y);
	}

	/// <summary>
	/// Returns the value at x,y.
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	public T At (int x, int y) {

		// Return null if values uninitialized or empty
		if (values == null || width == 0) return null;

		// Return null if values out of bounds
		if (x+width/2 < 0 || x+width/2 >= width || 
			y+width/2 < 0 || y+width/2 >= width) return null;
		
		return values[x+width/2,y+width/2];
	}

	/// <summary>
	/// Sets the value at x,y.
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="item"></param>
	public void Set (int x, int y, T item) {

		// While value is out of bounds, resize array
		while (x +width/2 >= width || y+width/2 >= width ||
		x +width/2 < 0 || y+width/2 < 0) Resize();

		values[x+width/2,y+width/2] = item;
	}

	/// <summary>
	/// Returns the current width of the array.
	/// </summary>
	public int Width {
		get {
			return width;
		}
	}

	/// <summary>
	/// Conversion to string.
	/// </summary>
	/// <returns></returns>
	public override string ToString () {
		return "Width: "+width;
	}
}
