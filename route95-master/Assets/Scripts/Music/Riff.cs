using UnityEngine;
using System; // for enum stuff
using System.Collections;
using System.Collections.Generic;// need for using lists
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Class to store sequences of beats, as well as effect data.
/// </summary>
[System.Serializable]
public class Riff {

	#region NonSerialized Riff Vars

	/// <summary>
	/// Maximum number of 16th notes in a riff.
	/// </summary>
	[NonSerialized]
	public const int MAX_BEATS = 16;

	[NonSerialized]
	public Instrument instrument;            // Reference to instrument used in riff

	[NonSerialized]
	AudioSource source;                      // Source to play notes on

	[NonSerialized]
	AudioDistortionFilter distortion;        // Distortion filter

	[NonSerialized]
	AudioTremoloFilter tremolo;              // Tremolo filter

	[NonSerialized]
	AudioChorusFilter chorus;                // Chorus filter

	[NonSerialized]
	AudioFlangerFilter flanger;              // Flanger filter

	[NonSerialized]
	AudioEchoFilter echo;                    // Echo filter

	[NonSerialized]
	AudioReverbFilter reverb;                // Reverb filter

	#endregion
	#region Serialized Riff Vars

	[SerializeField]
	public string name;                      // User-defined name of the riff

	[SerializeField]
	public int instrumentIndex = 0;          // Index of instrument used for this riff

	[SerializeField]
	public List<int> beatIndices;            // List of indices of all beats used in this riff

	[SerializeField]
	public bool cutSelf = true;              // If true, sounds will cut themselves off

	[SerializeField]
	public float volume = 0.8f;                // Volume scaler for all riff notes

	[SerializeField]
	public float panning = 0f;               // Stereo panning value

	[SerializeField]
	public int index;                        // Project-assigned riff index

	[SerializeField]
	public bool distortionEnabled = false;   // Is distortion enabled on this riff?
	[SerializeField]
	public float distortionLevel = 0f;       // Level of distortion

	// Tremolo
	[SerializeField]
	public bool tremoloEnabled = false;      // Is tremolo enabled on this riff?
	[SerializeField]
	public float tremoloRate = 0f;           // Rate of tremolo oscillation
	[SerializeField]
	public float tremoloDepth = 0f;          // Amplitude of tremolo oscillation

	// Chorus
	[SerializeField]
	public bool chorusEnabled = false;       // Is chorus enabled on this riff?
	[SerializeField]
	public float chorusDryMix = 0f;          // Ratio of dry/wet output
	[SerializeField]
	public float chorusRate= 0f;             // Rate of chorus oscillation
	[SerializeField]
	public float chorusDepth = 0f;           // Depth of chorus oscillation

	// Echo
	[SerializeField]
	public bool echoEnabled = false;         // Is echo enabled on this riff?
	[SerializeField]
	public float echoDecayRatio = 1f;        // Ratio of echo output to previous output
	[SerializeField]
	public float echoDelay = 0f;             // Echo delay
	[SerializeField]
	public float echoDryMix = 0f;            // Echo dry/wet ratio

	// Reverb
	[SerializeField]
	public bool reverbEnabled = false;       // Is reverb enabled on this riff?
	[SerializeField]
	public float reverbDecayTime = 0f;       // Reverb decay time
	[SerializeField]
	public float reverbLevel = 0f;           // Level of reverb

	// Flanger
	[SerializeField]
	public bool flangerEnabled = false;      // Is flanger enabled on this riff?
	[SerializeField]
	public float flangerRate = Mathf.PI/32f; // Rate of flanger oscillation
	[SerializeField]
	public float flangerDryMix = 0f;         // Flanger dry/wet mix

	#endregion
	#region Riff Methods

	/// <summary>
	/// Default constructor.
	/// </summary>
	public Riff (int instIndex=0) {
		beatIndices = new List<int>();
		instrumentIndex = instIndex;
		Refresh();
	}

	public void Refresh () {

		// Init references
		if (instrument == null) instrument = Instrument.AllInstruments[instrumentIndex];
		source = MusicManager.instance.instrumentAudioSources[instrument];
		distortion = source.GetComponent<AudioDistortionFilter>();
		tremolo    = source.GetComponent<AudioTremoloFilter>();
		chorus     = source.GetComponent<AudioChorusFilter>();
		flanger    = source.GetComponent<AudioFlangerFilter>();
		echo       = source.GetComponent<AudioEchoFilter>();
		reverb     = source.GetComponent<AudioReverbFilter>();
	}

