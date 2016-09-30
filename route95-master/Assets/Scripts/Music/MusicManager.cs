using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;// need for using lists
using System.IO; // need for path operations
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// All musical KeyManager.instance.
/// </summary>
public enum Key {
	None,
	C,
	CSharp,
	D,
	DSharp,
	E,
	F,
	FSharp,
	G,
	GSharp,
	A,
	ASharp,
	B
};

/// <summary>
/// All tempos.
/// </summary>
public enum Tempo {
	Slowest,
	Slower,
	Slow,
	Medium,
	Fast,
	Faster,
	Fastest,
	NUM_TEMPOS
};

/// <summary>
/// Instanced MonoBehaviour class to manage all music-related operations.
/// </summary>
public class MusicManager : MonoBehaviour {

	#region Sound Struct

	/// <summary>
	/// Struct to hold all relevant sound and playback data.
	/// </summary>
	public struct Sound {
		public AudioClip clip;     // AudioClip to play
		public AudioSource source; // AudioSource to use for playback
		public float volume;       // Note volume
		public float delay;        // Note delay
	}

	#endregion
	#region MusicManager Vars

	//-----------------------------------------------------------------------------------------------------------------
	[Header("MusicManager Status")]
	
	public static MusicManager instance;

	AudioSource source;                                                // Global MM audio source

	float startLoadTime;                                               // Time at which loading started
	int loadProgress;                                                  // Number of load tasks
	public int loadsToDo;                                              // Number of tasks to load

	[Tooltip("Is this playing right now?")]
	public bool playing = false;

	[Tooltip("Loop riff?")]
	public bool loop = false;

	[Tooltip("Loop playlist?")]
	public bool loopPlaylist = false;

	public Instrument currentInstrument = 
		MelodicInstrument.ElectricGuitar;                              // Current instrument in live mode

	[Tooltip("Current tempo.")]
	public Tempo tempo = Tempo.Medium;

	public static Dictionary<Tempo, float> tempoToFloat =              // Mappings of tempos to values
		new Dictionary<Tempo, float> () {
		{ Tempo.Slowest, 50f },
		{ Tempo.Slower, 70f },
		{ Tempo.Slow, 90f },
		{ Tempo.Medium, 110f },
		{ Tempo.Fast, 130f },
		{ Tempo.Faster, 150f },
		{ Tempo.Fastest, 170f }
	};

	[Tooltip("Index of currently playing song.")]
	public int currentPlayingSong;

	[Tooltip("Current beat.")]
	[SerializeField]
	private int beat;

	private float BeatTimer;                                           // Countdown to next beat

	[Tooltip("Number of beats elapsed in current song.")]
	public int beatsElapsedInCurrentSong = 0;

	[Tooltip("Number of beats elapsed in current playlist.")]
	public int beatsElapsedInPlaylist = 0;

	[Tooltip("Number of elapsed guitar notes in current song.")]
	public int guitarNotes = 0;

	[Tooltip("Current density of guitar notes.")]
	[SerializeField]
	float guitarDensity = 0f;

	[Tooltip("Number of elapsed keyboard notes in current song.")]
	public int keyboardNotes = 0;

	[Tooltip("Current density of keyboard notes.")]
	[SerializeField]
	float keyboardDensity = 0f;

	[Tooltip("Number of elapsed brass notes in current song.")]
	public int brassNotes = 0;

	[Tooltip("Current density of brass notes.")]
	[SerializeField]
	float brassDensity = 0f;

	//-----------------------------------------------------------------------------------------------------------------
	[Header("Object References")]

	[Tooltip("Mixer to use for MusicManager.instance.")]
	public AudioMixer mixer;

	[Tooltip("AudioSource to use for UI sounds.")]
	public AudioSource OneShot;

	[Tooltip("AudioSource to use for UI riff playback.")]
	public AudioSource LoopRiff;

	[Tooltip("Loop playlist button sprite.")]
	public Image loopPlaylistButton;

	//-----------------------------------------------------------------------------------------------------------------
	[Header("Project References")]

	[Tooltip("Current open project.")]
	//[NonSerialized]
	public Project currentProject;

	[Tooltip("Current song being edited/played.")]
	//[NonSerialized]
	public Song currentSong;

	//-----------------------------------------------------------------------------------------------------------------
	[Header("Sounds")]

	public static Dictionary<string, AudioClip> SoundClips = 
		new Dictionary<string, AudioClip>();                           // Holds all loaded sounds

	[NonSerialized]
	public Dictionary<Instrument, AudioSource> instrumentAudioSources; // Mapping of instruments to AudioSources

	public bool riffMode = true;

	#endregion
	#region Unity Callbacks

