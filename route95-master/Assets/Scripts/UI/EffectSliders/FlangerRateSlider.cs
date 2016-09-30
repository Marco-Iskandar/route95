using UnityEngine;
using System.Collections;

public class FlangerRateSlider : EffectSlider {

	public override void Initialize () {
		UpdateSlider (InstrumentSetup.currentRiff.flangerRate / (Mathf.PI*32f) - Mathf.PI/32f);
	}

	public override void ChangeValue () {
		InstrumentSetup.currentRiff.flangerDryMix = Mathf.PI/32f + Mathf.PI/32f * slider.value;
	}
}
