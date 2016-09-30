using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChorusDepthSlider : EffectSlider {

	public override void Initialize () {
		UpdateSlider (InstrumentSetup.currentRiff.chorusDepth);
	}

	public override void ChangeValue () {
		InstrumentSetup.currentRiff.chorusDepth = slider.value;
	}

}