	/// <summary>
	/// Checks if a note exists in the riff.
	/// </summary>
	/// <param name="filename">Note filename.</param>
	/// <param name="pos">Beat position.</param>
	/// <returns>True if a note with the given filename
	/// exists in the riff.</returns>
	public bool Lookup (string filename, int pos) {
		Note temp = new Note(filename);
		return Lookup (temp, pos);
	}

	/// <summary>
	/// Checks if a note exists in the riff.
	/// </summary>
	/// <param name="newNote">Note.</param>
	/// <param name="pos">Beat position.</param>
	/// <returns>True if a note with the same filename
	/// as the given note exists in the riff.</returns>
	public bool Lookup (Note newNote, int pos) {
		Song song = MusicManager.instance.currentSong;
		Beat beat = song.beats[beatIndices[pos]];

		try {

			// Check each note in beat
			foreach (Note note in beat.notes)
				if (note.filename == newNote.filename) return true;

			return false;
		
		// Catch invalid beat checks
		} catch (ArgumentOutOfRangeException) {
			Debug.LogError("Tried to access pos "+pos+" in "+Length+"-long riff!");
			return false;
		}
	}

	public Note GetNote (string fileName, int pos) {
		Song song = MusicManager.instance.currentSong;
		Beat beat = song.beats[beatIndices[pos]];

		foreach (Note note in beat.notes)
			if (note.filename == fileName) return note;
		
		return null;
	}

