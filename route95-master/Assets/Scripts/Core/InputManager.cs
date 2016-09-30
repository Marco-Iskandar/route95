using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Instanced class to handle all keyboard and mouse input.
/// </summary>
public class InputManager : MonoBehaviour {

	#region InputManager Vars

	public static InputManager instance;

	[Header("InputManager Values")]

	[Tooltip("Current selected object.")]
	public GameObject selected;

	[Tooltip("List of scroll views to disable while dragging.")]
	public List<ScrollRect> scrollviews;

	Vector3 prevMouse = Vector3.zero;    // Mouse position during last frame

	[NonSerialized]
	public Vector3 mouseDelta;           // Change in mouse position between last frame and this frame

	Vector3 clickPosition;               // Position clicked
	List<AudioSource> audioSources;      // List of all note audiosources

	[NonSerialized]
	public int framesDragged = 0;        // Number of frames dragged

	[Tooltip("Number of frames user must hold click to be considered a drag.")]
	public int dragThreshold = 30; 

	#endregion
	#region Key Mappings

	// Mappings of number keys to instruments
	public static Dictionary<KeyCode, Instrument> keyToInstrument = new Dictionary<KeyCode, Instrument>() {
		{ KeyCode.Alpha1, PercussionInstrument.RockDrums },
		{ KeyCode.Alpha2, PercussionInstrument.ExoticPercussion },
		{ KeyCode.Alpha3, MelodicInstrument.ElectricGuitar },
		{ KeyCode.Alpha4, MelodicInstrument.ElectricBass },
		{ KeyCode.Alpha5, MelodicInstrument.AcousticGuitar },
		{ KeyCode.Alpha6, MelodicInstrument.ClassicalGuitar },
		{ KeyCode.Alpha7, MelodicInstrument.PipeOrgan },
		{ KeyCode.Alpha8, MelodicInstrument.Keyboard },
		{ KeyCode.Alpha9, MelodicInstrument.Trumpet }
	};
		
	// Mappings of keys to notes
	public static Dictionary<KeyCode, int> keyToNote = new Dictionary<KeyCode, int>() {
		{ KeyCode.P, 0 },
		{ KeyCode.O, 1 },
		{ KeyCode.I, 2 },
		{ KeyCode.U, 3 },
		{ KeyCode.Y, 4 },
		{ KeyCode.T, 5 },
		{ KeyCode.R, 6 },
		{ KeyCode.E, 7 },
		{ KeyCode.W, 8 },
		{ KeyCode.Q, 9 },
		{ KeyCode.L, 10 },
		{ KeyCode.K, 11 },
		{ KeyCode.J, 12 },
		{ KeyCode.H, 13 },
		{ KeyCode.G, 14 },
		{ KeyCode.F, 15 },
		{ KeyCode.D, 16 },
		{ KeyCode.S, 17 },
		{ KeyCode.A, 18 },
		{ KeyCode.M, 19 },
		{ KeyCode.N, 20 },
		{ KeyCode.B, 21 },
		{ KeyCode.V, 22 },
		{ KeyCode.C, 23 },
		{ KeyCode.X, 24 },
		{ KeyCode.Z, 25 }
	};

	#endregion
	#region Unity Callbacks

	void Awake () {
		instance = this;

		// Initialize audio sources for all notes
		audioSources = new List<AudioSource>();
		Transform tr = transform;
		for (int i=0; i<26; i++) {

			// Create GameObject
			GameObject obj = new GameObject("Note"+i+"Source");
			AudioSource temp = obj.AddComponent<AudioSource>();
			audioSources.Add(temp);

			// Parent to InputManager (for easy hiding)
			obj.transform.parent = tr;
			
			// Set default volume
			temp.volume = 1.0f;
		}
	}

