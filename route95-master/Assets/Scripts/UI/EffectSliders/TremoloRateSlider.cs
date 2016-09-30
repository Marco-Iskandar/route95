using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TremoloRateSlider : EffectSlider {

	public override void Initialize () {
		UpdateSlider (InstrumentSetup.currentRiff.tremoloRate - (Mathf.PI/32f) / (Mathf.PI/32f));
	}

	public override void ChangeValue () {
		InstrumentSetup.currentRiff.tremoloRate = (Mathf.PI/32f) + slider.value * (Mathf.PI/32f);
	}

}