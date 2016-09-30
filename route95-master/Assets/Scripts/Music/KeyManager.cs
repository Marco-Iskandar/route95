using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Instanced MonoBehaviour to handle all key to scale to 
/// note operations.
/// </summary>
public class KeyManager : MonoBehaviour {

	#region KeyManager Vars

	public static KeyManager instance;

	// Mappings of keys to scales to instruments
	public Dictionary<Key, Dictionary<ScaleInfo, Dictionary<MelodicInstrument, Scale>>> scales;

	// Mapping of percussion instruments to notes
	public Dictionary<Instrument, List<string>> percussionSets;

	#endregion
	#region Unity Callbacks

	void Awake () {
		instance = this;
	}

	#endregion
	#region KeyManager Methods

	/// <summary>
	/// Begins building scales.
	/// </summary>
	public void DoBuildScales () {
		StartCoroutine ("BuildScales");
	}

	/// <summary>
	/// Coroutine to build scales.
	/// </summary>
	/// <returns></returns>
	IEnumerator BuildScales () {

		// Update loading message
		GameManager.instance.ChangeLoadingMessage("Loading scales...");

		// Mark start time
		float startTime = Time.realtimeSinceStartup;
		int numLoaded = 0;

		// Build percussion sets
		percussionSets = new Dictionary<Instrument, List<string>>() {
			{ PercussionInstrument.RockDrums, Sounds.soundsToLoad["RockDrums"] },
			{ PercussionInstrument.ExoticPercussion, Sounds.soundsToLoad["ExoticPercussion"] }
		};

		// Init scales dict
		scales = new Dictionary<Key, Dictionary<ScaleInfo, Dictionary<MelodicInstrument, Scale>>>();

		// For reach key
		foreach (Key key in Enum.GetValues(typeof(Key))) {

			// Skip Key.None
			if (key == Key.None) continue;

			// Add key to dictionary mapping
			scales.Add (key, new Dictionary<ScaleInfo, Dictionary<MelodicInstrument, Scale>>());

			// For reach scale type
			foreach (ScaleInfo scale in ScaleInfo.AllScales) {

				// Add scale to melodic instrument mapping
				scales[key].Add (scale, new Dictionary<MelodicInstrument, Scale>());

				// For each instrument
				foreach (Instrument instrument in Instrument.AllInstruments) {

					// Skip percussion instruments
					if (instrument.type == Instrument.Type.Percussion) continue;
			
					// Add instrument to sscale mapping
					scales[key][scale].Add (
						(MelodicInstrument)instrument, BuildScale (Sounds.soundsToLoad[instrument.codeName], 
							scale, ((MelodicInstrument)instrument).startingNote[key])
					);

					numLoaded++;

					// If over time, skip until next frame
					if (Time.realtimeSinceStartup - startTime > GameManager.instance.targetDeltaTime) {
						yield return null;
						startTime = Time.realtimeSinceStartup;
						GameManager.instance.ReportLoaded(numLoaded);
				
						numLoaded = 0;
					}

				}
			}
		}

		// Finish loading
		MusicManager.instance.FinishLoading();
		yield return null;

	}

	/// <summary>
	/// Builds a scale.
	/// </summary>
	/// <param name="soundFiles">List of sounds to draw from.</param>
	/// <param name="scale">Scale type.</param>
	/// <param name="startIndex">Index of sound to start with.</param>
	/// <returns>A scale.</returns>
	public static Scale BuildScale (List<string> soundFiles, ScaleInfo scale, int startIndex) {
		Scale result = new Scale ();
		int i = startIndex;
		try {
			while (i < soundFiles.Count) {
				// Add root/octave
				result.root.Add (soundFiles [i]);
				result.allNotes.Add (soundFiles[i]);

				// Add second
				i += scale.secondIndex;
				result.second.Add (soundFiles [i]);
				result.allNotes.Add (soundFiles[i]);

				// Add third
				i += scale.thirdIndex;
				result.third.Add (soundFiles [i]);
				result.allNotes.Add (soundFiles[i]);

				// Add fourth
				i += scale.fourthIndex;
				result.fourth.Add (soundFiles [i]);
				result.allNotes.Add (soundFiles[i]);

				// Add fifth
				i += scale.fifthIndex;
				result.fifth.Add (soundFiles [i]);
				result.allNotes.Add (soundFiles[i]);

				// Add sixth
				i += scale.sixthIndex;
				result.sixth.Add (soundFiles [i]);
				result.allNotes.Add (soundFiles[i]);

				// Add seventh
				i += scale.seventhIndex;
				result.seventh.Add (soundFiles [i]);
				result.allNotes.Add (soundFiles[i]);

				// Go to next octave
				i += scale.rootIndex;
			}
			return result;
		} catch (ArgumentOutOfRangeException) {
			return result;
		} catch (NullReferenceException n) {
			Debug.LogError (n);
			return result;
		}
	}

	#endregion
	  	  
}
	