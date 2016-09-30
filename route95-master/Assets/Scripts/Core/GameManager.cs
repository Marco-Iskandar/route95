using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Instanced class to handle all application functions,
/// major state changes, and UI interactions.
/// </summary>
public class GameManager : MonoBehaviour {

	#region GameManager Enums

	/// <summary>
	/// Enum for various game states.
	/// </summary>
	public enum State {
		Loading,
		Setup,
		Live,
		Postplay
	};

	#endregion
	#region GameManager Vars

	public static GameManager instance;

	[Header("Game Status")]

	[Tooltip("Is the game paused?")]
	public bool paused = false;

	[Tooltip("Current game state.")]
	public State currentState = State.Loading;

	[NonSerialized]
	public float targetDeltaTime;                  // 1 / Target FPS -- useful for coroutines

	//-----------------------------------------------------------------------------------------------------------------
	[Header("Load Status")]

	private int loadProgress = 0;                  // Number of load operations performed
	private int loadsToDo;                         // Number of load operations to perform

	[Tooltip("Is the game loaded?")]
	public bool loaded = false;

	public bool loading = false;                          // Is the game currently loading?

	float startLoadTime;                           // Time at which loading started
	public GameObject loadingScreen;               // Parent loading screen object
	public GameObject loadingBar;                  // Loading bar
	public GameObject loadingMessage;              // Message above loading bar

	//-----------------------------------------------------------------------------------------------------------------
	[Header("Casette Settings")]

	public GameObject casette;                     // Casette GameObject
	bool casetteMoving = false;                    // Is the casette currently moving?
	bool willMoveCasette = false;                  // Will the casette move after camera lerp?

	[Tooltip("Speed at which the casette moves.")]
	[Range(0.5f,2f)]
	public float casetteMoveSpeed = 1f;

	[Tooltip("Position for casette to move in front of camera.")]
	public Transform casetteFront;  

	[Tooltip("Position for casette to move behind camera.")]
	public Transform casetteBack;

	Transform casetteTarget;                       // Current casette lerp target
	Transform casettePosition;                     // Current casette lerp start position

	[SerializeField]
	float progress;                                // Start time of casette lerp

	//-----------------------------------------------------------------------------------------------------------------
	[Header("UI Settings")]

	[Tooltip("(Live Mode) How long to wait before fading the instrument icons.")]
	public float fadeWaitTime;

	[Tooltip("(Live Mode) How quickly to fade the instrument icons.")]
	public float fadeSpeed;

	float fadeTimer;                               // Current fade timer
	Vector3 prevMouse = Vector3.zero;              // Position of mouse during last frame

	//-----------------------------------------------------------------------------------------------------------------
	[Header("IO Settings")]
	public string projectSaveFolder = "Projects/"; // Name of the folder in which to save projects
	public string songSaveFolder = "Songs/";       // Name of the folder in which to save songs

	[NonSerialized]
	public string projectSavePath;                 // Full path to which to save projects

	[NonSerialized]
	public string songSavePath;                    // Full path to which to save songs

	//-----------------------------------------------------------------------------------------------------------------
	[Header("UI Resources")]

	[Tooltip("Font to use for UI.")]
	public Font font;

	[Tooltip("Handwritten-style font to use for UI.")]
	public Font handwrittenFont;

	public Sprite arrowIcon;
	public Sprite addIcon;
	public Sprite editIcon;
	public Sprite playIcon;
	public Sprite pauseIcon;
	public Sprite loadIcon;
	public Sprite removeIcon;
	public Sprite circleIcon;
	public Sprite volumeIcon;
	public Sprite melodicVolumeIcon;
	public Sprite fillSprite;
	public Sprite scribbleCircle;

	[Tooltip("Sound to use when clicking a menu.")]
	public AudioClip menuClick;
	public AudioClip menuClick2;
	public AudioClip effectsOn;
	public AudioClip effectsOff;
	public List<AudioClip> scribbles;

	//-----------------------------------------------------------------------------------------------------------------
	[Header("Menu Objects")]

	public GameObject mainMenu;
	public GameObject playlistMenu;
	public GameObject keySelectMenu;
	public GameObject songArrangeMenu;
	public GameObject riffEditMenu;
	public GameObject postPlayMenu;
	public GameObject pauseMenu;

	public GameObject addRiffPrompt;            // "Add Riff"
	public GameObject loadPrompt;               // "Load Project"
	public GameObject prompt;                   // Generic pop-up prompt
	public GameObject confirmExitPrompt;        // "Would you like to exit..."

	public GameObject keySelectConfirmButton;

