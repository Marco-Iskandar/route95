using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EffectSlider : MonoBehaviour {

	public Slider slider;

	public virtual void Initialize () {} // what to do when started
	public virtual void ChangeValue () {} // if the slider is moved, what do?
	public void UpdateSlider (float value) {
		slider.value = value;
	}

}


