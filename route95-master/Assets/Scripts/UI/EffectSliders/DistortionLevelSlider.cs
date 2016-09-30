using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DistortionLevelSlider : EffectSlider {

	public override void Initialize () {
		UpdateSlider (InstrumentSetup.currentRiff.distortionLevel);
	}

	public override void ChangeValue () {
		InstrumentSetup.currentRiff.distortionLevel = slider.value;
	}

}