	public GameObject systemButtons;            // "Settings" and "Exit"
	public Image livePlayQuitPrompt;            // "Exit"
	public GameObject liveIcons;                // Parent of instrument icons
	public GameObject songProgressBar;
	public GameObject loopIcon;
	public GameObject cameraBlocker;
	public GameObject exitButton;

	//-----------------------------------------------------------------------------------------------------------------
	[Header("Tooltip Settings")]

	[Tooltip("GameObject to use for tooltip.")]
	public GameObject tooltip;  
	
	[Tooltip("Distance to show tooltip.")]                
	public float tooltipDistance;

	//-----------------------------------------------------------------------------------------------------------------
	Camera mainCamera;
	int cullingMaskBackup = 0;
	CameraClearFlags clearFlagsBackup = CameraClearFlags.Nothing;

	#endregion
	#region Unity Callbacks

	void Awake () {
		instance = this;

		// Remove profiler sample limit
		Profiler.maxNumberOfSamplesPerFrame = -1;

		// Set application target frame rate
		Application.targetFrameRate = 120;
		targetDeltaTime = 1f / (float)Application.targetFrameRate;

		// Init save paths
		projectSavePath = Application.dataPath + projectSaveFolder;
		songSavePath = Application.dataPath + songSaveFolder;

		// Create folders if non-existent
		if (!Directory.Exists(songSavePath))
			Directory.CreateDirectory (songSavePath);

		if (!Directory.Exists(projectSavePath))
			Directory.CreateDirectory (projectSavePath);

		casetteTarget = casetteBack;
		casettePosition = casetteBack;
	}

	void Start () {

		ShowAll();

		// Init camera ref
		mainCamera = Camera.main;

		// Stop 3D rendering while loading
		StopRendering ();

		// Hide menus
		HideAll();
		Hide (prompt);
	}

	void Update () {

		// Don't update if not loaded
		if (loading) return;

		// Start loading if not laoded
		if (!loaded) {
			HideAll();
			Load();
		}

		switch (currentState) {

		case State.Setup:
			
			// Check for tooltip
			if (tooltip.activeSelf) {
				RectTransform tr = tooltip.RectTransform();
				Vector2 realPosition = new Vector2 (
					Input.mousePosition.x / Screen.width * ((RectTransform)tr.parent).rect.width, 
					Input.mousePosition.y / Screen.height * ((RectTransform)tr.parent).rect.height
				);

				float w = tr.rect.width/2f;
				float h = tr.rect.height/2f;

				Vector3 pos = Vector3.zero;
				pos.z = Input.mousePosition.z;

				if (Input.mousePosition.x > Screen.width - w) pos.x = realPosition.x - w;
				else pos.x = realPosition.x + w;

				if (Input.mousePosition.y > Screen.height - w) pos.y = realPosition.y - h;
				else pos.y = realPosition.y + h;

				tr.anchoredPosition3D = pos;
			}
			break;

		case State.Live:
			if (!paused) {

				// Wake/fade UI icons
				Color temp = livePlayQuitPrompt.color;
				if (prevMouse != Input.mousePosition) {
					WakeLiveUI();
					prevMouse = Input.mousePosition;
				} else {
					if (fadeTimer <= 0f) temp.a -= fadeSpeed;
					else fadeTimer--;
					livePlayQuitPrompt.color = temp;
					foreach (Image image in liveIcons.GetComponentsInChildren<Image>()) {
						image.color = temp;
						if(image.GetComponentInChildren<Text>())
							image.GetComponentInChildren<Text>().color = temp;
					}
				}
			} else livePlayQuitPrompt.color = Color.white;
			break;
		}
			
		// Move casette
		if (casetteMoving) {
			if (progress < 1f) {

				progress += casetteMoveSpeed * Time.deltaTime;
			
				Vector3 pos = Vector3.Lerp (casettePosition.position, casetteTarget.position, progress);
				Quaternion rot = Quaternion.Lerp (casettePosition.rotation, casetteTarget.rotation, progress);
				casette.transform.position = pos;
				casette.transform.rotation = rot;

			} else casetteMoving = false;
		} else {
			casette.transform.position = casetteTarget.position;
			casette.transform.rotation = casetteTarget.rotation;
		}

	}

	#endregion
	#region GameManager Loading Methods

	/// <summary>
	/// Load this instance.
	/// </summary>
	void Load () {

		// Hide all menus
		HideAll ();

		// Show loading screen
		Show (loadingScreen);

		// Calculate operations to do
		loadsToDo = MusicManager.instance.loadsToDo + WorldManager.instance.loadsToDo;

		// Init vars
		startLoadTime = Time.realtimeSinceStartup;
		loading = true;

		// Start by loading MusicManager
		MusicManager.instance.Load();
	}