	void Update() {

		// Update mouse delta
		mouseDelta = Input.mousePosition - prevMouse;

		switch (GameManager.instance.currentState) {

			// Live mode
			case GameManager.State.Live:

				Instrument inst = MusicManager.instance.currentInstrument;

				// If available, get current playing song
				Song song = null;
				if (MusicManager.instance.currentProject.songs.Count > 0) 
					song = MusicManager.instance.currentProject.songs[MusicManager.instance.currentPlayingSong];

				// Check for pause
				if (Input.GetKeyDown (KeyCode.Escape)) GameManager.instance.TogglePause ();

				// Check for tempo up/down
				else if (Input.GetKeyDown (KeyCode.UpArrow)) MusicManager.instance.IncreaseTempo ();
				else if (Input.GetKeyDown (KeyCode.DownArrow))  MusicManager.instance.DecreaseTempo ();

				// Check for instrument volume up/down
				else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
					AudioSource source = MusicManager.instance.instrumentAudioSources[inst];
					if (source.volume >= 0.1f) source.volume -= 0.1f;
				} else if (Input.GetKeyDown (KeyCode.RightArrow)) {
					AudioSource source = MusicManager.instance.instrumentAudioSources[inst];
					if (source.volume <= 0.9f) source.volume += 0.1f;
				}

				else {
				
					// Check for instruments switch
					foreach (KeyValuePair<KeyCode, Instrument> key in keyToInstrument) {
						if (Input.GetKeyDown(key.Key)) {
							SwitchInstrument(keyToInstrument[key.Key]);
							GameManager.instance.WakeLiveUI();
						}
					}
					
					// Check for note presses
					foreach (KeyCode keyPress in keyToNote.Keys.ToList()) {
						if (Input.GetKeyDown(keyPress)) {
							
							int noteIndex;
							AudioSource source = audioSources[keyToNote[keyPress]];

							// If percussion is selected
							if (inst.type == Instrument.Type.Percussion) {
								noteIndex = KeyManager.instance.percussionSets[inst].Count-1-keyToNote[keyPress];
								if (noteIndex >= 0) {
									Note note = new Note(KeyManager.instance.percussionSets[inst][noteIndex]);
									note.PlayNote(source, 1f, false);

									// If snare, cause lightning strike
									if (note.IsSnare()) WorldManager.instance.LightningStrike(note.volume * source.volume);

									// If kick or tom, cause lightning flash
									else if (note.IsKick()) WorldManager.instance.LightningFlash(note.volume * source.volume);
									else if (note.IsTom()) WorldManager.instance.LightningFlash(0.75f * note.volume * source.volume);

									// If shaker, increase rain density
									else if (note.IsShaker()) WorldManager.instance.shakers++;

									// If hat, create stars
									else if (note.IsHat()) WorldManager.instance.StarBurst();

									// If cymbal, create shooting star
									else if (note.IsCymbal()) WorldManager.instance.ShootingStar();

									// If wood, create exhaust puff
									else if (note.IsWood()) WorldManager.instance.ExhaustPuff();
								}

							// If melodic is selected (must be a valid song)
							} else if (song != null && song.scale != -1 && song.key != Key.None){

								Key key = MusicManager.instance.currentSong.key;
								ScaleInfo scale = ScaleInfo.AllScales[MusicManager.instance.currentSong.scale];

								noteIndex = KeyManager.instance.scales[key][scale][(MelodicInstrument)inst].allNotes.Count - 1 - keyToNote[keyPress];
								if (noteIndex >= 0) {
									Note note = new Note(KeyManager.instance.scales[key][scale][(MelodicInstrument)inst].allNotes[noteIndex]);
									if (note != null) note.PlayNote(source, 1f, true);
								}

								// If electric bass
								if (inst.codeName == "ElectricBass") {

									// Cause terrain deformation
									WorldManager.instance.DeformRandom();

								// All other melodic instruments
								} else {
									switch (inst.family) {
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

						// If key released, stop note
						} else if (Input.GetKeyUp(keyPress) && inst.type != Instrument.Type.Percussion)
							audioSources[keyToNote[keyPress]].Stop();
					} 
				}
				break;

			// Setup mode
			case GameManager.State.Setup:

				// If left mouse button released
				if (Input.GetMouseButtonUp(0)) {
					if (selected != null) {
					
						// Call OnMouseUp() if possible
						if (selected.GetComponent<DraggableButton>() != null)
							selected.GetComponent<DraggableButton>().OnMouseUp();
					}
				
					// Clear selected
					selected = null;
				
					// Unfreeze scroll views
					UnfreezeAllScrollviews();
				
					// Reset click position
					clickPosition = Vector3.zero;
				
				// If left mouse button clicked
				} else if (Input.GetMouseButtonDown(0)) {
				
					// Get click position
					clickPosition = Input.mousePosition;
				
					// Get selected object from EventSystem
					selected = EventSystem.current.currentSelectedGameObject;
				
					// If something was clicked
					if (selected != null) {
					
						// Freeze scroll views if necessary
						if (selected.tag == "StopScrolling") FreezeAllScrollviews();
							
						// Call OnMouseDown() if possible
						if (selected.GetComponent<DraggableButton>() != null)
							selected.GetComponent<DraggableButton>().OnMouseDown();
					}

				// If mouse button held down or not down
				} else {

					// If held down on an object
					if (selected) {

						// Increment frames dragged
						framesDragged++;

						// Call Drag() if possible
						if (selected.GetComponent<DraggableButton>() != null)
							selected.GetComponent<DraggableButton>().Drag(Input.mousePosition - clickPosition);

					// If no object, reset frames dragged
					} else framesDragged = 0;
				}

				break;
		}

		// If in free camera mode
		if (CameraControl.instance.state == CameraControl.State.Free) {

			// Space -> start car movement
			if (Input.GetKeyDown (KeyCode.Space)) GameManager.instance.SwitchToLive();

			// Left/Right -> adjust time of day
			else if (Input.GetKeyDown (KeyCode.LeftArrow)) WorldManager.instance.timeOfDay -= Mathf.PI/16f;
			else if (Input.GetKeyDown (KeyCode.RightArrow)) WorldManager.instance.timeOfDay += Mathf.PI/16f;
		}

		// Update prevMouse
		prevMouse = Input.mousePosition;
	}

	#endregion
	#region InputManager Methods

	/// <summary>
	/// Returns whether or not the user is dragging.
	/// </summary>
	public bool IsDragging
	{
		get
		{
			return framesDragged >= dragThreshold;
		}
	}

	/// <summary>
	/// Freezes all selected scrollviews.
	/// </summary>
	void FreezeAllScrollviews () {
		foreach (ScrollRect scrollview in scrollviews)
			scrollview.enabled = false;
	}

	/// <summary>
	/// Unfreezes all selected scrollviews.
	/// </summary>
	void UnfreezeAllScrollviews () {
		foreach (ScrollRect scrollview in scrollviews)
			scrollview.enabled = true;
	}

	/// <summary>
	/// Switches the instrument.
	/// </summary>
	/// <param name="instrument">Instrument.</param>
	void SwitchInstrument (Instrument instrument) {
		Instrument inst = MusicManager.instance.currentInstrument;
		if (instrument != inst) {
			InstrumentDisplay.instance.FadeGlow (inst.index);
			MusicManager.instance.currentInstrument = instrument;
			InstrumentDisplay.instance.WakeGlow(instrument.index);
			MusicManager.PlayMenuSound(instrument.switchSound);
			InstrumentDisplay.instance.Refresh();
		}
	}

	#endregion
}
