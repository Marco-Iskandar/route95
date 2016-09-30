using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class to store all relevant instrument data and references.
/// </summary>
public class Instrument : IComparable {

	#region Instrument Enums

	/// <summary>
	/// Base type of instrument.
	/// </summary>
	public enum Type {
		Percussion,
		Melodic
	};

	/// <summary>
	/// Instrument family.
	/// </summary>
	public enum Family {
		Percussion,
		Guitar,
		Bass,
		Keyboard,
		Brass
	};

	#endregion
	#region Instrument Vars

	public string name;                            // User-friendly name
	public string codeName;                        // Name in code
	public int index;                              // Game index
	public Type type;                              // Instrument type
	public Family family;                          // Instrument family

	public Sprite icon;                            // Icon sprite
	protected string iconPath;                     // Path from which to load icon

	public Sprite glow;                            // Glow sprite
	protected string glowPath;                     // Path from which to load glow sprite

	public AudioClip switchSound;                  // Sound to play when switched to in live mode
	protected string switchSoundPath;              // Path from which to load switch sound

	public static List<Instrument> AllInstruments; // List of all instruments available in the game

	#endregion
	#region Instrument Methods

	/// <summary>
	/// Loads all resources associated with an instrument.
	/// </summary>
	public virtual void Load () {
		icon = Resources.Load<Sprite>(iconPath);
		glow = Resources.Load<Sprite>(glowPath);
		switchSound = Resources.Load<AudioClip>(switchSoundPath);
	}

	/// <summary>
	/// Loads all instruments.
	/// </summary>
	public static void LoadInstruments () {

		// Populate list of instruments
		AllInstruments = new List<Instrument> () {
			PercussionInstrument.RockDrums,
			PercussionInstrument.ExoticPercussion,
			MelodicInstrument.ElectricGuitar,
			MelodicInstrument.ElectricBass,
			MelodicInstrument.AcousticGuitar,
			MelodicInstrument.ClassicalGuitar,
			MelodicInstrument.PipeOrgan,
			MelodicInstrument.Keyboard,
			MelodicInstrument.Trumpet
		};

		// Load all instruments
		foreach (Instrument instrument in AllInstruments)
			instrument.Load();
	}

	public int CompareTo (object obj) {
		if (obj == null) return 1;

		Instrument other = obj as Instrument;
		if (other != null) return this.index.CompareTo (other.index);
		else throw new ArgumentException ("Argument is not an instrument.");
		
	}

	#endregion
}