	void Awake () {

		// Init instance
		if (instance) Debug.LogError("More than one MusicManager exists!");
		else instance = this;

		source = GetComponent<AudioSource>();

		// Load instrument lists
		Instrument.LoadInstruments();

		// Calculate number of objects to load
		loadsToDo = Sounds.soundsToLoad.Count + Instrument.AllInstruments.Count +
			Instrument.AllInstruments.Count * (Enum.GetValues(typeof(Key)).Length-1) * ScaleInfo.AllScales.Count;
	}

	void FixedUpdate() {

		// Return if not playing or game is paused
		if (!playing) return;
		if (GameManager.instance.paused) return;

		// If new beat
		if (BeatTimer <= 0f) {
			switch (GameManager.instance.currentState) {

				// Setup mode (riff editor)
				case GameManager.State.Setup:

					if (riffMode) {

						// Play riff note
						InstrumentSetup.currentRiff.PlayRiff (beat++);

						// Wrap payback
						if (beat >= Riff.MAX_BEATS && loop) beat = 0;

						// Decrement shaker density
						WorldManager.instance.shakers -= 2;

					} else {

						if (currentSong.Beats == 0) return;

						// If song is finished
						if (beat >= currentSong.Beats && loop)
							beat = 0;

						// Play notes
						currentSong.PlaySong(beat++);


					}
					break;

				// Live mode
				case GameManager.State.Live:

					if (currentProject.songs.Count > 0) {

						// If song is finished
						if (beat >= currentSong.Beats || currentSong.Beats == 0) {
							beat = 0;

							// Reset vars
							beatsElapsedInCurrentSong = 0;
							guitarNotes = 0;
							keyboardNotes = 0;
							brassNotes = 0;

							// Reset shaker density
							WorldManager.instance.shakers = 0;
						
							// If another song available, switch
							if (currentPlayingSong < currentProject.songs.Count-1) {
								DisableAllAudioSources();
								currentPlayingSong++;
								currentSong = currentProject.songs[currentPlayingSong];

							// If no more songs to play
							} else {

								// Loop playlist if possible
								if (loopPlaylist) {
									currentPlayingSong = 0;
									beatsElapsedInPlaylist = 0;

								// Otherwise go to postplay menu
								} else GameManager.instance.SwitchToPostplay();
							}
						}

						if (currentSong.Beats == 0) return;

						// Play notes
						currentSong.PlaySong(beat);

						// Calculate song progress
						float songTotalTime = currentSong.Beats*7200f/tempoToFloat[tempo]/4f;
						float songCurrentTime = (beat*7200f/tempoToFloat[tempo]/4f) + (7200f/tempoToFloat[tempo]/4f)-BeatTimer;
						GameManager.instance.songProgressBar.GetComponent<SongProgressBar>().SetValue(songCurrentTime/songTotalTime);

						// Increment vars
						beat++;
						beatsElapsedInCurrentSong++;
						beatsElapsedInPlaylist++;

						// Update instrument densities
						guitarDensity = (float)guitarNotes/(float)beatsElapsedInCurrentSong;
						keyboardDensity = (float)keyboardNotes/(float)beatsElapsedInCurrentSong;
						brassDensity = (float)brassNotes/(float)beatsElapsedInCurrentSong;
						if (WorldManager.instance.shakers > 2) WorldManager.instance.shakers -= 2;
						WorldManager.instance.roadVariance = Mathf.Clamp(guitarDensity * 0.6f, 0.2f, 0.6f);
						WorldManager.instance.roadMaxSlope = Mathf.Clamp (keyboardDensity * 0.002f, 0.002f, 0.001f);
						WorldManager.instance.decorationDensity = Mathf.Clamp (brassDensity * 2f, 1f, 2f);
					}
					break;
				}

				// Reset beat timer
				BeatTimer = 7200f / tempoToFloat[tempo] /4f; // 3600f = 60 fps * 60 seconds

			// Decrement beat timer
			} else BeatTimer -= 1.667f;
	} 
	
	#endregion
	#region Load Methods

	/// <summary>
	/// Begins loading MusicManager.
	/// </summary>
	public void Load() {

		// Save loading start time
		startLoadTime = Time.realtimeSinceStartup;

		// Begin by loading sounds
		StartCoroutine ("LoadSounds");
	}

	/// <summary>
	/// Coroutine to load all instrument sounds.
	/// </summary>
	/// <returns></returns>
	IEnumerator LoadSounds () {

		// Update loading message
		GameManager.instance.ChangeLoadingMessage("Tuning instruments...");

		// Mark start time
		float startTime = Time.realtimeSinceStartup;
		int numLoaded = 0;

		// For each sound path
		foreach (KeyValuePair<string, List<string>> list in Sounds.soundsToLoad) {
			foreach (string path in list.Value) {

				// Load sound
				LoadAudioClip(path);
				numLoaded++;

				// If over time
				if (Time.realtimeSinceStartup - startTime > GameManager.instance.targetDeltaTime) {
					yield return null;
					startTime = Time.realtimeSinceStartup;
					GameManager.instance.ReportLoaded (numLoaded);
					numLoaded = 0;
				}
			}
		}

		// When done, start loading instruments
		yield return StartCoroutine("LoadInstruments");
	}