	/// <summary>
	/// Toggles a note at the given position.
	/// </summary>
	/// <param name="newNote">Note to toggle.</param>
	/// <param name="pos">Position at which to add note.</param>
	/// <returns>True if note was added,
	/// false if note was removed.</returns>
	public bool Toggle (Note newNote, int pos) {
		Song song = MusicManager.instance.currentSong;
		Beat beat = song.beats[beatIndices[pos]];
		Instrument instrument = Instrument.AllInstruments[instrumentIndex];

		// Check if note exists
		if (Lookup(newNote, pos)) {
			RemoveNote (newNote, pos);
			return false;
		}

		// If doesn't exist, add note
		beat.Add (newNote);

		// Play note
		source.panStereo = panning;
		newNote.PlayNote(source, volume, true);

		// Do environmental effects
		if (instrument.type == Instrument.Type.Percussion) {
			if (newNote.IsSnare()) WorldManager.instance.LightningStrike(0.5f * volume * source.volume * newNote.volume);
			else if (newNote.IsKick()) WorldManager.instance.LightningFlash(0.5f * volume * source.volume * newNote.volume);
			else if (newNote.IsTom()) WorldManager.instance.LightningFlash(0.375f * volume * source.volume * newNote.volume);
			else if (newNote.IsShaker()) WorldManager.instance.shakers++;
			else if (newNote.IsHat()) WorldManager.instance.StarBurst();
			else if (newNote.IsCymbal()) WorldManager.instance.ShootingStar();
			else if (newNote.IsWood()) WorldManager.instance.ExhaustPuff();
		} else {
			if (instrument == MelodicInstrument.ElectricBass) WorldManager.instance.DeformRandom();
			else {
				switch (instrument.family) {
				case Instrument.Family.Guitar:
					MusicManager.instance.guitarNotes++;
					break;
				case Instrument.Family.Keyboard:
					MusicManager.instance.keyboardNotes++;
					break;
				case Instrument.Family.Brass:
					MusicManager.instance.brassNotes++;
					break;
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Plays all notes at the given beat.
	/// </summary>
	/// <param name="pos">Beat to play notes from.</param>
	public void PlayRiff (int pos) { 
		try {
			Song song = MusicManager.instance.currentSong;
			Beat beat = song.beats[beatIndices[pos]];

			// Skip if empty
			if (beat.NoteCount == 0) return;

			source.panStereo = panning;

			// Update effect levels
			if (distortionEnabled) {
				distortion.enabled = true;
				distortion.distortionLevel = distortionLevel;
			} else distortion.enabled = false;

			if (tremoloEnabled) {
				tremolo.enabled = true;
				tremolo.depth = tremoloDepth;
				tremolo.rate = tremoloRate;
			} else tremolo.enabled = false;

			if (echoEnabled) {
				echo.enabled = true;
				echo.decayRatio = echoDecayRatio;
				echo.delay = echoDelay;
				echo.dryMix = echoDryMix;
			} else echo.enabled = false;

			if (reverbEnabled) {
				reverb.enabled = true;
				reverb.decayTime = reverbDecayTime;
				reverb.reverbLevel = reverbLevel;
			} else reverb.enabled = false;

			if (chorusEnabled) {
				chorus.enabled = true;
				chorus.dryMix = chorusDryMix;
				chorus.rate = chorusRate;
				chorus.depth = chorusDepth;
			} else chorus.enabled = false;

			if (flangerEnabled) {
				flanger.enabled = true;
				flanger.rate = flangerRate;
				flanger.dryMix = flangerDryMix;
			} else flanger.enabled = false;

			// Cutoff
			if (cutSelf) source.Stop();

			// For each note
			foreach (Note note in beat.notes) {

				// Play note
				note.PlayNote(source, volume);

				// Do environmental effects
				if (instrument.type == Instrument.Type.Percussion) {
					if (note.IsSnare()) WorldManager.instance.LightningStrike(note.volume * volume * source.volume);
					else if (note.IsKick()) WorldManager.instance.LightningFlash(note.volume * volume * source.volume);
					else if (note.IsTom()) WorldManager.instance.LightningFlash(0.75f * note.volume * volume * source.volume);
					else if (note.IsShaker()) WorldManager.instance.shakers++;
					else if (note.IsHat()) WorldManager.instance.StarBurst();
					else if (note.IsCymbal()) WorldManager.instance.ShootingStar();
					else if (note.IsWood()) WorldManager.instance.ExhaustPuff();
				} else {
					if (instrument == MelodicInstrument.ElectricBass)
						WorldManager.instance.DeformRandom();

					else {
						switch (instrument.family) {
							case Instrument.Family.Guitar:
								MusicManager.instance.guitarNotes++;
								break;
							case Instrument.Family.Keyboard:
								MusicManager.instance.keyboardNotes++;
								break;
							case Instrument.Family.Brass:
								MusicManager.instance.brassNotes++;
								break;
						}
					
					}
				}
			}
		} catch (ArgumentOutOfRangeException) {
			Debug.LogError("Tried to play out of range of song! Pos: "+pos);
		}
	}

	/// <summary>
	/// Removes the given note from a beat.
	/// </summary>
	/// <param name="newNote">Note to remove.</param>
	/// <param name="pos">Beat to remove note from.</param>
	public void RemoveNote (Note newNote, int pos) {
		Song song = MusicManager.instance.currentSong;
		Beat beat = song.beats[beatIndices[pos]];


		// Look for note in beat
		foreach (Note note in beat.notes)
			if (note.filename == newNote.filename) {
				beat.notes.Remove(note);
				return;
			}
	}

	/// <summary>
	/// Removes all notes at the given beat.
	/// </summary>
	/// <param name="pos">Beat to remove notes at.</param>
	public void Clear (int pos) {
		Song song = MusicManager.instance.currentSong;
		Beat beat = song.beats[beatIndices[pos]];
		beat.Clear();
	}

	/// <summary>
	/// Returns the volume of a note with the given filename.
	/// </summary>
	/// <param name="fileName">Filename of note to find.</param>
	/// <param name="pos">Beat to look in.</param>
	/// <returns>The volume of the note with the given
	/// filename, or 1 if no note found.</returns>
	public float VolumeOfNote(string fileName, int pos) {
		Song song = MusicManager.instance.currentSong;
		Beat beat = song.beats[beatIndices[pos]];

		// Check each note in beat
		foreach (Note note in beat.notes)
			if (note.filename == fileName) return note.volume;

		// Return 1f if not found
		return 1f;
	}

	/// <summary>
	/// Returns the length of the riff, in number of beats.
	/// </summary>
	public int Length {
		get {
			return beatIndices.Count;
		}
	}

	#endregion
}
