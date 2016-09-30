using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChorusDryMixSlider : EffectSlider {

	public override void Initialize () {
		UpdateSlider (InstrumentSetup.currentRiff.chorusDryMix);
	}

	public override void ChangeValue () {
		InstrumentSetup.currentRiff.chorusDryMix = slider.value;
	}

}