	/// <summary>
	/// Loads a sound.
	/// </summary>
	/// <param name="path">Sound path.</param>
	void LoadAudioClip (string path) {
		AudioClip sound = (AudioClip) Resources.Load (path);

		if (sound == null) Debug.LogError("Failed to load AudioClip at "+path);
		else SoundClips.Add (path, sound);
	}

	/// <summary>
	/// Coroutine to load instruments.
	/// </summary>
	/// <returns></returns>
	IEnumerator LoadInstruments () {

		List<string> loadMessages = new List<string>() {
			"Renting instruments...",
			"Grabbing instruments...",
			"Unpacking instruments..."
		};

		// Update loading message
		GameManager.instance.ChangeLoadingMessage(loadMessages.Random());

		// Mark start time
		float startTime = Time.realtimeSinceStartup;
		int numLoaded = 0;

		// Init audio source dict
		instrumentAudioSources = new Dictionary<Instrument, AudioSource>();

		// Foreach instrument
		for (int i=0; i<Instrument.AllInstruments.Count; i++) {

			// Load instrument data
			Instrument.AllInstruments[i].Load();

			// Create instrument AudioSource GameObject
			GameObject obj = new GameObject (Instrument.AllInstruments[i].name);
			AudioSource source = obj.AddComponent<AudioSource>();

			// Group instrument under MusicManager
			obj.transform.parent = transform.parent;

			// Connect AudioSource to mixer
			source.outputAudioMixerGroup = mixer.FindMatchingGroups (obj.name) [0];

			// Connect instrument to AudioSource
			instrumentAudioSources.Add(Instrument.AllInstruments[i], source);

			// Add distortion filter
			AudioDistortionFilter distortion = obj.AddComponent<AudioDistortionFilter> ();
			distortion.enabled = false;

			// Add tremolo filter
			AudioTremoloFilter tremolo = obj.AddComponent<AudioTremoloFilter>();
			tremolo.enabled = false;

			// Add chorus filter
			AudioChorusFilter chorus = obj.AddComponent<AudioChorusFilter> ();
			chorus.enabled = false;

			// Add flanger filter
			AudioFlangerFilter flanger = obj.AddComponent<AudioFlangerFilter>();
			flanger.enabled = false;

			// Add echo filter
			AudioEchoFilter echo = obj.AddComponent<AudioEchoFilter> ();
			echo.enabled = false;

			// Add reverb filter based on MusicManager's reverb filter
			AudioReverbFilter reverb = obj.AddComponent<AudioReverbFilter>();
			AudioReverbFilter masterReverb = GetComponent<AudioReverbFilter>();
			reverb.dryLevel = masterReverb.dryLevel;
			reverb.room = masterReverb.room;
			reverb.roomHF = masterReverb.roomHF;
			reverb.roomLF = masterReverb.roomLF;
			reverb.decayTime = masterReverb.decayTime;
			reverb.decayHFRatio = masterReverb.decayHFRatio;
			reverb.reflectionsLevel = masterReverb.reflectionsLevel;
			reverb.reflectionsDelay = masterReverb.reflectionsDelay;
			reverb.reverbLevel = masterReverb.reverbLevel;
			reverb.hfReference = masterReverb.hfReference;
			reverb.lfReference = masterReverb.lfReference;
			reverb.diffusion = masterReverb.diffusion;
			reverb.density = masterReverb.density;
			reverb.enabled = false;

			numLoaded++;

			// If over time
			if (Time.realtimeSinceStartup - startTime > GameManager.instance.targetDeltaTime) {
				yield return null;
				startTime = Time.realtimeSinceStartup;
				GameManager.instance.ReportLoaded (numLoaded);
				numLoaded = 0;
			}
		}

		// When done, start building scales
		if (instrumentAudioSources.Count == Instrument.AllInstruments.Count)
			KeyManager.instance.DoBuildScales();
		yield return null;
	}

	/// <summary>
	/// Finishes loading MusicManager.
	/// </summary>
	public void FinishLoading() {

		// Report loading time
		Debug.Log("MusicManager.Load(): finished in "+(Time.realtimeSinceStartup-startLoadTime).ToString("0.0000")+" seconds.");

		// Start loading WorldManager
		WorldManager.instance.Load();
	}

	#endregion
	#region MusicManager Callbacks

