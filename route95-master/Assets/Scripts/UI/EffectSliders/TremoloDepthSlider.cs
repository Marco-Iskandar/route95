using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TremoloDepthSlider : EffectSlider {

	public override void Initialize () {
		UpdateSlider (InstrumentSetup.currentRiff.tremoloDepth);
	}

	public override void ChangeValue () {
		InstrumentSetup.currentRiff.tremoloDepth = slider.value;
	}

}