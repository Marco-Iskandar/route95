using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class that handles transition effects of UI objects.
/// </summary>
public class ShowHide : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	#region ShowHide Enums

	/// <summary>
	/// Type of transition.
	/// </summary>
	public enum TransitionType {
		Instant,
		Fade
	}

	#endregion
	#region ShowHide Vars

	public List<GameObject> objects;       // Objects to show/hide
	public TransitionType transitionType = // Type of transition
		TransitionType.Instant;

	public float fadeSpeed;                // Speed of fade effect
	List<IEnumerator> activeFades;         // All active fade operations

	#endregion
	#region ShowHide Methods

	/// <summary>
	/// Shows the object based on the transition type.
	/// </summary>
	public void Show() {
		foreach (GameObject obj in objects) {
			switch (transitionType) {
			case TransitionType.Instant:
				obj.SetActive(true);
				break;
			case TransitionType.Fade:
				if (activeFades == null) activeFades = new List<IEnumerator>();
				IEnumerator temp = Fade(obj);
				activeFades.Add(temp);
				StartCoroutine (temp);
				break;
			}
		}
	}

	/// <summary>
	/// Hides the object based on the transition type.
	/// </summary>
	public void Hide () {
		foreach (GameObject obj in objects) {
			switch (transitionType) {
			case TransitionType.Instant:
				obj.SetActive(false);
				break;
			case TransitionType.Fade:
				foreach (IEnumerator fade in activeFades)
					StopCoroutine (fade);
				activeFades.Clear();
				break;
			}
		}
	}

	/// <summary>
	/// Called when pointer first is over object.
	/// </summary>
	/// <param name="eventData"></param>
	public void OnPointerEnter (PointerEventData eventData) {
		if (objects != null && InputManager.instance.selected == null) {
			Show();
		}
	}

	/// <summary>
	/// Called when pointer leaves object.
	/// </summary>
	/// <param name="eventData"></param>
	public void OnPointerExit (PointerEventData eventData) {
		if (objects != null && InputManager.instance.selected == null) {
			Hide();
		}
	}

	/// <summary>
	/// Fades target image.
	/// </summary>
	/// <param name="target"></param>
	/// <returns></returns>
	public IEnumerator Fade (GameObject target) {
		for (float a = 0f; a < 1.0f; a += fadeSpeed) {
			target.GetComponent<Image>().color = new Color (1, 1, 1f, a);
			yield return null;
		}
	}

	#endregion
}
