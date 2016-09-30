using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class to handle the song arrange menu
/// </summary>
public class SongArrangeSetup : MonoBehaviour {

	#region SongArrangeSetup Vars

	public static SongArrangeSetup instance; // Quick reference to this instance

	[Tooltip("Index of currently selected riff.")]
	public int selectedRiffIndex;

	[Tooltip("Riff selection dropdown.")]
	public Dropdown dropdown;

	[Tooltip("Song name input field.")]
	public InputField songNameInputField;

	[Tooltip("Play riff button.")]
	public GameObject playRiffButton;

	[Tooltip("Edit riff button.")]
	public GameObject editRiffButton;

	[Tooltip("Add riff reminder prompt.")]
	public GameObject addRiffReminder;

	public GameObject previewSongButton;

	#endregion
	#region Unity Callbacks

	void Awake () {
		instance = this;

		// Set song name input field functionality
		songNameInputField.onEndEdit.AddListener (delegate { MusicManager.instance.currentSong.name = songNameInputField.text;});
	}

	#endregion
	#region SongArrangeSetup Methods

	/// <summary>
	/// Refreshes all elements on the song arranger UI.
	/// </summary>
	public void Refresh () {

		// Update the options in the dropdown to include all riffs
		dropdown.ClearOptions ();
		List<Dropdown.OptionData> options = new List<Dropdown.OptionData> ();

		foreach (Riff riff in MusicManager.instance.currentSong.riffs) {
			Sprite sprite = riff.instrument.icon;
			Dropdown.OptionData option = new Dropdown.OptionData (riff.name, sprite);
			options.Add (option);
		}
		dropdown.AddOptions (options);

		if (MusicManager.instance.currentSong.riffs.Count == 0) {
			dropdown.interactable = false;
			editRiffButton.Button().interactable = false;
			playRiffButton.Button().interactable = false;
			previewSongButton.Button().interactable = false;
		} else {
			dropdown.interactable = true;
			editRiffButton.Button().interactable = true;
			playRiffButton.Button().interactable = true;
			previewSongButton.Button().interactable = true;
			if (InstrumentSetup.currentRiff == null)
				InstrumentSetup.currentRiff = MusicManager.instance.currentSong.riffs [0];
		}

		dropdown.value = selectedRiffIndex;

		// Refresh song name input field
		songNameInputField.text = MusicManager.instance.currentSong.name;

		// Update play riff button art
		playRiffButton.Image().sprite = GameManager.instance.playIcon;

		bool hasRiffs = MusicManager.instance.currentSong.riffs.Count != 0;
		SongTimeline.instance.SetInteractable (hasRiffs);
		addRiffReminder.SetActive(!hasRiffs);

	}

	/// <summary>
	/// Sets the selected riff from the dropdown.
	/// </summary>
	public void UpdateValue () {
		selectedRiffIndex = dropdown.value;
		InstrumentSetup.currentRiff = MusicManager.instance.currentSong.riffs[selectedRiffIndex];
	}

	/// <summary>
	/// Sets the dropdown value from the selected riff.
	/// </summary>
	public void SetValue () {
		SetValue (selectedRiffIndex);
	}

	public void SetValue (int i) {
		dropdown.value = i;
	}

	/// <summary>
	/// Hide the dropdown.
	/// </summary>
	public void HideDropdown () {
		dropdown.Hide();
	}

	/// <summary>
	/// Updates the play riff button art.
	/// </summary>
	public void TogglePlayRiffButton () {
		if (MusicManager.instance.playing && MusicManager.instance.riffMode) playRiffButton.Image().sprite = GameManager.instance.pauseIcon;
		else playRiffButton.Image().sprite = GameManager.instance.playIcon;
	}

	public void TogglePlaySongButton () {
		if (MusicManager.instance.playing && !MusicManager.instance.riffMode) previewSongButton.Image().sprite = GameManager.instance.pauseIcon;
		else previewSongButton.Image().sprite = GameManager.instance.playIcon;
	}

	#endregion
}
