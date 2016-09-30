using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Audio;

/// <summary>
/// Class to handle effect sliders and buttons.
/// </summary>
public class EffectButton : MonoBehaviour {

	public Image image;

	/// <summary>
	/// Toggles the active status of the distortion filter.
	/// </summary>
	public void ToggleDistortion () {
		Riff riff = InstrumentSetup.currentRiff;
		Instrument inst = riff.instrument;
		GameObject source = MusicManager.instance.instrumentAudioSources [inst].gameObject;

		// Toggle distortion
		AudioDistortionFilter distortion = source.GetComponent<AudioDistortionFilter>();
		distortion.enabled = !distortion.enabled;
		riff.distortionEnabled = distortion.enabled;

		// Update button art
		if (image == null) Debug.Log("shit");
		image.sprite = (distortion.enabled ? 
			InstrumentSetup.instance.percussionFilled : InstrumentSetup.instance.percussionEmpty);

		// Play sound
		if (distortion.enabled) GameManager.instance.EffectsOn();
		else GameManager.instance.EffectsOff();
	}

	/// <summary>
	///  Toggles the active status of the echo filter.
	/// </summary>
	public void ToggleEcho () {
		Riff riff = InstrumentSetup.currentRiff;
		Instrument inst = riff.instrument;
		GameObject source = MusicManager.instance.instrumentAudioSources [inst].gameObject;

		// Toggle echo
		AudioEchoFilter echo = source.GetComponent<AudioEchoFilter>();
		echo.enabled = !echo.enabled;
		riff.echoEnabled = echo.enabled;

		// Update button art
		image.sprite = (echo.enabled ? 
			InstrumentSetup.instance.percussionFilled : InstrumentSetup.instance.percussionEmpty);

		// Play sound
		if (echo.enabled) GameManager.instance.EffectsOn();
		else GameManager.instance.EffectsOff();
	}

	/// <summary>
	/// Toggles the active status of the reverb filter.
	/// </summary>
	public void ToggleReverb () {
		Riff riff = InstrumentSetup.currentRiff;
		Instrument inst = riff.instrument;
		GameObject source = MusicManager.instance.instrumentAudioSources [inst].gameObject;

		// Toggle reverb
		AudioReverbFilter reverb = source.GetComponent<AudioReverbFilter>();
		reverb.enabled = !reverb.enabled;
		riff.reverbEnabled = reverb.enabled;
		
		// Update button art
		image.sprite = (reverb.enabled ? 
			InstrumentSetup.instance.percussionFilled : InstrumentSetup.instance.percussionEmpty);

		// Play sound
		if (reverb.enabled) GameManager.instance.EffectsOn();
		else GameManager.instance.EffectsOff();
	}

	/// <summary>
	/// Toggles the active status of the tremolo filter.
	/// </summary>
	public void ToggleTremolo () {
		Riff riff = InstrumentSetup.currentRiff;
		Instrument inst = riff.instrument;
		GameObject source = MusicManager.instance.instrumentAudioSources [inst].gameObject;

		// Toggle tremolo
		AudioTremoloFilter tremolo = source.GetComponent<AudioTremoloFilter>();
		tremolo.enabled = !tremolo.enabled;
		riff.tremoloEnabled = tremolo.enabled;

		// Update button art
		image.sprite = (tremolo.enabled ? 
			InstrumentSetup.instance.percussionFilled : InstrumentSetup.instance.percussionEmpty);

		// Play sound
		if (tremolo.enabled) GameManager.instance.EffectsOn();
		else GameManager.instance.EffectsOff();
	}

	/// <summary>
	/// Toggles the active status of the chorus filter.
	/// </summary>
	public void ToggleChorus () {
		Riff riff = InstrumentSetup.currentRiff;
		Instrument inst = riff.instrument;
		GameObject source = MusicManager.instance.instrumentAudioSources [inst].gameObject;

		// Toggle chorus
		AudioChorusFilter chorus = source.GetComponent<AudioChorusFilter>();
		chorus.enabled = !chorus.enabled;
		riff.chorusEnabled = chorus.enabled;

		// Update button art
		image.sprite = (chorus.enabled ? 
			InstrumentSetup.instance.percussionFilled : InstrumentSetup.instance.percussionEmpty);

		// Play sound
		if (chorus.enabled) GameManager.instance.EffectsOn();
		else GameManager.instance.EffectsOff();
	}

