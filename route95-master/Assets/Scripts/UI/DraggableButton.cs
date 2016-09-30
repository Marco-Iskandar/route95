using UnityEngine;
using System.Collections;

/// <summary>
/// Enables a button to be draggable.
/// </summary>
public class DraggableButton : MonoBehaviour {

	#region DraggableButton Vars

	public float maxDragDistanceUp;    // Maxmimum drag distance upwards
	public float maxDragDistanceDown;  // Maxmimum drag distance downwards
	public float maxDragDistanceLeft;  // Maxmimum drag distance to the left
	public float maxDragDistanceRight; // Maxmimum drag distance to the right

	const float dragBias = 1.25f;      // Threshold for drag to recognize one direction over another

	#endregion
	#region DraggableButton Methods

	/// <summary>
	/// Calls the appropriate function based on drag direction.
	/// </summary>
	/// <param name="dragVector">Vector of mouse dragging.</param>
	public void Drag (Vector3 dragVector) {
		float hDrag = Mathf.Abs(dragVector.x);
		float yDrag = Mathf.Abs(dragVector.y);

		if (dragVector.x < 0f) DragLeft (Mathf.Clamp01(hDrag/maxDragDistanceLeft));
		else DragRight (Mathf.Clamp01(hDrag/maxDragDistanceRight));
		if (dragVector.y < 0f) DragDown (Mathf.Clamp01(yDrag/maxDragDistanceDown));
		else DragUp (Mathf.Clamp01(yDrag/maxDragDistanceUp));

	}

	public virtual void OnMouseDown () {}
	public virtual void OnMouseUp () {}

	public virtual void DragLeft (float actionRatio) {}
	public virtual void DragRight (float actionRatio) {}
	public virtual void DragDown (float actionRatio) {}
	public virtual void DragUp (float actionRation) {}

	#endregion
}
