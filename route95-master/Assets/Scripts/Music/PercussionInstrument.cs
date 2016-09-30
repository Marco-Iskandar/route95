using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class to hold percussion instrument data.
/// </summary>
public class PercussionInstrument : Instrument {

	#region Percussioninstrument Vars

	public Dictionary <string, Sprite> icons; // Dictionary of individual drums to icons
	Dictionary <string, string> iconPaths;    // Paths from which to load icons

	#endregion
	#region PercussionInstrument Methods

	/// <summary>
	/// Loads this percussion instrument.
	/// </summary>
	public override void Load () {

		// Load instrument data
		base.Load();

		// Load drum icons
		icons = new Dictionary<string, Sprite>();
		foreach (string path in iconPaths.Keys) {
			Sprite sprite = Resources.Load<Sprite>(iconPaths[path]);

			if (sprite == null) {
				Debug.LogError ("PercussionInstrument.Load(): failed to load icon "+iconPaths[path]);
				continue;
			}

			icons.Add (path, sprite);
		}
	}

	#endregion
	#region PercussionInstrument Instruments

	/// <summary>
	/// Rock drums.
	/// </summary>
	public static PercussionInstrument RockDrums = new PercussionInstrument {
		name = "Rock Drums",
		codeName = "RockDrums",
		index = 0,
		type = Type.Percussion,
		family = Family.Percussion,
		iconPath = "UI/Instrument_RockDrums",
		glowPath = "UI/Instrument_RockDrums_Glow",
		switchSoundPath = "Audio/Gameplay/Instruments/RockDrums",
		iconPaths = new Dictionary <string, string> () {
			{ "Audio/Instruments/Percussion/RockDrums/RockDrums_Kick", "UI/Percussion_Kick" },
			{ "Audio/Instruments/Percussion/RockDrums/RockDrums_Snare", "UI/Percussion_Snare" },
			{ "Audio/Instruments/Percussion/RockDrums/RockDrums_LowTom", "UI/Percussion_Tom" },
			{ "Audio/Instruments/Percussion/RockDrums/RockDrums_MidTom", "UI/Percussion_Tom" },
			{ "Audio/Instruments/Percussion/RockDrums/RockDrums_HiTom", "UI/Percussion_Tom" },
			{ "Audio/Instruments/Percussion/RockDrums/RockDrums_Hat", "UI/Percussion_Hat" },
			{ "Audio/Instruments/Percussion/RockDrums/RockDrums_Crash", "UI/Percussion_Hat" }
		}
	};

	/// <summary>
	/// Exotic percussion.
	/// </summary>
	public static PercussionInstrument ExoticPercussion = new PercussionInstrument {
		name = "Exotic Percussion",
		codeName = "ExoticPercussion",
		index = 1,
		type = Type.Percussion,
		family = Family.Percussion,
		iconPath = "UI/Instrument_ExoticPercussion",
		glowPath = "UI/Instrument_ExoticPercussion",
		switchSoundPath = "Audio/Gameplay/Instruments/RockDrums",
		iconPaths = new Dictionary<string, string> () {
			{ "Audio/Instruments/Percussion/ExoticPercussion/ExoticPercussion_Castinets","UI/Percussion_Castinets" },
			{ "Audio/Instruments/Percussion/ExoticPercussion/ExoticPercussion_Claves", "UI/Percussion_Claves" },
			{ "Audio/Instruments/Percussion/ExoticPercussion/ExoticPercussion_Cowbell", "UI/Percussion_Cowbell" },
			{ "Audio/Instruments/Percussion/ExoticPercussion/ExoticPercussion_Cowbell2", "UI/Percussion_Cowbell" },
			{ "Audio/Instruments/Percussion/ExoticPercussion/ExoticPercussion_JamBlock", "UI/Percussion_JamBlock" },
			{ "Audio/Instruments/Percussion/ExoticPercussion/ExoticPercussion_Maracas1", "UI/Percussion_Maracas" },
			{ "Audio/Instruments/Percussion/ExoticPercussion/ExoticPercussion_Maracas2", "UI/Percussion_Maracas" },
			{ "Audio/Instruments/Percussion/ExoticPercussion/ExoticPercussion_Tambourine", "UI/Percussion_Tambourine" }
		}
	};

	#endregion
			
}
