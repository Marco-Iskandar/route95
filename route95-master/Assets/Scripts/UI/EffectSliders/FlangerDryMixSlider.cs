using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FlangerDryMixSlider : EffectSlider {

	public override void Initialize () {
		UpdateSlider ((InstrumentSetup.currentRiff.flangerDryMix + 1f) / 2f);
	}

	public override void ChangeValue () {
		InstrumentSetup.currentRiff.flangerDryMix = slider.value * 2f - 1f;
	}

}