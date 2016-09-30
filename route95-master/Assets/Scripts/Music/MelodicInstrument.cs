using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class to store all melodic instrument data.
/// </summary>
public class MelodicInstrument : Instrument {

	#region MelodicInstrument Vars

	public Dictionary<Key, int> startingNote; // Mapping of musical key to index of starting note

	#endregion
	#region MelodicInstrument Methods

	/// <summary>
	/// Electric guitar.
	/// </summary>
	public static MelodicInstrument ElectricGuitar = new MelodicInstrument {
		name = "Electric Guitar",
		codeName = "ElectricGuitar",
		index = 2,
		type = Type.Melodic,
		family = Family.Guitar,
		iconPath = "UI/Instrument_ElectricGuitar",
		glowPath = "UI/Instrument_ElectricGuitar_Glow",
		switchSoundPath = "Audio/Gameplay/Instruments/ElectricGuitar",
		startingNote = new Dictionary<Key, int> () {
			{ Key.C, 8 },
			{ Key.CSharp, 9 },
			{ Key.D, 10 },
			{ Key.DSharp, 11 },
			{ Key.E, 0 },
			{ Key.F, 1 },
			{ Key.FSharp, 2 },
			{ Key.G, 3 },
			{ Key.GSharp, 4 },
			{ Key.A, 5 },
			{ Key.ASharp, 6 },
			{ Key.B, 7 }
		}
	};

	/// <summary>
	/// Electric bass.
	/// </summary>
	public static MelodicInstrument ElectricBass = new MelodicInstrument {
		name = "Electric Bass",
		codeName = "ElectricBass",
		index = 3,
		type = Type.Melodic,
		family = Family.Bass,
		iconPath = "UI/Instrument_ElectricBass",
		glowPath = "UI/Instrument_ElectricBass_Glow",
		switchSoundPath = "Audio/Gameplay/Instruments/ElectricBass",
		startingNote = new Dictionary<Key, int> () {
			{ Key.C, 8 },
			{ Key.CSharp, 9 },
			{ Key.D, 10 },
			{ Key.DSharp, 11 },
			{ Key.E, 0 },
			{ Key.F, 1 },
			{ Key.FSharp, 2 },
			{ Key.G, 3 },
			{ Key.GSharp, 4 },
			{ Key.A, 5 },
			{ Key.ASharp, 6 },
			{ Key.B, 7 }
		}
	};

	/// <summary>
	/// Acoustic guitar.
	/// </summary>
	public static MelodicInstrument AcousticGuitar = new MelodicInstrument {
		name = "Acoustic Guitar",
		codeName = "AcousticGuitar",
		index = 4,
		type = Type.Melodic,
		family = Family.Guitar,
		iconPath = "UI/Instrument_AcousticGuitar",
		glowPath = "UI/Instrument_AcousticGuitar_Glow",
		switchSoundPath = "Audio/Gameplay/Instruments/ElectricGuitar",
		startingNote = new Dictionary<Key, int> () {
			{ Key.C, 8 },
			{ Key.CSharp, 9 },
			{ Key.D, 10 },
			{ Key.DSharp, 11 },
			{ Key.E, 0 },
			{ Key.F, 1 },
			{ Key.FSharp, 2 },
			{ Key.G, 3 },
			{ Key.GSharp, 4 },
			{ Key.A, 5 },
			{ Key.ASharp, 6 },
			{ Key.B, 7 }
		}
	};

	/// <summary>
	/// Classical guitar.
	/// </summary>
	public static MelodicInstrument ClassicalGuitar = new MelodicInstrument {
		name = "Classical Guitar",
		codeName = "ClassicalGuitar",
		index = 5,
		type = Type.Melodic,
		family = Family.Guitar,
		iconPath = "UI/Instrument_ClassicalGuitar",
		glowPath = "UI/Instrument_ClassicalGuitar_Glow",
		switchSoundPath = "Audio/Gameplay/Instruments/ElectricGuitar",
		startingNote = new Dictionary<Key, int> () {
			{ Key.C, 8 },
			{ Key.CSharp, 9 },
			{ Key.D, 10 },
			{ Key.DSharp, 11 },
			{ Key.E, 0 },
			{ Key.F, 1 },
			{ Key.FSharp, 2 },
			{ Key.G, 3 },
			{ Key.GSharp, 4 },
			{ Key.A, 5 },
			{ Key.ASharp, 6 },
			{ Key.B, 7 }
		}
	};

	/// <summary>
	/// Pipe organ.
	/// </summary>
	public static MelodicInstrument PipeOrgan = new MelodicInstrument {
		name = "Pipe Organ",
		codeName = "PipeOrgan",
		index = 6,
		type = Type.Melodic,
		family = Family.Keyboard,
		iconPath = "UI/Instrument_PipeOrgan",
		glowPath = "UI/Instrument_PipeOrgan_Glow",
		switchSoundPath = "Audio/Gameplay/Instruments/ElectricGuitar",
		startingNote = new Dictionary<Key, int> () {
			{ Key.C, 3 },
			{ Key.CSharp, 4 },
			{ Key.D, 5 },
			{ Key.DSharp, 6 },
			{ Key.E, 7 },
			{ Key.F, 8 },
			{ Key.FSharp, 9 },
			{ Key.G, 10 },
			{ Key.GSharp, 11 },
			{ Key.A, 0 },
			{ Key.ASharp, 1 },
			{ Key.B, 2 }
		}
	};

	/// <summary>
	/// Keyboard.
	/// </summary>
	public static MelodicInstrument Keyboard = new MelodicInstrument {
		name = "Keyboard",
		codeName = "Keyboard",
		index = 7,
		type = Type.Melodic,
		family = Family.Keyboard,
		iconPath = "UI/Instrument_Keyboard",
		glowPath = "UI/Instrument_Keyboard_Glow",
		switchSoundPath = "Audio/Gameplay/Instruments/ElectricGuitar",
		startingNote = new Dictionary<Key, int> () {
			{ Key.C, 3 },
			{ Key.CSharp, 4 },
			{ Key.D, 5 },
			{ Key.DSharp, 6 },
			{ Key.E, 7 },
			{ Key.F, 8 },
			{ Key.FSharp, 9 },
			{ Key.G, 10 },
			{ Key.GSharp, 11 },
			{ Key.A, 0 },
			{ Key.ASharp, 1 },
			{ Key.B, 2 }
		}
	};

	/// <summary>
	/// Trumpet.
	/// </summary>
	public static MelodicInstrument Trumpet = new MelodicInstrument {
		name = "Trumpet",
		codeName = "Trumpet",
		index = 8,
		type = Type.Melodic,
		family = Family.Brass,
		iconPath = "UI/Instrument_Trumpet",
		glowPath = "UI/Instrument_Trumpet_Glow",
		switchSoundPath = "Audio/Gameplay/Instruments/ElectricGuitar",
		startingNote = new Dictionary<Key, int> () {
			{ Key.C, 3 },
			{ Key.CSharp, 4 },
			{ Key.D, 5 },
			{ Key.DSharp, 6 },
			{ Key.E, 7 },
			{ Key.F, 8 },
			{ Key.FSharp, 9 },
			{ Key.G, 10 },
			{ Key.GSharp, 11 },
			{ Key.A, 0 },
			{ Key.ASharp, 1 },
			{ Key.B, 2 }
		}
	};

	#endregion

}