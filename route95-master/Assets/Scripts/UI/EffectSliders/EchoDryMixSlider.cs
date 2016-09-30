using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EchoDryMixSlider : EffectSlider {

	public override void Initialize () {
		UpdateSlider (InstrumentSetup.currentRiff.echoDryMix);
	}

	public override void ChangeValue () {
		InstrumentSetup.currentRiff.echoDryMix = slider.value;
	}

}