	/// <summary>
	/// Used to tell GameManager how many items were just loaded.
	/// </summary>
	/// <param name="numLoaded">Number loaded.</param>
	public void ReportLoaded (int numLoaded) {
		loadProgress += numLoaded;
		loadingBar.GetComponent<Slider>().value = (float)loadProgress/(float)loadsToDo;
	}

	/// <summary>
	/// Changes the message on the loading screen.
	/// </summary>
	/// <param name="message">Message.</param>
	public void ChangeLoadingMessage (string message) {
		loadingMessage.GetComponent<Text>().text = message;
	}

	/// <summary>
	/// Performs all necessary actions after loading.
	/// </summary>
	public void FinishLoading () {

		// Report to console
		Debug.Log("Fully loaded in "+(Time.realtimeSinceStartup-startLoadTime).ToString("0.0000")+" seconds.");

		// Update vars
		loading = false;
		loaded = true;

		// Change state
		currentState = State.Setup;

		SnapCasetteBack();

		// Hide all menus
		Hide (loadingScreen);
		Hide (cameraBlocker);
		HideAll ();

		// Show main menu
		Show (mainMenu);

		CameraControl.instance.SnapToView(CameraControl.instance.OutsideCar);

		// Begin 3D rendering again
		StartRendering ();
	}

	#endregion
	#region GameManager Menu Methods

	/// <summary>
	/// Show the specified menu, fading if possible.
	/// </summary>
	/// <param name="menu">Menu to show.</param>
	public void Show (GameObject menu) {
		menu.SetActive(true);
		Fadeable fade = menu.GetComponent<Fadeable>();
		if (fade != null) fade.UnFade();
	}

	/// <summary>
	/// Shows all menus, fading if possible.
	/// </summary>
	public void ShowAll () {
		Show (mainMenu);
		Show (playlistMenu);
		Show (keySelectMenu);
		Show (songArrangeMenu);
		Show (riffEditMenu);
		Show (postPlayMenu);

		Show (addRiffPrompt);
		Show (loadPrompt);
		Show (prompt);
		Show (liveIcons);
	}

	/// <summary>
	/// Hide the specified menu, fading if possible.
	/// </summary>
	/// <param name="menu">Menu to hide.</param>
	public void Hide (GameObject menu) {
		Fadeable fade = menu.GetComponent<Fadeable>();
		if (fade != null) fade.Fade();
	}

	/// <summary>
	/// Hides all menus, fading if possible.
	/// </summary>
	public void HideAll () {
		Hide (mainMenu);
		Hide (playlistMenu);
		Hide (keySelectMenu);
		Hide (songArrangeMenu);
		Hide (riffEditMenu);
		Hide (postPlayMenu);

		Hide (addRiffPrompt);
		Hide (loadPrompt);
		Hide (liveIcons);
		Hide (pauseMenu);
	}

	#endregion
	#region Menu Transition Methods
		
	/// <summary>
	/// Goes to main menu.
	/// </summary>
	public void GoToMainMenu () {

		currentState = State.Setup;
		
		// Hide other menus
		HideAll ();
		MoveCasetteBack();

		// Show main menu
		Show (mainMenu);

		// Move camera to outside view
		CameraControl.instance.LerpToView (CameraControl.instance.OutsideCar);
		CameraControl.instance.doSway = true;
	}

	/// <summary>
	/// Goes to key select menu.
	/// </summary>
	public void GoToKeySelectMenu () {

		currentState = State.Setup;

		// Hide other menus
		MoveCasetteBack();
		HideAll();

		// Show key select menu
		Show (keySelectMenu);

		// Move camera to driving view
		CameraControl.instance.LerpToView (CameraControl.instance.Driving);
		CameraControl.instance.doSway = true;

		// Refresh radial menu
		RadialKeyMenu.instance.Refresh();

		// Enable/disable confirmation button
		keySelectConfirmButton.GetComponent<Button>().interactable = 
			MusicManager.instance.currentSong.scale != -1 && MusicManager.instance.currentSong.key != Key.None;
		
	}

	/// <summary>
	/// Goes to song arrange menu.
	/// </summary>
	public void GoToSongArrangeMenu () {

		currentState = State.Setup;

		// Hide other menus
		MoveCasetteBack();
		HideAll ();

		// Show and refresh song arranger menu
		Show (songArrangeMenu);
		SongArrangeSetup.instance.Refresh();
		SongTimeline.instance.RefreshTimeline();

		// Move camera to radio view
		CameraControl.instance.LerpToView(CameraControl.instance.Radio);
		CameraControl.instance.doSway = false;
	}

