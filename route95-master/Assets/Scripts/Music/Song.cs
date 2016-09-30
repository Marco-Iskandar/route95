using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// Class to store all song data.
/// </summary>
[System.Serializable]
public class Song {

	#region Serialized Song Vars

	[SerializeField]
	public string name;                // User-defined name of song.

	[SerializeField]
	public Key key = Key.None;         // Musical key of song

	[SerializeField]
	public int scale = -1;             // Index of scale used in song

	[SerializeField]
	public List<SongPiece> songPieces; // All song pieces used in song

	[SerializeField]
	public List<Measure> measures;     // All measures used in song

	[SerializeField]
	public List<Riff> riffs;           // All riffs used in song

	[SerializeField]
	public List<Beat> beats;           // All beats used in song

	[SerializeField]
	public List<int> songPieceIndices; // Indices of song pieces used

	/// <summary>
	/// Default constructor.
	/// </summary>
	public Song () {

		// Default name
		name = "New Song";

		// Init lists
		songPieceIndices = new List<int>();
		songPieces = new List<SongPiece>();
		measures = new List<Measure>();
		riffs = new List<Riff>();
		beats = new List<Beat>();
	}

	/// <summary>
	/// Refreshes all data that was not loaded.
	/// Called after deserialization.
	/// </summary>
	/// <param name="context"></param>
	[OnDeserialized()]
	internal void Refresh (StreamingContext context) {

		// Init any uninitialized lists
		if (songPieceIndices == null) songPieceIndices = new List<int>();
		if (songPieces == null) songPieces = new List<SongPiece>();
		if (measures == null) measures = new List<Measure>();
		if (riffs == null) riffs = new List<Riff>();
		if (beats == null) beats = new List<Beat>();

		// Refresh riffs
		foreach (Riff riff in riffs) riff.Refresh();
	}

	/// <summary>
	/// Returns the number of beats in the song.
	/// </summary>
	public int Beats {
		get {
			return songPieceIndices.Count * 16;
		}
	}

	/// <summary>
	/// Returns the number of measures in the song.
	/// </summary>
	public int Measures {
		get {
			return songPieceIndices.Count;
		}
	}

	/// <summary>
	/// Checks if a song has the same name as another.
	/// </summary>
	/// <param name="other">Song to compare to.</param>
	/// <returns>True if the other song has the same name.</returns>
	public bool Equals (Song other) {
		return name == other.name;
	}

	/// <summary>
	/// Creates a new song piece and registers it.
	/// </summary>
	/// <returns></returns>
	public SongPiece NewSongPiece () {

		// Create new song piece
		SongPiece songPiece = new SongPiece();

		// Init measures
		Measure measure = NewMeasure();
		songPiece.measureIndices.Add (measure.index);

		// Register song piece
		RegisterSongPiece (songPiece);

		return songPiece;
	}

	/// <summary>
	/// Adds a song piece to the song and assigns it an index.
	/// </summary>
	/// <param name="songPiece">Song piece to register.</param>
	public void RegisterSongPiece (SongPiece songPiece) {
		songPiece.index = songPieces.Count;
		songPieces.Add (songPiece);
		songPieceIndices.Add (songPiece.index);
	}

	/// <summary>
	/// Adds a riff to the song and assigns it an index,
	/// and does the same with all of its beats.
	/// </summary>
	/// <param name="riff">Riff to register.</param>
	public void RegisterRiff (Riff riff) {
		riff.index = riffs.Count;
		riffs.Add (riff);
		for (int i=0; i<16; i++) {
			Beat beat = new Beat ();
			beat.index = beats.Count;
			riff.beatIndices.Add (beat.index);
			beats.Add (beat);
		}
	}

	/// <summary>
	/// Adds a new measure and registers it.
	/// </summary>
	/// <returns></returns>
	public Measure NewMeasure () {
		Measure measure = new Measure();
		measure.index = measures.Count;
		measures.Add(measure);
		return measure;
	}
		
	/// <summary>
	/// Plays all notes at the given beat.
	/// </summary>
	/// <param name="pos">Beat at which to play notes.</param>
	public void PlaySong (int pos) {
		try {
			SongPiece songPiece = songPieces[songPieceIndices[pos/Riff.MAX_BEATS]];
			Measure measure = measures[songPiece.measureIndices[0]];

			// Play all notes
			foreach (int i in measure.riffIndices) riffs[i].PlayRiff(pos % Riff.MAX_BEATS);

		} catch (ArgumentOutOfRangeException) {
			Debug.LogError ("Song.PlaySong(): index out of range! "+pos);
		}
	}

	/// <summary>
	/// Toggles a riff at the given position.
	/// </summary>
	/// <param name="newRiff">Riff to toggle.</param>
	/// <param name="pos">Measure to toggle.</param>
	public void ToggleRiff (Riff newRiff, int pos) {
		SongPiece songPiece = songPieces[songPieceIndices[pos]];
		Measure measure = measures[songPiece.measureIndices[0]];

		// For each riff in measure
		foreach (int r in measure.riffIndices) {
			Riff riff = riffs[r];

			// Skip if riff not the same instrument
			if (riff.instrument != newRiff.instrument) continue;

			// Remove riff if it exists
			if (newRiff == riff) measure.riffIndices.Remove (newRiff.index);

			// Replace riff with new one
			else {
				measure.riffIndices.Remove (riff.index);
				measure.riffIndices.InsertSorted<int> (newRiff.index, false);
			}

			return;
		}

		// Riff not already there
		measure.riffIndices.InsertSorted<int> (newRiff.index, false);
	}

	#endregion
}
