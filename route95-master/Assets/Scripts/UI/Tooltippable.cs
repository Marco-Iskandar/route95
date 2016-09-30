using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Class to enable a UI object to have a tooltip displayed for it.
/// </summary>
public class Tooltippable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

	[Tooltip("Message to show when hovered.")]
	public string message;

	public void OnPointerEnter (PointerEventData e) {
		GameManager.instance.Show(Tooltip.instance.gameObject);
		Tooltip.instance.SetText (message);
	}

	public void OnPointerExit (PointerEventData e) {
		GameManager.instance.Hide(Tooltip.instance.gameObject);
	}

	public void OnPointerClick (PointerEventData e) {
		GameManager.instance.Hide(Tooltip.instance.gameObject);
	}
}