	/// <summary>
	/// Goes to riff editor.
	/// </summary>
	public void GoToRiffEditor () {

		currentState = State.Setup;

		// Hide other menus
		MoveCasetteBack();
		HideAll ();

		// If no scale selected, go to key select first
		if (MusicManager.instance.currentSong.scale == -1) GoToKeySelectMenu();

		else {
			SongArrangeSetup.instance.UpdateValue();

			// Otherwise show riff editor
			Show (riffEditMenu);
			InstrumentSetup.instance.Initialize ();
			

			// Move camera to driving view
			CameraControl.instance.LerpToView (CameraControl.instance.Driving);
			CameraControl.instance.doSway = true;
		}
	}

	/// <summary>
	/// Goes to playlist menu.
	/// </summary>
	public void GoToPlaylistMenu () {

		// Switch modes
		currentState = State.Setup;

		// Stop music/live mode operations
		MusicManager.instance.StopPlaying();
		PlayerMovement.instance.StopMoving();
		CameraControl.instance.StopLiveMode();
		GameManager.instance.paused = false;

		// Hide other menus
		HideAll ();
		Hide (liveIcons);
		Hide (songProgressBar);

		// Show playlist menu
		Show (playlistMenu);
		PlaylistBrowser.instance.Refresh();
		PlaylistBrowser.instance.RefreshName();

		// Queue casette to move when done moving camera
		willMoveCasette = true;

		// Move camera to outside view
		CameraControl.instance.LerpToView (CameraControl.instance.OutsideCar);
		CameraControl.instance.doSway = true;
	}

	/// <summary>
	/// Goes to post play menu.
	/// </summary>
	public void GoToPostPlayMenu() {

		// Switch mode
		currentState = State.Postplay;

		// Hide other menus
		MoveCasetteBack();
		HideAll();

		// Show postplay menu
		Show (postPlayMenu);

		CameraControl.instance.doSway = true;
	}
		
	#endregion
	#region Mode Switching Methods

	/// <summary>
	/// Switches to live mode.
	/// </summary>
	public void SwitchToLive () {

		// Switch mode
		currentState = State.Live;
		paused = false;

		// Hide menus
		SnapCasetteBack();
		HideAll ();

		// Show live menus
		Show (liveIcons);
		Show (songProgressBar);
		if (MusicManager.instance.loopPlaylist) Show(loopIcon);
		else Hide(loopIcon);

		// Init music
		MusicManager.instance.currentPlayingSong = 0;
		MusicManager.instance.currentSong = (
			MusicManager.instance.currentProject.songs.Count > 0 ? MusicManager.instance.currentProject.songs[0] : null);
		if (MusicManager.instance.currentSong != null) {
			MusicManager.instance.StartPlaylist();
			MusicManager.instance.StartSong();
		}

		// Start live operations
		InstrumentDisplay.instance.Refresh();
		CameraControl.instance.StartLiveMode();
		PlayerMovement.instance.StartMoving();
	}

	/// <summary>
	/// Switches to postplay mode.
	/// </summary>
	public void SwitchToPostplay () {

		// Switch mode
		currentState = State.Postplay;
		paused = false;

		Hide (liveIcons);
		Hide (songProgressBar);

		// Stop music/live operations
		MusicManager.instance.StopPlaying();
		PlayerMovement.instance.StopMoving();
		CameraControl.instance.StopLiveMode();

		// Show prompt
		livePlayQuitPrompt.GetComponent<Image>().color = Color.white;

		// Go to postplay menu
		GoToPostPlayMenu();
	}

	/// <summary>
	/// Stops 3D rendering on the main camera.
	/// </summary>
	public void StopRendering () {
		cullingMaskBackup = mainCamera.cullingMask;
		clearFlagsBackup = mainCamera.clearFlags;
		mainCamera.cullingMask = 0;
		mainCamera.clearFlags = CameraClearFlags.Nothing;
		mainCamera.GetComponent<SunShafts> ().enabled = false;
		mainCamera.GetComponent<CameraMotionBlur> ().enabled = false;
		mainCamera.GetComponent<BloomOptimized> ().enabled = false;
	}

	/// <summary>
	/// Starts 3D rendering on the main camera
	/// </summary>
	public void StartRendering() {
		mainCamera.cullingMask = cullingMaskBackup;
		mainCamera.clearFlags = clearFlagsBackup;
		mainCamera.GetComponent<SunShafts> ().enabled = true;
		mainCamera.GetComponent<CameraMotionBlur> ().enabled = true;
		mainCamera.GetComponent<BloomOptimized> ().enabled = true;
	}