	/// <summary>
	/// Toggles the active status of the flanger filter.
	/// </summary>
	public void ToggleFlanger () {
		Riff riff = InstrumentSetup.currentRiff;
		Instrument inst = riff.instrument;
		GameObject source = MusicManager.instance.instrumentAudioSources [inst].gameObject;

		// Toggle flanger
		AudioFlangerFilter flanger = source.GetComponent<AudioFlangerFilter>();
		flanger.enabled = !flanger.enabled;
		riff.flangerEnabled = flanger.enabled;

		// Update button art
		image.sprite = (flanger.enabled ? 
			InstrumentSetup.instance.percussionFilled : InstrumentSetup.instance.percussionEmpty);

		// Play sound
		if (flanger.enabled) GameManager.instance.EffectsOn();
		else GameManager.instance.EffectsOff();
	}

	public void updatedistortionLevel(Slider slider){
		AudioSource source = MusicManager.instance.instrumentAudioSources [InstrumentSetup.currentRiff.instrument];
		InstrumentSetup.currentRiff.distortionLevel = slider.value;
		source.gameObject.GetComponent<AudioDistortionFilter>().distortionLevel = InstrumentSetup.currentRiff.distortionLevel;
		//instrumentAudioSources[MelodicInstrument.ElectricGuitar].gameObject.GetComponent<AudioDistortionFilter>().distortionLevel = 0.9f;

		Riff riff = InstrumentSetup.currentRiff;
		if (!riff.distortionEnabled && InstrumentSetup.instance.initialized) ToggleDistortion();
	}

	public void updateechoDecayRatio(Slider slider){
		AudioSource source = MusicManager.instance.instrumentAudioSources [InstrumentSetup.currentRiff.instrument];
		InstrumentSetup.currentRiff.echoDecayRatio = slider.value;
		source.gameObject.GetComponent<AudioEchoFilter>().decayRatio = InstrumentSetup.currentRiff.echoDecayRatio;

		Riff riff = InstrumentSetup.currentRiff;
		if (!riff.echoEnabled && InstrumentSetup.instance.initialized) ToggleEcho();
	}


	public void updateechoDelay(Slider slider){
		AudioSource source = MusicManager.instance.instrumentAudioSources [InstrumentSetup.currentRiff.instrument];
		InstrumentSetup.currentRiff.echoDelay = slider.value;
		source.gameObject.GetComponent<AudioEchoFilter>().delay = InstrumentSetup.currentRiff.echoDelay;		
		
		Riff riff = InstrumentSetup.currentRiff;
		if (!riff.echoEnabled && InstrumentSetup.instance.initialized) ToggleEcho();	
	}


	public void updateechoDryMix(Slider slider){
		AudioSource source = MusicManager.instance.instrumentAudioSources [InstrumentSetup.currentRiff.instrument];
		InstrumentSetup.currentRiff.echoDryMix = slider.value;
		source.gameObject.GetComponent<AudioEchoFilter>().dryMix = InstrumentSetup.currentRiff.echoDryMix;	
		
		Riff riff = InstrumentSetup.currentRiff;
		if (!riff.echoEnabled && InstrumentSetup.instance.initialized) ToggleEcho();	
	}

	public void updatereverbDecayTime(Slider slider){
		AudioSource source = MusicManager.instance.instrumentAudioSources [InstrumentSetup.currentRiff.instrument];
		InstrumentSetup.currentRiff.reverbDecayTime = slider.value;
		source.gameObject.GetComponent<AudioReverbFilter>().decayTime = InstrumentSetup.currentRiff.reverbDecayTime;	
		
		Riff riff = InstrumentSetup.currentRiff;
		if (!riff.reverbEnabled && InstrumentSetup.instance.initialized) ToggleReverb();	
	}


