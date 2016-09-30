using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EchoDecaySlider : EffectSlider {

	public override void Initialize () {
		UpdateSlider (InstrumentSetup.currentRiff.echoDecayRatio / 0.99f);
	}

	public override void ChangeValue () {
		InstrumentSetup.currentRiff.echoDecayRatio = slider.value * 0.99f;
	}

}