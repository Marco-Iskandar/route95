using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChorusRateSlider : EffectSlider {

	public override void Initialize () {
		UpdateSlider (InstrumentSetup.currentRiff.chorusRate);
	}

	public override void ChangeValue () {
		InstrumentSetup.currentRiff.chorusRate = slider.value;
	}

}