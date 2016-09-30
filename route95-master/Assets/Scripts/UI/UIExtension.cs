using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Class for UI object extensions.
/// </summary>
public static class UIExtension {

	#region RectTransform Extensions

	/// <summary>
	/// Anchors at point.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public static void AnchorAtPoint (this RectTransform tr, float x, float y) {
		Vector2 anchorPoint = new Vector2 (x, y);
		tr.anchorMax = anchorPoint;
		tr.anchorMin = anchorPoint;
	}

	/// <summary>
	/// Anchors at point.
	/// </summary>
	/// <param name="anchorPoint">Anchor point.</param>
	public static void AnchorAtPoint (this RectTransform tr, Vector2 anchorPoint) {
		tr.anchorMax = anchorPoint;
		tr.anchorMin = anchorPoint;
	}

	/// <summary>
	/// Resets local scale and rotation to defaults.
	/// </summary>
	/// <param name="tr"></param>
	public static void ResetScaleRot (this RectTransform tr) {
		tr.localScale = Vector3.one;
		tr.localRotation = Quaternion.Euler(Vector3.zero);
	}

	#endregion
	#region MaskableGraphic Extensions

	public static void SetAlpha (this MaskableGraphic gr, float alpha) {
		Color color = gr.color;
		color.a = alpha;
		gr.color = color;
	}

	#endregion
}