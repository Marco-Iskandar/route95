using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class to store all measure data.
/// </summary>
[System.Serializable]
public class SongPiece {

	#region NonSerialized Song Piece Vars

	const int DEFAULT_MEASURES = 1;

	#endregion
	#region Serialized Song Piece Vars

	[SerializeField]
	public string name;              // Name of song piece

	[SerializeField]
	public int index;                // Project-assigned index

	[SerializeField]
	public List<int> measureIndices; // List of indices of measures used

	#endregion
	#region Song Piece Methods

	/// <summary>
	/// Default constructor.
	/// </summary>
	public SongPiece () {
		measureIndices = new List<int> ();
	}

	/// <summary>
	/// Plays all notes at the given position.
	/// </summary>
	/// <param name="pos">Beat at which to play notes.</param>
	public void PlaySongPiece (int pos){
		int measureNum = pos/4;
		Song song = MusicManager.instance.currentSong;
		Measure measure = song.measures[measureNum];

		// Play all riffs
		foreach (int r in measure.riffIndices) {
			Riff riff = song.riffs[r];
			riff.PlayRiff (pos % 4);
		}
	}

	#endregion

}
