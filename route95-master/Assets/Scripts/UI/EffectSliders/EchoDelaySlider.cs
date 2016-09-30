using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EchoDelaySlider : EffectSlider {

	float min = 10f;
	float max = 1500f;

	public override void Initialize () {
		UpdateSlider ((InstrumentSetup.currentRiff.echoDelay - min) / (max- min));
	}

	public override void ChangeValue () {
		float val = (max-min) * slider.value + min;
		Debug.Log("changevalue " + val);
		InstrumentSetup.currentRiff.echoDelay = val;
	}

}