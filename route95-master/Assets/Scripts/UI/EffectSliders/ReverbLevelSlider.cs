using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReverbLevelSlider : EffectSlider {

	public override void Initialize () {
		UpdateSlider (InstrumentSetup.currentRiff.reverbLevel / 2000f);
	}

	public override void ChangeValue () {
		InstrumentSetup.currentRiff.reverbLevel = slider.value * 2000f;
	}

}