using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;// need for using lists

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Class to store note data.
/// </summary>
[System.Serializable]
public class Note {

	#region Note Vars

	const float DEFAULT_VOLUME = 0.75f;          // Default volume
	const float DEFAULT_DURATION = 1f;        // Default duration

	[SerializeField]
	public string filename;                   // Filename of audio clip

	[SerializeField]
	public float volume = DEFAULT_VOLUME;     // Note volume

	[SerializeField]
	public float duration = DEFAULT_DURATION; // Note duration

	#endregion
	#region Note Methods

	/// <summary>
	/// Default constructor.
	/// </summary>
	public Note () {
		filename = null;
	}

	/// <summary>
	/// Filename/volume/duration constructor.
	/// </summary>
	/// <param name="fileName">Filename to use.</param>
	/// <param name="vol">Initial volume.</param>
	/// <param name="dur">Initial duration.</param>
	public Note (string fileName, float vol=DEFAULT_VOLUME, float dur=DEFAULT_DURATION) {

		// Check if MM has sound
		if (!MusicManager.SoundClips.ContainsKey(fileName)) {
			Debug.LogError ("Note.Note(): filename \"" + fileName + "\" invalid!");
			filename = null;
		} else filename = fileName;

		// Set vars
		volume = vol;
		duration = dur;
	}

	/// <summary>
	/// Plays a note.
	/// </summary>
	/// <param name="source">AudioSource on which to play the note.</param>
	/// <param name="newVolume">Volume scaler.</param>
	/// <param name="cutoff">Cut the AudioSource before playing?</param>
	public void PlayNote (AudioSource source, float newVolume=1f, bool cutoff=false) {
		if (!source.enabled) source.enabled = true;
		else if (cutoff) source.Stop();
		source.PlayOneShot(MusicManager.SoundClips[filename], newVolume*volume*source.volume);
	}

	/// <summary>
	/// Returns whether or not the other note is the same.
	/// </summary>
	/// <param name="other">Note to compare.</param>
	/// <returns></returns>
	public bool Equals (Note other) {
		return filename == other.filename || (this == null && other == null);
	}

	/// <summary>
	/// Checks if a note is a kick.
	/// </summary>
	/// <returns>Returns true if the note is a kick note.</returns>
	public bool IsKick () {
		return filename == "Audio/Instruments/Percussion/RockDrums/RockDrums_Kick";
	}

	/// <summary>
	/// Checks if a note is a snare.
	/// </summary>
	/// <returns></returns>
	public bool IsSnare () {
		return filename == "Audio/Instruments/Percussion/RockDrums/RockDrums_Snare";
	}

	/// <summary>
	/// Checks if a note is a tom.
	/// </summary>
	/// <returns></returns>
	public bool IsTom () {
		return filename == "Audio/Instruments/Percussion/RockDrums/RockDrums_LowTom" || 
			filename == "Audio/Instruments/Percussion/RockDrums/RockDrums_MidTom" || 
			filename == "Audio/Instruments/Percussion/RockDrums/RockDrums_HiTom";
	}

	/// <summary>
	/// Checks if a note is a shaker.
	/// </summary>
	/// <returns></returns>
	public bool IsShaker () {
		return 
			filename == "Audio/Instruments/Percussion/ExoticPercussion/ExoticPercussion_Maracas1" ||
			filename == "Audio/Instruments/Percussion/ExoticPercussion/ExoticPercussion_Maracas2";
	}

	/// <summary>
	/// Checks if a note is a cymbal.
	/// </summary>
	/// <returns></returns>
	public bool IsCymbal () {
		return filename == "Audio/Instruments/Percussion/RockDrums/RockDrums_Crash";
	}

	/// <summary>
	/// Checks if a note is a hat.
	/// </summary>
	/// <returns></returns>
	public bool IsHat () {
		return 
			filename == "Audio/Instruments/Percussion/RockDrums/RockDrums_Hat" ||
			filename == "Audio/Instruments/Percussion/ExoticPercussion/ExoticPercussion_Tambourine";
	}

	/// <summary>
	/// Checks if a note is wood (claves, castinets, cowbell, jam block).
	/// </summary>
	/// <returns></returns>
	public bool IsWood () {
		return
			filename == "Audio/Instruments/Percussion/ExoticPercussion/ExoticPercussion_Castinets" ||
			filename == "Audio/Instruments/Percussion/ExoticPercussion/ExoticPercussion_Claves" ||
			filename == "Audio/Instruments/Percussion/ExoticPercussion/ExoticPercussion_Cowbell" ||
			filename == "Audio/Instruments/Percussion/ExoticPercussion/ExoticPercussion_Cowbell2" ||
			filename == "Audio/Instruments/Percussion/ExoticPercussion/ExoticPercussion_JamBlock";
	}

	#endregion
}