	/// <summary>
	/// Plays a one-shot AudioClip.
	/// </summary>
	/// <param name="sound">Sound to play.</param>
	/// <param name="volume">Volume scaler.</param>
	public static void PlayMenuSound (AudioClip sound, float volume=1f) {
		instance.source.PlayOneShot (sound, volume);
	}

	/// <summary>
	/// Creates a new, blank project.
	/// </summary>
	public void NewProject () {
		currentProject = new Project();
	}

	/// <summary>
	/// Saves the current project.
	/// </summary>
	public void SaveCurrentProject () {
		SaveLoad.SaveCurrentProject();
	}

	/// <summary>
	/// Creates a new blank song and adds
	/// it to the current project.
	/// </summary>
	public void NewSong () {
		currentSong = new Song();
		currentProject.songs.Add(currentSong);
	}

	/// <summary>
	/// Saves the current song.
	/// </summary>
	public void SaveCurrentSong () {
		SaveLoad.SaveCurrentSong();
	}

	/// <summary>
	/// Sets the key of the current song.
	/// </summary>
	/// <param name="key">New key (int).</param>
	public void SetKey (int key) {
		currentSong.key = (Key)key;
	}

	/// <summary>
	/// Sets the key of the current song.
	/// </summary>
	/// <param name="key">New key.</param>
	public void SetKey (Key key) {
		SetKey ((int)key);
	}

	/// <summary>
	/// Toggles whether to loop playlist.
	/// </summary>
	public void ToggleLoopPlaylist () {
		loopPlaylist = !loopPlaylist;

		// Update sprite
		if (InstrumentSetup.instance == null) Debug.Log("shit");
		loopPlaylistButton.sprite = loopPlaylist ? 
			InstrumentSetup.instance.percussionFilled : InstrumentSetup.instance.percussionEmpty;
	}

	/// <summary>
	/// Toggles looping the current riff.
	/// </summary>
	public void PlayRiffLoop(){
		SongArrangeSetup.instance.UpdateValue();
		riffMode = true;
		Loop ();
	}

	public void PlaySongLoop() {
		riffMode = false;
		Loop();
	}

	void Loop () {
		// If looping
		if (loop) {

			// Stop doing so
			StopLooping();
			
			// Stop AudioSource
			Instrument instrument = Instrument.AllInstruments[InstrumentSetup.currentRiff.instrumentIndex];
			instrumentAudioSources[instrument].Stop();

		// If not looping, then start
		} else {
			playing = true;
			loop = true;
		}
	}

	/// <summary>
	/// Stops looping the current riff.
	/// </summary>
	public void StopLooping () {
		playing = false;
		loop = false;
		beat = 0;
		Instrument instrument = Instrument.AllInstruments[InstrumentSetup.currentRiff.instrumentIndex];
		instrumentAudioSources[instrument].Stop();
	}

	/// <summary>
	/// Disables all instrument audio sources.
	/// </summary>
	void DisableAllAudioSources () {
		foreach (Instrument inst in Instrument.AllInstruments) instrumentAudioSources[inst].enabled = false;
	}

	/// <summary>
	/// Increases the tempo.
	/// </summary>
	public void IncreaseTempo () {
		if ((int)tempo < (int)Tempo.NUM_TEMPOS-1) {
			tempo = (Tempo)((int)tempo+1);
			if (InstrumentSetup.instance != null)
				InstrumentSetup.instance.UpdateTempoText();
		}
	}

	/// <summary>
	/// Decreases the tempo.
	/// </summary>
	public void DecreaseTempo () {
		if ((int)tempo > 0) {
			tempo = (Tempo)((int)tempo-1);
			if (InstrumentSetup.instance != null)
				InstrumentSetup.instance.UpdateTempoText();
		}
	}

	/// <summary>
	/// Adds a riff to the current song.
	/// </summary>
	/// <returns></returns>
	public Riff AddRiff () {

		// Create a new riff
		Riff temp = new Riff ();

		// Register the riff with the current song
		currentSong.RegisterRiff(temp);

		// Update riff editor
		InstrumentSetup.currentRiff = temp;
		
		// Update song arrange
		SongArrangeSetup.instance.selectedRiffIndex = temp.index;
		SongArrangeSetup.instance.Refresh();

		return temp;
	}

	/// <summary>
	/// Starts playing a song.
	/// </summary>
	public void StartSong () {
		playing = true;
	}

	/// <summary>
	/// Starts playing a playlist.
	/// </summary>
	public void StartPlaylist() {
		beatsElapsedInPlaylist = 0;
	}

	/// <summary>
	/// Stops playing a song.
	/// </summary>
	public void StopPlaying () {
		playing = false;
		beat = 0;
	}
	
	#endregion
}