	#endregion
	#region Save/Load Methods

	/// <summary>
	/// Saves the current project.
	/// </summary>
	public void SaveCurrentProject () {
		SaveLoad.SaveCurrentProject();
	}

	/// <summary>
	/// Shows the load prompt for projects.
	/// </summary>
	public void ShowLoadPromptForProjects () {
		LoadPrompt.instance.Refresh(LoadPrompt.Mode.Project);
		Show (loadPrompt);
	}

	/// <summary>
	/// Shows the load prompt for songs.
	/// </summary>
	public void ShowLoadPromptForSongs () {
		LoadPrompt.instance.Refresh(LoadPrompt.Mode.Song);
		Show (loadPrompt);
	}

	#endregion
	#region Utility Methods

	/// <summary>
	/// Moves the casette front.
	/// </summary>
	public void MoveCasetteFront () {
		casetteMoving = true;
		casettePosition = casette.transform;
		casetteTarget = casetteFront;
		progress = 0f;
		willMoveCasette = false;
	}

	/// <summary>
	/// Moves the casette back.
	/// </summary>
	public void MoveCasetteBack () {
		casetteMoving = true;
		casettePosition = casette.transform;
		casetteTarget = casetteBack;
		progress = 0f;
		willMoveCasette = false;
	}

	public void SnapCasetteBack () {
		casetteMoving = false;
		casettePosition = casetteBack;
		casetteTarget = casetteBack;
		willMoveCasette = false;
	}

	/// <summary>
	/// If set to move casette, will do so.
	/// </summary>
	public void AttemptMoveCasette () {
		if (willMoveCasette) MoveCasetteFront();
	}

	/// <summary>
	/// Wakes the live UI.
	/// </summary>
	public void WakeLiveUI () {
		fadeTimer = fadeWaitTime;
		Color color = Color.white;
		color.a = 1f;
		livePlayQuitPrompt.color = color;
		foreach (Image image in liveIcons.GetComponentsInChildren<Image>()) {
			image.color = color;
			if (image.GetComponentInChildren<Text>())
				image.GetComponentInChildren<Text>().color = color;
		}
	}

	/// <summary>
	/// Attempts to exit the GameManager.instance.
	/// </summary>
	public void AttemptExit () {
		switch (currentState) {
		case State.Loading:  case State.Setup: case State.Postplay:
			Show(confirmExitPrompt);
			break;
		case State.Live:
			if (paused) Show(confirmExitPrompt);
			else Pause();
			break;
		}
	}

	/// <summary>
	/// Plays a click noise.
	/// </summary>
	public void MenuClick () {
		MusicManager.PlayMenuSound (menuClick);
	}

	/// <summary>
	/// Plays an alternate click noise.
	/// </summary>
	public void MenuClick2 () {
		MusicManager.PlayMenuSound (menuClick2);
	}

	/// <summary>
	/// Plays an effects on noise.
	/// </summary>
	public void EffectsOn () {
		MusicManager.PlayMenuSound (effectsOn);
	}

	/// <summary>
	/// Plays an effects off noise.
	/// </summary>
	public void EffectsOff () {
		MusicManager.PlayMenuSound (effectsOff);
	}

	/// <summary>
	/// Plays a random pen scribble sound.
	/// </summary>
	public void Scribble () {
		MusicManager.PlayMenuSound (scribbles.Random(), 0.75f);
	}

	/// <summary>
	/// Toggles paused status.
	/// </summary>
	public void TogglePause () {
		if (paused) Unpause();
		else Pause();
	}

	/// <summary>
	/// Pause this instance.
	/// </summary>
	public void Pause () {
		paused = true;
		Show(pauseMenu);
		PlayerMovement.instance.StopMoving();
		CameraControl.instance.Pause();
	}

	/// <summary>
	/// Unpause this instance.
	/// </summary>
	public void Unpause () {
		paused = false;
		Hide (pauseMenu);
		PlayerMovement.instance.StartMoving();
		CameraControl.instance.Unpause();
	}

	/// <summary>
	/// Use this to prevent debug statement spam.
	/// </summary>
	/// <returns></returns>
	public static bool IsDebugFrame () {
		return (Time.frameCount % 100 == 1);
	}

	/// <summary>
	/// Exit this instance.
	/// </summary>
	public void Exit () {
		Application.Quit();
	}

	#endregion
}