	public void updatereverbLevel(Slider slider){
		AudioSource source = MusicManager.instance.instrumentAudioSources [InstrumentSetup.currentRiff.instrument];
		InstrumentSetup.currentRiff.reverbLevel = slider.value;
		source.gameObject.GetComponent<AudioReverbFilter>().reverbLevel = InstrumentSetup.currentRiff.reverbLevel;	

		Riff riff = InstrumentSetup.currentRiff;
		if (!riff.reverbEnabled && InstrumentSetup.instance.initialized) ToggleReverb();
	}

	public void updatetremoloRate(Slider slider){
		AudioSource source = MusicManager.instance.instrumentAudioSources [InstrumentSetup.currentRiff.instrument];
		InstrumentSetup.currentRiff.tremoloRate = slider.value;		
		source.gameObject.GetComponent<AudioTremoloFilter>().rate = InstrumentSetup.currentRiff.tremoloRate;

		Riff riff = InstrumentSetup.currentRiff;
		if (!riff.tremoloEnabled && InstrumentSetup.instance.initialized) ToggleTremolo();
	}

	public void updatetremoloDepth(Slider slider){
		AudioSource source = MusicManager.instance.instrumentAudioSources [InstrumentSetup.currentRiff.instrument];
		InstrumentSetup.currentRiff.tremoloDepth = slider.value;		
		source.gameObject.GetComponent<AudioTremoloFilter>().depth = InstrumentSetup.currentRiff.tremoloDepth;

		Riff riff = InstrumentSetup.currentRiff;
		if (!riff.tremoloEnabled && InstrumentSetup.instance.initialized) ToggleTremolo();
	}

	public void updatechorusDryMix(Slider slider){
		AudioSource source = MusicManager.instance.instrumentAudioSources [InstrumentSetup.currentRiff.instrument];
		InstrumentSetup.currentRiff.chorusDryMix = slider.value;
		source.gameObject.GetComponent<AudioChorusFilter>().dryMix = InstrumentSetup.currentRiff.chorusDryMix;	
		
		Riff riff = InstrumentSetup.currentRiff;
		if (!riff.chorusEnabled && InstrumentSetup.instance.initialized) ToggleChorus();	
	}

	public void updatechorusRate(Slider slider){
		AudioSource source = MusicManager.instance.instrumentAudioSources [InstrumentSetup.currentRiff.instrument];
		InstrumentSetup.currentRiff.chorusRate = slider.value;
		source.gameObject.GetComponent<AudioChorusFilter>().rate = InstrumentSetup.currentRiff.chorusRate;

		Riff riff = InstrumentSetup.currentRiff;
		if (!riff.chorusEnabled && InstrumentSetup.instance.initialized) ToggleChorus();
	}

	public void updatechorusDepth(Slider slider){
		AudioSource source = MusicManager.instance.instrumentAudioSources [InstrumentSetup.currentRiff.instrument];
		InstrumentSetup.currentRiff.chorusDepth = slider.value;
		source.gameObject.GetComponent<AudioChorusFilter>().depth = InstrumentSetup.currentRiff.chorusDepth;

		Riff riff = InstrumentSetup.currentRiff;
		if (!riff.chorusEnabled && InstrumentSetup.instance.initialized) ToggleChorus();
	}

	public void updateflangerRate(Slider slider){
		AudioSource source = MusicManager.instance.instrumentAudioSources [InstrumentSetup.currentRiff.instrument];
		InstrumentSetup.currentRiff.flangerRate = Mathf.PI/32f + Mathf.PI/32f * slider.value;
		source.gameObject.GetComponent<AudioFlangerFilter>().rate = InstrumentSetup.currentRiff.flangerRate;

		Riff riff = InstrumentSetup.currentRiff;
		if (!riff.flangerEnabled && InstrumentSetup.instance.initialized) ToggleFlanger();
	}
		

	public void updateflangerDryMix(Slider slider){
		AudioSource source = MusicManager.instance.instrumentAudioSources [InstrumentSetup.currentRiff.instrument];
		InstrumentSetup.currentRiff.flangerDryMix = slider.value;
		source.gameObject.GetComponent<AudioFlangerFilter>().dryMix = InstrumentSetup.currentRiff.flangerDryMix;

		Riff riff = InstrumentSetup.currentRiff;
		if (!riff.flangerEnabled && InstrumentSetup.instance.initialized) ToggleFlanger();
	}
}

