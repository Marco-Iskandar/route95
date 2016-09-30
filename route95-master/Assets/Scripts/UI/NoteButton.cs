using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// A special use of DraggableButton for the riff editor not buttons.
/// </summary>
public class NoteButton : DraggableButton {

	#region NoteButton Vars

	const int notesVisible = 10;
	const float vDragDistance = 128f;
	const float hDragDistance = 128f;

	public Note targetNote;
	public Image volumeImage;

	float oldVolume;
	#endregion
	#region Unity Callbacks

	void Awake () {
		maxDragDistanceUp = vDragDistance;
		maxDragDistanceDown = vDragDistance;
		maxDragDistanceLeft = hDragDistance;
		maxDragDistanceRight = hDragDistance;

		//dragThreshold = 25f;
	}

	#endregion
	#region DraggableButton Overrides

	public override void OnMouseDown() {
		oldVolume = targetNote.volume;
	}


	public override void DragDown (float actionRatio) {
		targetNote.volume = Mathf.Clamp01 (oldVolume - actionRatio);
		UpdateButtonArt();
	}

	public override void DragUp (float actionRatio) {
		targetNote.volume = Mathf.Clamp01 (oldVolume + actionRatio);
		UpdateButtonArt();
	}

	public override void DragLeft (float actionRatio) {
		targetNote.duration = 1f;
	}

	public override void DragRight (float actionRatio) {
		targetNote.duration = 1 + (float)(notesVisible-1) * actionRatio;
	}

	#endregion
	#region DraggableButton Methods

	/// <summary>
	/// Updates the button's fill amount and color.
	/// </summary>
	public void UpdateButtonArt() {
		volumeImage.fillAmount = targetNote.volume;
		volumeImage.color = new Color (0.75f*targetNote.volume + 0.25f, 0.25f, 0.25f, 1f);
	}

	#endregion
}
