using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class to hold all types of scales.
/// </summary>
public class ScaleInfo {

	#region ScaleInfo Vars

	public string name;      // Name of scale
	public int scaleIndex;   // Index in list of all scales

	public int rootIndex;    // Interval to root
	public int secondIndex;  // Interval to second
	public int thirdIndex;   // Interval to third
	public int fourthIndex;  // Interval to fourth
	public int fifthIndex;   // Interval to fifth
	public int sixthIndex;   // Interval to sixth
	public int seventhIndex; // Interval to seventh

	/// <summary>
	/// Major scale.
	/// </summary>
	public static ScaleInfo Major = new ScaleInfo () {
		name = "Major",
		scaleIndex = 0,
		secondIndex = 2,
		thirdIndex = 2,
		fourthIndex = 1,
		fifthIndex = 2,
		sixthIndex = 2,
		seventhIndex = 2,
		rootIndex = 1
	};

	/// <summary>
	/// Minor scale.
	/// </summary>
	public static ScaleInfo Minor = new ScaleInfo () {
		name = "Minor",
		scaleIndex = 1,
		secondIndex = 2,
		thirdIndex = 1,
		fourthIndex = 2,
		fifthIndex = 2,
		sixthIndex = 1,
		seventhIndex = 2,
		rootIndex = 2
	};
			
	/// <summary>
	/// List of all scale types.
	/// </summary>
	public static List<ScaleInfo> AllScales = new List<ScaleInfo> () {
		Major,
		Minor
	};

	#endregion

}