using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class to hold all scale info.
/// These must be compiled by KeyManager.
/// </summary>
public class Scale {

	#region Scale Enums

	/// <summary>
	/// Musical note.
	/// </summary>
	public enum Note {
		Root,
		Second,
		Third,
		Fourth,
		Fifth,
		Sixth,
		Seventh
	}

	#endregion
	#region Scale Vars

	public List<string> root;     // All roots/octaves in scale
	public List<string> second;   // All seconds in scale
	public List<string> third;    // All thirds in scale
	public List<string> fourth;   // All fourths in scale
	public List<string> fifth;    // All fifths in scale
	public List<string> sixth;    // All sixths in scale
	public List<string> seventh;  // All sevenths in scale

	public List<string> allNotes; // All notes in scale

	#endregion
	#region Scale Methods

	/// <summary>
	/// Default constructor.
	/// </summary>
	public Scale () {

		// Init lists
		root = new List<string> ();
		second = new List<string> ();
		third = new List<string> ();
		fourth = new List<string> ();
		fifth = new List<string> ();
		sixth = new List<string> ();
		seventh = new List<string> ();

		allNotes = new List<string> ();
	}

	#endregion

}
