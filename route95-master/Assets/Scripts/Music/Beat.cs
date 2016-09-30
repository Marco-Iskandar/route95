using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class to store all notes at a certain beat.
/// </summary>
[Serializable]
public class Beat {

	#region Serialized Vars

	[SerializeField]
	public int index;        // Project-specific index

	[SerializeField]
	public List<Note> notes; // List of notes

	#endregion
	#region Beat Methods

	/// <summary>
	/// Default constructor.
	/// </summary>
	public Beat () {
		notes = new List<Note> ();
	}

	/// <summary>
	/// Adds a note to the beat.
	/// </summary>
	/// <param name="note">Note to add.</param>
	public void Add (Note note) {
		notes.Add(note);
	}

	/// <summary>
	/// Removes a note from the beat.
	/// </summary>
	/// <param name="note">Note to remove.</param>
	public void Remove (Note note) {
		notes.Remove(note);
	}

	/// <summary>
	/// Clears all notes in the beat.
	/// </summary>
	public void Clear () {
		notes.Clear();
	}

	/// <summary>
	/// Returns the number of notes in the beat.
	/// </summary>
	public int NoteCount {
		get {
			return notes.Count;
		}
	}

	#endregion
}
